using AutoMapper;
using Litium.Customers;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.Mailing.Models
{
    public class WelcomeEmailModel : IAutoMapperConfiguration
    {
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Person, WelcomeEmailModel>()
                .ForMember(x => x.LoginName, o => o.MapFrom(x => x.LoginCredential.Username));
        }
    }
}
