using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.FieldFramework.FieldTypes;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using System;
using System.Collections.Generic;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.ViewModels.MyPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Litium.Accelerator.ViewModels.Login
{
    public class LoginViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string Title { get; set; }
        public LinkModel RedirectLink { get; set; }
        public LinkModel ForgottenPasswordLink { get; set; }
        public bool InsufficientPermissions { get; set; }

        public LoginFormViewModel LoginForm { get; set; } = new LoginFormViewModel();
        public ChangePasswordFormViewModel PasswordForm { get; set; } = new ChangePasswordFormViewModel();

        public IEnumerable<SelectListItem> Organizations { get; set; }
        public Guid SelectedOrganization { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorMessage { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, LoginViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.RedirectLink, m => m.MapFrom(loginPage => loginPage.GetValue<PointerPageItem>(LoginPageFieldNameConstants.RedirectLink).MapTo<LinkModel>()))
               .ForMember(x => x.ForgottenPasswordLink, m => m.MapFrom(loginPage => loginPage.GetValue<PointerPageItem>(LoginPageFieldNameConstants.ForgottenPasswordLink).MapTo<LinkModel>()));
        }
    }
}
