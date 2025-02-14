using System.Web;
using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a limited text field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.LimitedText)]
    internal class LimitedTextFieldFormatter : FieldFormatter
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
            args.ContentType = "text/html";
            return (args.HtmlEncode && item != null) ? HttpUtility.HtmlEncode(item.ToString()) : item as string;
        }
    }
}
