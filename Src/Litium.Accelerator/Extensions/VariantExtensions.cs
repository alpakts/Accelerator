using System.Globalization;
using Litium.Products;

namespace Litium.Accelerator.Extensions
{
    /// <summary>
    /// Represents an extension for variant.
    /// </summary>
    public static class VariantExtensions
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The display name.</returns>
        public static string GetDisplayName(this Variant variant, CultureInfo culture)
        {
            return (culture != null) ? variant.Localizations[culture].Name ?? string.Empty : null;
        }
    }
}
