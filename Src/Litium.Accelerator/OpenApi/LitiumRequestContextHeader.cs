using Litium.Web.WebApi.OpenApi;
using Microsoft.OpenApi.Models;

namespace Litium.Accelerator.OpenApi
{
    public class LitiumRequestContextHeader : IOpenApiOperationFilter
    {
        public void Apply(OpenApiOperation openApiOperation, OpenApiOperationFilterContext context)
        {
            openApiOperation.Parameters.Add(new OpenApiParameter
            {
                In = ParameterLocation.Header,
                Name = "litium-request-context",
                Required = true,
                AllowEmptyValue = false,
                Description = @"The litium request context as a json string.

The litium request context is used by the application to identify the channel, page, category, or product the request belongs to.

The litium-request-context is generated inside the source of the page and assigned to the __litium.requestContext JavaScript variable.",
                Example = new Microsoft.OpenApi.Any.OpenApiString(@"{""channelSystemId"":""8f15c20a-c9af-441e-be15-fd4b3fe61b73"",""currentPageSystemId"":""23ab9493-da58-4fd9-a766-c80fe8a4d3f6""}")
            });
        }
    }
}
