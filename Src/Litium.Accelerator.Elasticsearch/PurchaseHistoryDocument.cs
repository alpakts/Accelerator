using System;
using System.Collections.Generic;
using Litium.Search;
using Nest;

namespace Litium.Accelerator.Search
{
    public class PurchaseHistoryDocument : IDocument
    {
        [Keyword(Ignore = true)]
        public string Id => SystemId.ToString();

        [Keyword]
        public Guid ChannelSystemId { get; set; }

        /// <summary>
        /// Gets or sets the order systemId.
        /// </summary>
        [Keyword]
        public Guid SystemId { get; set; }

        public DateTimeOffset OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the article numbers
        /// </summary>
        [Keyword]
        public List<string> ArticleNumbers { get; set; } = new List<string>();

        /// <summary>
        /// The order row quantity mapping. This is not used in the index but is used to populate the purchase history.
        /// </summary>
        [Object(Enabled = false)]
        public List<RowItem> Rows { get; set; } = new List<RowItem>();

        public class RowItem
        {
            public string ArticleNumber { get; set; }
            public decimal Quantity { get; set; }
        }
    }
}
