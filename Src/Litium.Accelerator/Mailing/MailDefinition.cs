using System;

namespace Litium.Accelerator.Mailing
{
    /// <summary>
    /// Base mail implementation, inherit <see cref="PlainMailDefinition" />, <see cref="HtmlMailDefinition" />
    /// or <see cref="PageMailDefinition" />.
    /// </summary>
    public abstract class MailDefinition
    {
        /// <summary>
        /// Gets the channel systemId.
        /// </summary>
        /// <value>
        /// The channelSystemId.
        /// </value>
        public abstract Guid ChannelSystemId { get; }
        /// <summary>
        /// Gets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public abstract string Subject { get; }
        /// <summary>
        /// Gets to email.
        /// </summary>
        /// <value>
        /// To email.
        /// </value>
        public abstract string ToEmail { get; }
    }
}
