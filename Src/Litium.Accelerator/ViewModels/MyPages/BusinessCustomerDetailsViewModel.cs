using AutoMapper;
using JetBrains.Annotations;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.MyPages
{
    public class BusinessCustomerDetailsViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string CustomerNumber { get; set; }
        public string OrganizationNumber { get; set; }
        public string OrganizationName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, BusinessCustomerDetailsViewModel>();
            cfg.CreateMap<Person, BusinessCustomerDetailsViewModel>()
               .ForMember(x => x.CustomerNumber, m => m.MapFrom(person => person.Id))
               .ReverseMap();
            cfg.CreateMap<Organization, BusinessCustomerDetailsViewModel>()
               .ForMember(x => x.OrganizationNumber, m => m.MapFrom(organization => organization.Fields.GetValue<string>("LegalRegistrationNumber","*") ?? organization.Id))
               .ForMember(x => x.OrganizationName, m => m.MapFrom(organization => organization.Name));
        }
    }
}
