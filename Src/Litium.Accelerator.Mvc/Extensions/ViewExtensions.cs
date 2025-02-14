using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Litium.Accelerator.Mvc.Extensions
{
    public static class ViewExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
                var viewResult = viewEngine.GetView("~/", viewName, !partial);
                if (viewResult.Success == false)
                {
                    return $"View with name '{viewName}' could not be found.";
                }

                var tempDataProvider = controller.HttpContext.RequestServices.GetRequiredService<ITempDataProvider>();
                var actionContext = new ActionContext(controller.HttpContext, new RouteData(), new ActionDescriptor());

                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
