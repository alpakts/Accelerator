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
    ///     Writes contents of a media pointer file field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.MediaPointerFile)]
    internal class MediaPointerFileFieldFormatter : FieldFormatter
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

            var imageModel = ((Guid)item).MapTo<FileModel>();
            if (imageModel is null)
            {
                return string.Empty;
            }

            var resourceFormatArgs = args as MediaResourceFieldFormatArgs;
            if (resourceFormatArgs != null)
            {
                resourceFormatArgs.FileName = imageModel.Filename;
            }

            args.ContentType = imageModel.ContentType;

            var value = imageModel.Url;
            return args.HtmlEncode ? HttpUtility.HtmlEncode(value) : value;
        }
    }
}
