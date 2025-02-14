using Litium.Web.WebApi.OpenApi;
using Microsoft.OpenApi.Models;

namespace Litium.Accelerator.OpenApi
{
    public class LitiumCartContextCookie : IOpenApiOperationFilter
    {
        public void Apply(OpenApiOperation openApiOperation, OpenApiOperationFilterContext context)
        {
            openApiOperation.Parameters.Add(new OpenApiParameter
            {
                In = ParameterLocation.Cookie,
                Name = "cart-context",
                AllowEmptyValue = false,
                Description = @"The litium cart context cookie.

The cookie are used to identify the current cart context.

The cart-context cookie can be found in the browsers developer tools in the tab application.",
                Example = new Microsoft.OpenApi.Any.OpenApiString("CfDJ8FZwEriFFAlAuEPRfyFz-plpxHYjDvgBBB9S9AuCH1zDtYWh2lA_htR3m4hNsfNOMch1d_Sch6RICgMIPeEERza4mgdx_gmDRck1wlvILdl2o8qJfPBNsGSAoWp8hXdGUUo62e_n7ntMTOZ_iGCFdmI")
            });
        }
    }
}
