using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using System;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;

namespace Litium.Accelerator.ViewModels.Login
{
    public class ForgotPasswordViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string Title { get; set; }
        
        public ForgotPasswordFormViewModel ForgotPasswordForm { get; set; } = new ForgotPasswordFormViewModel();
        public LinkModel LoginLink { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public Guid ChannelSystemId { get; set; }
        public string RedirectUrl { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, ForgotPasswordViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title));
        }
    }
}
