using System.Globalization;
using System.Linq;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.Extensions
{
    internal static class FieldDefinitionExtensions
    {
        public static string GetTranslation(this IFieldDefinition field, string key, CultureInfo culture = null)
        {
            if (key == null)
            {
                return null;
            }

            var options = field.Option as TextOption;
            var option = options?.Items.FirstOrDefault(x => x.Value == key);
            if (option == null)
            {
                return key;
            }

            if (option.Name.TryGetValue(culture?.Name ?? CultureInfo.CurrentUICulture.Name, out string translation) && !string.IsNullOrEmpty(translation))
            {
                return translation;
            }

            return key;
        }
    }
}
