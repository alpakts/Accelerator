using Litium.Accelerator.ViewModels.LandingPage;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.LandingPage
{
    public class LandingPageViewModelBuilder : IViewModelBuilder<LandingPageViewModel>
    {
        /// <summary>
        /// Build the landing page model
        /// </summary>
        /// <param name="pageModel">The current page</param>
        /// <returns>Return the landing page model</returns>
        public virtual LandingPageViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<LandingPageViewModel>();
            return model;
        }
    }
}

