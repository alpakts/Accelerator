using System;
using System.Collections.Generic;
using Litium.Search;
using Nest;

namespace Litium.Accelerator.Search
{
    public class PageDocument : IDocument
    {
        [Keyword(Ignore = true)]
        public string Id => string.Concat(PageSystemId, ChannelSystemId);

        public Guid PageSystemId { get; set; }

        public List<Guid> ParentPages { get; set; }

        [Text(Name = "name", Norms = false, Similarity = "BM25")]
        public string Name { get; set; }

        [Text(Name = "content")]
        public ISet<string> Content { get; set; }

        [Nested]
        public List<PageDocument.Block> Blocks { get; set; } = new List<PageDocument.Block>();

        [Keyword]
        public bool IsBrand { get; set; }

        [Keyword]
        public bool IsNews { get; set; }

        public DateTimeOffset NewsDate { get; set; }

        [Date]
        public DateTimeOffset PublishDateTime { get; set; }

        [ActiveOnChannel]
        public Guid ChannelSystemId { get; set; }

        [ActiveOnWebsite]
        public Guid WebsiteSystemId { get; set; }

        [ActiveOnChannelStartDateUtc]
        public DateTimeOffset ChannelStartDateTime { get; set; }

        [ActiveOnChannelEndDateUtc]
        public DateTimeOffset ChannelEndDateTime { get; set; }

        [Permission]
        public IReadOnlyCollection<string> Permissions { get; set; }

        public class Block
        {
            [Text(Name = "content")]
            public ISet<string> Content { get; set; }

            public DateTimeOffset ChannelStartDateTime { get; set; }

            public DateTimeOffset ChannelEndDateTime { get; set; }

            [Keyword]
            public IReadOnlyCollection<string> Permissions { get; set; }
        }
    }
}
