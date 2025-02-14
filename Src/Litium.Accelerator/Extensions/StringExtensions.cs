using System;

namespace Litium.Accelerator.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Gets string in Camel case
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>The output camel case string.</returns>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
