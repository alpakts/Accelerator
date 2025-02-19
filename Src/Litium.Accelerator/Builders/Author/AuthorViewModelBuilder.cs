using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.Author;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using System;

namespace Litium.Accelerator.Builders.Author
{
    public class AuthorViewModelBuilder : IViewModelBuilder<AuthorViewModel>
    {
        private readonly IAuthorService authorService;

        public AuthorViewModelBuilder(IAuthorService authorService)
        {
            this.authorService = authorService;
        }
        /// <summary>
        /// Build the Author model
        /// </summary>
        /// <param name="pageModel">The current Author page</param>
        /// <returns>Return the Author model</returns>
        public virtual AuthorViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<AuthorViewModel>();
            model.Books = this.authorService.GetBooksByAuthor(new Guid());
            return model;
        }

    }
}
