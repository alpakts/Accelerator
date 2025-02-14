using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a text option field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.TextOption)]
    internal class TextOptionFieldFormatter : FieldFormatter
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
            if (item == null)
            {
                return null;
            }

            string result = null;
            var options = fieldDefinition.Option as TextOption;
            if (options != null && options.MultiSelect)
            {
                var resultList = (item as IEnumerable<string>)?.Select(x => GetValue(x, args.Culture, options));
                if (resultList != null)
                {
                    result = string.Join(args.MultiSelectDelimeter ?? "; ", resultList);
                }
            }
            else
            {
                result = GetValue(item as string, args.Culture, options);
            }

            return (args.HtmlEncode && result != null) ? HttpUtility.HtmlEncode(result) : result;
        }

        private static string GetValue(string key, CultureInfo culture, TextOption options)
        {
            if (key == null)
            {
                return null;
            }
            var option = options?.Items.FirstOrDefault(x => x.Value == key);
            if (option == null)
            {
                return key;
            }

            string translation;
            if (option.Name.TryGetValue(culture?.Name ?? CultureInfo.CurrentCulture.Name, out translation))
                return translation;
            return key;
        }
    }
}
