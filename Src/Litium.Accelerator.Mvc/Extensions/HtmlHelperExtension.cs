using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Litium.Accelerator.Mvc.Extensions
{
    public static class HtmlHelperExtension
    {
#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// Serialize the object to string using Camel case resolver.
        /// </summary>
        /// <param name="htmlHelper">The html helper.</param>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The IHtmlString which represents the serialized object.</returns>
        public static IHtmlContent Json(this IHtmlHelper htmlHelper, object obj)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
            return new HtmlString(JsonConvert.SerializeObject(obj, settings));
        }
    }
}
