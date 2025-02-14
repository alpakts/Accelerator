using System;
using System.Globalization;
using System.Web;
using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a date field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.Date)]
    internal class DateFieldFormatter : FieldFormatter
    {
        /// <summary>
        /// Converts the value of a specified object to an equivalent string representation using specified format and
        /// culture-specific formatting information.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="item">The item.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public override string Format(IFieldDefinition fieldDefinition, object item, FieldFormatArgs args)
        {
            args.ContentType = "text/plain";

            if (!(item is DateTimeOffset))
                return string.Empty;
            //when format not given, return in yyyy/MM/dd format.
            var value = ((DateTimeOffset)item).Date.ToString(args.Format ?? "d", args.Culture ?? CultureInfo.InvariantCulture);

            return (args.HtmlEncode) ? HttpUtility.HtmlEncode(value) : value;
        }
    }
}
