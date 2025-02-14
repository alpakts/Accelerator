using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Searchers
{
    [Service(ServiceType = typeof(BaseSearcher), Lifetime = DependencyLifetime.Scoped)]
    public abstract class BaseSearcher
    {
        public abstract string ModelKey { get; }

        public abstract int SortOrder { get; }

        public abstract int PageSize { get; }

        /// <summary>
        /// Queries the data based on the search query, as the compact version.
        /// The compact version is used in for example, the Quick search box.
        /// </summary>
        /// <param name="query">The plain text query.</param>
        /// <param name="includeScore">Flag to indicate if the score should be included in the result item.</param>
        /// <returns>The search result object.</returns>
        public abstract Task<SearchResult> QueryCompactAsync(string query, bool includeScore = false);

        /// <summary>
        /// Converts the list of <see cref="SearchResultItem"/> to list of <see cref="SearchItem"/>.
        /// </summary>
        /// <param name="items">The list of <see cref="SearchResultItem"/> to convert.</param>
        /// <returns>The list of <see cref="SearchItem"/>.</returns>
        public abstract IEnumerable<SearchItem> ToSearchItems(IEnumerable<SearchResultItem> items);
    }

    public abstract class BaseSearcher<T> : BaseSearcher where T : SearchResultItem
    {
        public override IEnumerable<SearchItem> ToSearchItems(IEnumerable<SearchResultItem> items)
        {
            return items.Select(x => ((T)x).MapTo<SearchItem>());
        }
    }
}
