using System;
using System.Drawing;
using System.Web;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a media pointer image field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.MediaPointerImage)]
    internal class MediaPointerImageFieldFormatter : FieldFormatter
    {
        /// <summary>
        ///     Converts the value of a specified object to an equivalent string representation using specified format and
        ///     culture-specific formatting information.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="item">The item.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public override string Format(IFieldDefinition fieldDefinition, object item, FieldFormatArgs args)
        {
            if (!(item is Guid))
            {
                return string.Empty;
            }

            var imageModel = ((Guid)item).MapTo<ImageModel>();
            if (imageModel is null)
            {
                return string.Empty;
            }

            var imageArgs = args as MediaResourceFieldFormatArgs;
            var value = imageModel.GetUrlToImage(imageArgs?.MinSize ?? Size.Empty, imageArgs?.MaxSize ?? Size.Empty);
            if (value is null)
            {
                return string.Empty;
            }

            imageArgs.IsImage = true;
            imageArgs.ImageSize = value.Dimension;
            imageArgs.FileName = imageModel.Filename;
            imageArgs.ContentType = imageModel.ContentType;

            return (args.HtmlEncode && value != null) ? HttpUtility.HtmlEncode(value.Url) : value.Url;
        }
    }
}
