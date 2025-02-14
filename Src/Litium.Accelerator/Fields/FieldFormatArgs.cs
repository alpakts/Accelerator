using System;
using System.Globalization;

namespace Litium.Accelerator.Fields
{
    /// <summary>
    /// Field format args.
    /// </summary>
    public class FieldFormatArgs
    {
        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        public virtual CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets the multi select delimeter.
        /// </summary>
        public virtual string MultiSelectDelimeter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to Html encode the result.
        /// </summary>
        public virtual bool HtmlEncode { get; set; }

        /// <summary>
        /// Gets or sets the format used if the field value being formatted supports <see cref="IFormattable"/>
        /// </summary>
        public virtual string Format { get; set; }

        /// <summary>
        /// Gets or sets the index to fetch, if the field is a List of values.
        /// </summary>
        public virtual int Index { get; set; } = 0;

        /// <summary>
        /// This is set by the formatter, to indicate the type of content. The value is a MIME type.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        public virtual string ContentType { get; set; }
    }
}
