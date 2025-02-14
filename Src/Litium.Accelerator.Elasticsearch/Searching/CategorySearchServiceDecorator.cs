using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Search;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Search;
using Litium.Web;
using Litium.Web.Customers.TargetGroups;
using Litium.Web.Models.Globalization;
using Nest;

namespace Litium.Accelerator.Search.Searching
{
    [ServiceDecorator(typeof(CategorySearchService))]
    internal class CategorySearchServiceDecorator : CategorySearchService
    {
        private readonly CategorySearchService _parent;
        private readonly SearchClientService _searchClientService;
        private readonly CategoryService _categoryService;
        private readonly UrlService _urlService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PersonStorage _personStorage;

        public CategorySearchServiceDecorator(
            CategorySearchService parent,
            SearchClientService searchClientService,
            CategoryService categoryService,
            UrlService urlService,
            RequestModelAccessor requestModelAccessor,
            PersonStorage personStorage,
            TargetGroupEngine targetGroupEngine)
            : base(targetGroupEngine)
        {
            _parent = parent;
            _searchClientService = searchClientService;
            _categoryService = categoryService;
            _urlService = urlService;
            _requestModelAccessor = requestModelAccessor;
            _personStorage = personStorage;
        }

        public override async Task<SearchResult> SearchAsync(SearchQuery searchQuery, bool includeScore = false)
        {
            if (!_searchClientService.IsConfigured)
            {
                return await _parent.SearchAsync(searchQuery);
            }

            var assortmentSystemId = _requestModelAccessor.RequestModel.ChannelModel?.Channel?.MarketSystemId?.MapTo<MarketModel>()?.Market?.AssortmentSystemId;
            var response = await _searchClientService
                .SearchAsync<CategoryDocument>(CultureInfo.CurrentCulture, selector => selector
                     .From((searchQuery.PageNumber - 1) * searchQuery.PageSize)
                     .Size(searchQuery.PageSize)
                     .QueryWithPermission(q =>
                        q.PublishedOnChannel()
                        && q.Bool(b => b.Must(bf => bf.Term(t => t.Field(x => x.Assortment).Value(assortmentSystemId))))
                        && OrganizationPointer(q)
                        && (
                            q.Match(x => x.Field(z => z.Name).Query(searchQuery.Text).Fuzziness(Fuzziness.Auto).Boost(2).SynonymAnalyzer())
                            || q.Match(x => x.Field(z => z.Content).Query(searchQuery.Text).Fuzziness(Fuzziness.Auto).SynonymAnalyzer())
                        ))
                     .Source(x => x.Includes(z => z.Fields(p => p.CategorySystemId)))
                     );

            return Transform(searchQuery, unchecked((int)response.Total), () =>
            {
                var category = _categoryService.Get(response.Hits.Select(x => x.Source.CategorySystemId));
                var categoryLookup = includeScore ? response.Hits.ToDictionary(x => x.Source.CategorySystemId, x => x.Score.GetValueOrDefault()) : null;

                return category.Select(x => new CategorySearchResult
                {
                    Item = x,
                    Id = x.SystemId,
                    Name = x.Localizations.CurrentCulture.Name,
                    Url = _urlService.GetUrl(x),
                    Score = includeScore ? categoryLookup.TryGetValue(x.SystemId, out var dbl) ? (float)dbl : default : default
                });
            });
        }

        private QueryContainer OrganizationPointer(QueryContainerDescriptor<CategoryDocument> selector)
        {
            if (_personStorage.CurrentSelectedOrganizationSystemId is Guid selectedOrganizationSystemId)
            {
                return (selector.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Organizations).Value(Guid.Empty))))
                        || selector.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Organizations).Value(selectedOrganizationSystemId)))));
            }

            return selector.Bool(b => b.Filter(bf => bf.Term(t => t.Field(x => x.Organizations).Value(Guid.Empty))));
        }
    }
}
