using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Data.Queryable;

namespace Litium.Accelerator.Search
{
    public class SearchQuery
    {
        private Guid? _categorySystemId;
        private Guid? _productSystemListId;

        public SearchQuery()
        {
            Category = new HashSet<Guid>();
            Tags = new SortedDictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase);
            PriceRanges = new List<(int, int)>();
            PageNumber = 1;
        }

        public string BaseUrl { get; internal set; }
        public HashSet<Guid> Category { get; set; }

        public bool CategoryShowRecursively { get; set; }

        public Guid? CategorySystemId
        {
            get => _categorySystemId;
            set { _categorySystemId = value; BaseUrl = null; }
        }

        public Tuple<DateTime, DateTime> NewsDate { get; set; }

        public int PageNumber { get; set; }

        public List<(int, int)> PriceRanges { get; set; }

        public string SortBy { get; set; }

        public SortDirection SortDirection { get; set; }

        public IDictionary<string, ISet<string>> Tags { get; set; }

        public string Text { get; set; }

        public SearchType Type { get; set; }

        public int? PageSize { get; set; }

        public Guid? ProductListSystemId
        {
            get { return _productSystemListId; }
            set { _productSystemListId = value; BaseUrl = null; }
        }

        public string PageType { get; set; }

        public Guid? PageSystemId { get; set; }

        public Guid WebsiteSystemId { get; set; }

        /// <summary>
        /// Gets or sets the list of article number should have in the result.
        /// </summary>
        public IEnumerable<string> ArticleNumbers { get; set; }

        public SearchQuery Clone()
        {
            var clone = (SearchQuery)MemberwiseClone();
            clone.PriceRanges = PriceRanges.Select(x => (x.Item1, x.Item2)).ToList();
            clone.Tags = Tags.ToDictionary(x => x.Key, x => (ISet<string>)new HashSet<string>(x.Value, StringComparer.OrdinalIgnoreCase));
            clone.Category = new HashSet<Guid>(Category);
            clone.ArticleNumbers = new List<string>(ArticleNumbers ?? Array.Empty<string>());
            return clone;
        }
    }
}
