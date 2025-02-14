using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Customers.TargetGroups;
using Litium.Web.Customers.TargetGroups.Events;

namespace Litium.Accelerator.Search
{
    [Service(ServiceType = typeof(CategorySearchService), Lifetime = DependencyLifetime.Singleton)]
    public abstract class CategorySearchService
    {
        private readonly TargetGroupEngine _targetGroupEngine;

        protected CategorySearchService(
            TargetGroupEngine targetGroupEngine)
        {
            _targetGroupEngine = targetGroupEngine;
        }

        public abstract Task<SearchResult> SearchAsync(SearchQuery searchQuery, bool includeScore = false);

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

    internal class CategorySearchServiceImpl : CategorySearchService
    {
        public CategorySearchServiceImpl(TargetGroupEngine targetGroupEngine)
            : base(targetGroupEngine)
        {
        }

        public override Task<SearchResult> SearchAsync(SearchQuery searchQuery, bool includeScore = false)
        {
            return Task.FromResult<SearchResult>(default);
        }
    }
}
