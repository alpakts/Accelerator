using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Customers;
using Litium.Data;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.Builders.MyPages
{
    public class BusinessPersonViewModelBuilder : IViewModelBuilder<BusinessPersonViewModel>
    {
        private readonly PersonService _personService;
        private readonly RoleService _roleService;
        private readonly DataService _dataService;
        private readonly PersonStorage _personStorage;

        public BusinessPersonViewModelBuilder(
            PersonService personService,
            RoleService roleService,
            DataService dataService,
            PersonStorage personStorage)
        {
            _personService = personService;
            _roleService = roleService;
            _dataService = dataService;
            _personStorage = personStorage;
        }

        public virtual IEnumerable<BusinessPersonViewModel> Build()
        {
            return GetPersons().MapEnumerableTo<BusinessPersonViewModel>();
        }

        public virtual BusinessPersonViewModel Build(Guid id)
        {
            var person = _personService.Get(id);
            return person.MapTo<BusinessPersonViewModel>();
        }

        private IEnumerable<Person> GetPersons()
        {
            if (_personStorage.CurrentSelectedOrganizationSystemId is not Guid currentOrganizationSystemId)
            {
                return null;
            }

            var linkedPersons = _personService.GetByOrganization(currentOrganizationSystemId);
            return linkedPersons.Where(x => x.OrganizationLinks.Any(t =>
                HasRoles(t, RolesConstants.RoleOrderApprover, RolesConstants.RoleOrderPlacer)));
        }

        private bool HasRoles(PersonToOrganizationLink personLink, params string[] roleIds)
        {
            var roles = roleIds.Select(roleId => _roleService.Get(roleId)).Where(role => role != null);

            return HasRoles(personLink, roles.ToArray());
        }

        private static bool HasRoles(PersonToOrganizationLink personLink, params Role[] roles)
        {
            return roles.Any(role => personLink.RoleSystemIds.Contains(role.SystemId));
        }
    }
}
