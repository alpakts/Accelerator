using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.ViewModels.MyPages
{
    public class LoginInfoViewModel : IAutoMapperConfiguration, IViewModel
    {
        public ChangeUserNameFormViewModel UserNameForm { get; set; } = new ChangeUserNameFormViewModel();
        public ChangePasswordFormViewModel PasswordForm { get; set; } = new ChangePasswordFormViewModel();
        public bool IsSystemAccount { get; set; }
        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, LoginInfoViewModel>();
            cfg.CreateMap<Person, LoginInfoViewModel>()
                .ForMember(x => x.UserNameForm, m => m.MapFrom(person => person.MapTo<ChangeUserNameFormViewModel>()));
        }
    }
}
