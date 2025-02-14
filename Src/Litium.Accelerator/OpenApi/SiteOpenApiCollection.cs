using Litium.Web.WebApi;
using Litium.Web.WebApi.OpenApi;

namespace Litium.Accelerator.OpenApi
{
    public class SiteOpenApiCollection : IApiCollection
    {
        internal const string _collectionName = "site";

        public string Name { get; } = "Accelerator Web API";
        public string Collection => _collectionName;

        public void Configure(OpenApiDocumentBuilder builder)
        {
            builder.AddOperationFilter<LitiumRequestContextHeader>();
            builder.AddOperationFilter<LitiumCartContextCookie>();
        }

        public bool IsAuthorized() => true;
    }
}
