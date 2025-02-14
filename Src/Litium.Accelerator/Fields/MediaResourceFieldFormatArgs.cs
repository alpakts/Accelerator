using System.Drawing;

namespace Litium.Accelerator.Fields
{
    /// <summary>
    /// Image field formatting.
    /// </summary>
    public class MediaResourceFieldFormatArgs:FieldFormatArgs
    {
        /// <summary>
        /// Gets or sets the size of the image., this field is set by the formatter.
        /// </summary>
        /// <value>
        /// The size of the image.
        /// </value>
        public Size ImageSize { get; set; }

        /// <summary>
        /// Gets or sets the max size, if the field is an image.
        /// </summary>
        public Size MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the min size, if the field is an image.
        /// </summary>
        public Size MinSize { get; set; }

        /// <summary>
        /// Value set by the formatter to indicate the resource is a image.
        /// </summary>
        public bool IsImage { get; set; }

        /// <summary>
        /// Value set by the formatter, gives resource file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
