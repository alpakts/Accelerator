using Litium.Accelerator.ViewModels.Author;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Author
{
    public class AuthorViewModelBuilder : IViewModelBuilder<AuthorViewModel>
    {
        /// <summary>
        /// Build the Author model
        /// </summary>
        /// <param name="pageModel">The current Author page</param>
        /// <returns>Return the Author model</returns>
        public virtual AuthorViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<AuthorViewModel>();
            return model;
        }
    }
}
