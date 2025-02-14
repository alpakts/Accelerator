using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Login;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Litium.Accelerator.Builders.Login
{
    public class LoginViewModelBuilder : IViewModelBuilder<LoginViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;

        public LoginViewModelBuilder(RequestModelAccessor requestModelAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
        }

        public virtual LoginViewModel Build(string redirectUrl)
            => Build(_requestModelAccessor.RequestModel.CurrentPageModel, redirectUrl);

        public virtual LoginViewModel Build(LoginViewModel loginViewModel)
            => Build(_requestModelAccessor.RequestModel.CurrentPageModel, loginViewModel);

        public virtual LoginViewModel Build(PageModel pageModel, string redirectUrl)
        {
            var model = pageModel.MapTo<LoginViewModel>();
            model.RedirectUrl = GetRedirectUrl(model, redirectUrl);
            return model;
        }

        public virtual LoginViewModel Build(PageModel pageModel, LoginViewModel loginViewModel)
        {
            var model = pageModel.MapTo<LoginViewModel>();
            model.LoginForm = loginViewModel.LoginForm;
            model.PasswordForm = loginViewModel.PasswordForm;
            return model;
        }

        public IEnumerable<SelectListItem> GetOrganizations(List<Organization> organizations)
        {
            return organizations.Select(org => new SelectListItem
            {
                Text = org.Name,
                Value = org.SystemId.ToString()
            });
        }

        private string GetRedirectUrl(LoginViewModel model, string redirectUrl)
        {
            redirectUrl = string.IsNullOrWhiteSpace(redirectUrl) ? model.RedirectLink?.Href : redirectUrl;
            return string.IsNullOrWhiteSpace(redirectUrl) ? "~/" : redirectUrl;
        }
    }
}
