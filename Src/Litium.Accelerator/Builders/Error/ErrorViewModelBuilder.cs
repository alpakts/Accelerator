using Litium.Accelerator.ViewModels.Error;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Error
{
    public class ErrorViewModelBuilder : IViewModelBuilder<ErrorViewModel>
    {
        /// <summary>
        /// Build the error model
        /// </summary>
        /// <param name="pageModel">The current error page</param>
        /// <returns>Return the error page model</returns>
        public virtual ErrorViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<ErrorViewModel>();
            return model;
        }
    }
}
