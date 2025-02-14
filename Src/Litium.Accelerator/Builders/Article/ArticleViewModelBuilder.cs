using Litium.Accelerator.ViewModels.Article;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Article
{
    public class ArticleViewModelBuilder : IViewModelBuilder<ArticleViewModel>
    {
        /// <summary>
        /// Build the article model
        /// </summary>
        /// <param name="pageModel">The current article page</param>
        /// <returns>Return the article model</returns>
        public virtual ArticleViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<ArticleViewModel>();
            return model;
        }
    }
}
