using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Customers.TargetGroups;
using Litium.Web.Customers.TargetGroups.Events;

namespace Litium.Accelerator.Search
{
    [Service(ServiceType = typeof(ProductSearchService), Lifetime = DependencyLifetime.Scoped)]
    public abstract class ProductSearchService
    {
        private readonly TargetGroupEngine _targetGroupEngine;

        protected ProductSearchService(
            TargetGroupEngine targetGroupEngine)
        {
            _targetGroupEngine = targetGroupEngine;
        }

        public abstract Task<SearchResult> SearchAsync(
            SearchQuery searchQuery,
            IDictionary<string, ISet<string>> tags = null,
            bool addPriceFilterTags = false,
            bool addNewsFilterTags = false,
            bool addCategoryFilterTags = false);

        public abstract Task<List<TagTerms>> GetTagTermsAsync(
            SearchQuery searchQuery,
            IEnumerable<string> tagNames);

        protected SearchResult Transform(SearchQuery searchQuery, int totalHitCount, Func<IEnumerable<SearchResultItem>> transformResultFunc)
        {
            return new SearchResult
            {
                Items = new Lazy<IEnumerable<SearchResultItem>>(() =>
                {
                    _targetGroupEngine.Process(new SearchEvent
                    {
                        SearchText = searchQuery.Text,
                        TotalHits = totalHitCount
                    });

                    return transformResultFunc();
                }),
                PageSize = searchQuery.PageSize.GetValueOrDefault(100000),
                Total = totalHitCount
            };
        }
    }

    internal class ProductSearchServiceImpl : ProductSearchService
    {
        public ProductSearchServiceImpl(TargetGroupEngine targetGroupEngine)
            : base(targetGroupEngine)
        {
        }

        public override Task<SearchResult> SearchAsync(SearchQuery searchQuery,
            IDictionary<string, ISet<string>> tags = null,
            bool addPriceFilterTags = false,
            bool addNewsFilterTags = false,
            bool addCategoryFilterTags = false)
        {
            return Task.FromResult<SearchResult>(null);
        }

        public override Task<List<TagTerms>> GetTagTermsAsync(SearchQuery searchQuery, IEnumerable<string> tagNames)
        {
            return Task.FromResult<List<TagTerms>>(null);
        }
    }
}
