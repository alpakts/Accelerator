using System;

namespace Litium.Accelerator.Fields
{
    /// <summary>
    /// Link field format args.
    /// </summary>
    public class LinkFieldFormatArgs : MediaResourceFieldFormatArgs
    {
        /// <summary>
        /// Value set by the formatter to indicate the resource is an external link.
        /// </summary>
        public bool IsExternalLink { get; set; } = false;

        /// <summary>
        /// Value set by the formatter.
        /// </summary>
        public string Text { get; set; } 
        
        /// <summary>
        /// Gets or sets the channel system identifier.
        /// </summary>
        /// <value>The channel system identifier.</value>
        public Guid? ChannelSystemId { get; set; }

    }
}
