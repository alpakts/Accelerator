using System;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Utilities;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using Litium.Security;

namespace Litium.Accelerator.ViewModels.Persons
{
    public class BusinessPersonViewModel : IViewModel, IAutoMapperConfiguration
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public Guid SystemId { get; set; }
        public bool Editable { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }

        [UsedImplicitly]
        public void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Person, BusinessPersonViewModel>()
                .ForMember(d => d.Editable, o => o.MapFrom<EditablePropertyResolver>())
                .ForMember(d => d.Role, o => o.MapFrom<RolePropertyResolver>());

            cfg.CreateMap<BusinessPersonViewModel, Person>()
                .ForMember(d => d.SystemId, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.LoginCredential.Username = src.Email;
                });

        }

        [UsedImplicitly]
        private class EditablePropertyResolver : IValueResolver<Person, BusinessPersonViewModel, bool>
        {
            private readonly SecurityContextService _securityContextService;

            public EditablePropertyResolver(SecurityContextService securityContextService)
            {
                _securityContextService = securityContextService;
            }

            public bool Resolve(Person source, BusinessPersonViewModel destination, bool destMember, ResolutionContext context)
            {
                //The logged user can only edit other persons in the same organization.
                return source.SystemId != _securityContextService.GetIdentityUserSystemId();
            }
        }

        [UsedImplicitly]
        private class RolePropertyResolver : IValueResolver<Person, BusinessPersonViewModel, string>
        {
            private readonly RoleService _roleService;
            private readonly PersonStorage _personStorage;

            public RolePropertyResolver(
                RoleService roleService,
                PersonStorage personStorage)
            {
                _roleService = roleService;
                _personStorage = personStorage;
            }

            public string Resolve(Person source, BusinessPersonViewModel destination, string destMember, ResolutionContext context)
            {
                var currentSelectedOrganizationSystemId = _personStorage.CurrentSelectedOrganizationSystemId.GetValueOrDefault();
                var roles = source.OrganizationLinks.First(x => x.OrganizationSystemId == currentSelectedOrganizationSystemId).RoleSystemIds.Select(x => _roleService.Get(x)).ToList();
                if (roles.Exists(t => t.Id == RolesConstants.RoleOrderApprover))
                {
                    return RolesConstants.RoleOrderApprover;
                }
                if (roles.Exists(t => t.Id == RolesConstants.RoleOrderPlacer))
                {
                    return RolesConstants.RoleOrderPlacer;
                }
                return string.Empty;
            }
        }
    }
}
