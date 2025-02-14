using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Customers;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.ViewModels.MyPages
{
    public class ChangeUserNameFormViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string UserName { get; set; }

        [UsedImplicitly]
        public void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Person, ChangeUserNameFormViewModel>()
                .ForMember(x => x.UserName, m => m.MapFrom(person => person.LoginCredential.Username));
        }
    }
}
