using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Search;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.DependencyInjection;
using Litium.Search;
using Litium.Web;
using Litium.Web.Customers.TargetGroups;
using Litium.Websites;
using Nest;

namespace Litium.Accelerator.Search.Searching
{
    [ServiceDecorator(typeof(PageSearchService))]
    internal class PageSearchServiceDecorator : PageSearchService
    {
        private readonly PageSearchService _parent;
        private readonly SearchClientService _searchClientService;
        private readonly PageService _pageService;
        private readonly UrlService _urlService;
        private readonly Func<BoolQueryDescriptor<PageDocument>, IBoolQuery> _permissionDescriptor;

        public PageSearchServiceDecorator(
            PageSearchService parent,
            SearchClientService searchClientService,
            PageService pageService,
            UrlService urlService,
            SearchPermissionService searchPermissionService,
            TargetGroupEngine targetGroupEngine)
            : base(targetGroupEngine)
        {
            _parent = parent;
            _searchClientService = searchClientService;
            _pageService = pageService;
            _urlService = urlService;
            _permissionDescriptor = b => b.Should(searchPermissionService.GetPermissions().Select<string, Func<QueryContainerDescriptor<PageDocument>, QueryContainer>>(x =>
                s => s.Term(t => t.Field(p => p.Blocks.First().Permissions).Value(x))));
        }

        public override async Task<SearchResult> SearchAsync(SearchQuery searchQuery, bool? onlyBrands = null, bool includeScore = false)
        {
            if (!_searchClientService.IsConfigured)
            {
                return await _parent.SearchAsync(searchQuery, onlyBrands);
            }

            var response = await _searchClientService
                .SearchAsync<PageDocument>(CultureInfo.CurrentUICulture, selector => selector
                     .From((searchQuery.PageNumber - 1) * searchQuery.PageSize.GetValueOrDefault(10000))
                     .Size(searchQuery.PageSize)
                     .QueryWithPermission(q =>
                        q.PublishedOnChannel()
                        && (!string.IsNullOrEmpty(searchQuery.PageType)
                                ? SearchByPageType(q, searchQuery) && q.PublishedOnWebsite()
                                : !string.IsNullOrEmpty(searchQuery.Text)
                                        ? SearchInBrands(q, onlyBrands) && q.PublishedOnWebsite() && SearchByText(q, searchQuery.Text, Fuzziness.Auto)
                                        : q.PublishedOnWebsite()))
                     .Sort(x => searchQuery.PageType == PageTemplateNameConstants.NewsList ? x.Descending(f => f.NewsDate) : null)
                     .Source(x => x.Includes(z => z.Fields(p => p.PageSystemId))));

            return Transform(searchQuery, unchecked((int)response.Total), () =>
            {
                var pages = _pageService.Get(response.Hits.Select(x => x.Source.PageSystemId));
                var pageLookup = includeScore ? response.Hits.ToDictionary(x => x.Source.PageSystemId, x => x.Score.GetValueOrDefault()) : null;

                return pages.Select(x => new PageSearchResult
                {
                    Item = x,
                    Id = x.SystemId,
                    Name = x.Localizations.CurrentUICulture.Name,
                    Url = _urlService.GetUrl(x),
                    Score = includeScore ? pageLookup.TryGetValue(x.SystemId, out var dbl) ? (float)dbl : default : default
                });
            });
        }

        public QueryContainer SearchByText(QueryContainerDescriptor<PageDocument> selector, string text, Fuzziness fuzziness)
        {
            return selector.Match(x => x.Field(z => z.Name).Query(text).Fuzziness(fuzziness).Boost(2).SynonymAnalyzer())
                   || selector.Match(x => x.Field(z => z.Content).Query(text).Fuzziness(fuzziness).SynonymAnalyzer())
                   || SearchInBlocks(selector, text, fuzziness);
        }

        public QueryContainer SearchByPageType(QueryContainerDescriptor<PageDocument> selector, SearchQuery searchQuery)
        {
            if (searchQuery.PageType == PageTemplateNameConstants.NewsList && searchQuery.PageSystemId.HasValue)
            {
                return selector.Bool(b => b.Filter(bf =>
                {
                    var q = bf.Term(t => t.Field(f => f.IsNews).Value(true));
                    q &= bf.Term(t => t.Field(f => f.ParentPages).Value(searchQuery.PageSystemId.Value));
                    return q;
                }));
            }

            return selector;
        }

        public QueryContainer SearchInBlocks(QueryContainerDescriptor<PageDocument> selector, string text, Fuzziness fuzziness)
        {
            return selector.Nested(qb => qb.Path(x => x.Blocks).Query(nq => nq.Bool(b => b.Filter(bf =>
            {
                var q = bf.Match(t => t.Field(f => f.Blocks.First().Content).Query(text).Fuzziness(fuzziness).SynonymAnalyzer());
                q &= bf.DateRange(t => t.Field(f => f.Blocks.First().ChannelStartDateTime).LessThan(DateTimeOffset.Now.UtcDateTime));
                q &= bf.DateRange(t => t.Field(f => f.Blocks.First().ChannelEndDateTime).GreaterThan(DateTimeOffset.Now.UtcDateTime));
                q &= bf.Bool(_permissionDescriptor);
                return q;
            })))
            );
        }

        public QueryContainer SearchInBrands(QueryContainerDescriptor<PageDocument> selector, bool? onlyBrands)
        {
            return onlyBrands.HasValue ? selector.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.IsBrand).Value(onlyBrands))))
            : selector;
        }
    }
}
