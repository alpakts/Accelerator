using System;
using System.Collections.Generic;
using Litium.Search;
using Nest;

namespace Litium.Accelerator.Search
{
    public class ProductDocument : IDocument
    {
        [Keyword(Ignore = true)]
        public string Id => string.Concat(ArticleNumber, ChannelSystemId).ToLowerInvariant();

        [Keyword]
        public string ArticleNumber { get; set; }

        public List<Guid> Assortments { get; set; } = new List<Guid>();

        public Guid BaseProductSystemId { get; set; }

        public List<Guid> Categories { get; set; } = new List<Guid>();

        public List<Guid> ParentCategories { get; set; } = new List<Guid>();

        [Nested]
        public List<SortItem> CategorySortIndex { get; set; } = new List<SortItem>();

        [ActiveOnChannel]
        public Guid ChannelSystemId { get; set; }

        public ISet<string> Content { get; set; }

        public bool IsBaseProduct { get; set; }

        [Nested]
        public List<MainCategory> MainCategories { get; set; }

        public decimal PurchaseHistoryQuantity { get; set; }

        public DateTime? NewsDate { get; set; }

        [Nested]
        public List<PriceItem> Prices { get; set; } = new List<PriceItem>();

        public List<Guid> ProductLists { get; set; } = new List<Guid>();

        [Nested]
        public List<SortItem> ProductListSortIndex { get; set; } = new List<SortItem>();

        [Nested]
        public List<TagItem> Tags { get; set; } = new List<TagItem>();

        public string Name { get; set; }

        public HashSet<Guid> VariantSystemIds { get; set; } = new HashSet<Guid>();

        [Permission]
        public IReadOnlyCollection<string> Permissions { get; set; }

        public ISet<Guid> Organizations { get; set; }

        [ActiveOnChannelStartDateUtc]
        public DateTimeOffset ChannelStartDateTime { get; set; }

        [ActiveOnChannelEndDateUtc]
        public DateTimeOffset ChannelEndDateTime { get; set; }

        public class SortItem
        {
            public Guid SystemId { get; set; }

            public int SortIndex { get; set; }
        }

        public class PriceItem
        {
            public ISet<Guid> PriceListSystemIds { get; } = new HashSet<Guid>();

            public Guid CountrySystemId { get; set; }

            public bool IsCampaignPrice { get; set; }

            public decimal PriceIncludeVat { get; set; }

            public decimal PriceExcludeVat { get; set; }
        }

        public class TagItem
        {
            [Keyword]
            public string Key { get; set; }

            [Keyword(EagerGlobalOrdinals = true)]
            public string Value { get; set; }
        }

        public class MainCategory
        {
            public Guid AssortmentSystemId { get; set; }

            [Keyword(EagerGlobalOrdinals = true)]
            public Guid CategorySystemId { get; set; }
        }
    }
}
