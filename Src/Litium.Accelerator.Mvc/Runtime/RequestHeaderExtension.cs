using Litium.Accelerator.ViewModels.Framework;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Litium.Accelerator.Mvc.Runtime
{
    public static class RequestHeaderExtension
    {
        public static SiteSettingViewModel GetSiteSettingViewModel(this IHeaderDictionary httpHeaders)
        {
            if (httpHeaders.TryGetValue("litium-request-context", out var items))
            {
                var json = items.First();
                var jObject = JObject.Parse(json);
                return jObject.ToObject<SiteSettingViewModel>();
            }
            return null;
        }
    }
}