using System;
using System.Collections.Generic;

namespace Litium.Accelerator.ViewModels.Search
{
    public class SearchResult
    {
        public Lazy<IEnumerable<SearchResultItem>> Items { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}
