using Litium.Accelerator.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Litium.Accelerator.Mvc.Runtime
{
    public class SessionStorageImpl : SessionStorage
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionStorageImpl(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override T GetValue<T>(string name)
        {
            var item = _httpContextAccessor.HttpContext?.Session?.GetString(name);
            if (item is null)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(item, Litium.Application.Runtime.ApplicationConverter.JsonSerializerSettings());
        }

        public override void SetValue<T>(string name, T value)
        {
            if (value == null)
            {
                _httpContextAccessor.HttpContext.Session.Remove(name);
            }
            else
            {
                var item = JsonConvert.SerializeObject(value, Litium.Application.Runtime.ApplicationConverter.JsonSerializerSettings());
                _httpContextAccessor.HttpContext.Session.SetString(name, item);
            }
        }
    }
}