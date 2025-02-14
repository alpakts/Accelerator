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
    ///     Writes contents of a int option field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.IntOption)]
    internal class IntOptionFieldFormatter : FieldFormatter
    {
        /// <summary>
        ///     Formats the specified field definition.
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
            var options = fieldDefinition.Option as IntOption;
            if (options != null && options.MultiSelect)
            {
                var resultList = (item as IEnumerable<int>)?.Select(x => GetValue(fieldDefinition, x, options, args));
                if (resultList != null)
                {
                    result = string.Join(args.MultiSelectDelimeter ?? "; ", resultList);
                }
            }
            else
            {
                if (!(item is int))
                {
                    return null;
                }
                result = GetValue(fieldDefinition, (int)item, options, args);
            }

            return (args.HtmlEncode && result != null) ? HttpUtility.HtmlEncode(result) : result;
        }

        private static string GetValue(IFieldDefinition fieldDefinition, int key, IntOption options, FieldFormatArgs args)
        {
            if (!fieldDefinition.IsMultiCulture)
            {
                return key.ToString(args.Format, args.Culture ?? CultureInfo.InvariantCulture);
            }

            var option = options?.Items.FirstOrDefault(x => x.Value == key);
            if (option == null)
            {
                return key.ToString(args.Format, args.Culture ?? CultureInfo.InvariantCulture);
            }

            string translation;
            if (option.Name.TryGetValue(args.Culture?.Name ?? CultureInfo.CurrentCulture.Name, out translation))
                return translation;
            return key.ToString(args.Format, args.Culture ?? CultureInfo.InvariantCulture);
        }
    }
}
