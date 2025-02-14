using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a media pointer image array field.
    /// </summary>
    [Service(Name = "MediaPointerImageArray")]
    internal class MediaPointerImageArrayFieldFormatter : FieldFormatter
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
            var items = (item as IEnumerable<Guid>)?.ToList();
            if (items == null || items.Count == 0 || args.Index < 0 || args.Index >= items.Count)
            {
                return null;
            }

            var fileId = items[args.Index];
            var imageModel = fileId.MapTo<ImageModel>();
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
