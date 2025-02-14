using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.Search.Indexing
{
    public static class OptionValueHelper
    {
        public static IEnumerable<string> GetDecimalOptionValues(DecimalOption options, IList<decimal> values, CultureInfo cultureInfo)
        {
            return values.Select(value => GetDecimalOptionValue(options, value, cultureInfo));
        }

        public static string GetDecimalOptionValue(DecimalOption options, decimal value, CultureInfo cultureInfo)
        {
            var option = options?.Items?.FirstOrDefault(x => x.Value == value);
            return option != null && option.Name.TryGetValue(cultureInfo.Name, out var translation) ? translation : value.ToString(cultureInfo);
        }

        public static IEnumerable<string> GetIntOptionValues(IntOption options, IList<int> values, CultureInfo cultureInfo)
        {
            return values.Select(value => GetIntOptionValue(options, value, cultureInfo));
        }

        public static string GetIntOptionValue(IntOption options, int value, CultureInfo cultureInfo)
        {
            var option = options?.Items?.FirstOrDefault(x => x.Value == value);
            return option != null && option.Name.TryGetValue(cultureInfo.Name, out var translation) ? translation : value.ToString(cultureInfo);
        }

        public static IEnumerable<string> GetTextOptionValues(TextOption options, IList<string> values, CultureInfo cultureInfo)
        {
            return values.Select(value => GetTextOptionValue(options, value, cultureInfo));
        }

        public static string GetTextOptionValue(TextOption options, string value, CultureInfo cultureInfo)
        {
            var option = options?.Items?.FirstOrDefault(x => x.Value == value);
            return option != null && option.Name.TryGetValue(cultureInfo.Name, out var translation) ? translation : value.ToString(cultureInfo);
        }
    }
}
