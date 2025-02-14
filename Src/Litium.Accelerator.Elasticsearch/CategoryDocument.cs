using System;
using System.Collections.Generic;
using Litium.Search;
using Nest;

namespace Litium.Accelerator.Search
{
    public class CategoryDocument : IDocument
    {
        [Keyword(Ignore = true)]
        public string Id => string.Concat(CategorySystemId, ChannelSystemId);

        public Guid Assortment { get; set; }

        [ActiveOnChannel]
        public Guid ChannelSystemId { get; set; }

        public Guid CategorySystemId { get; set; }

        public ISet<string> Content { get; set; }

        public string Name { get; set; }

        [Permission]
        public IReadOnlyCollection<string> Permissions { get; set; }

        public ISet<Guid> Organizations { get; set; }

        [ActiveOnChannelStartDateUtc]
        public DateTimeOffset ChannelStartDateTime { get; set; }

        [ActiveOnChannelEndDateUtc]
        public DateTimeOffset ChannelEndDateTime { get; set; }
    }
}
