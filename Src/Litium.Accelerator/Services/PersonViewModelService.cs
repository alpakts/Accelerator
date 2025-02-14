using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Mailing.Models;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;
using Litium.Security;
using Litium.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Services
{
    public class PersonViewModelService : ViewModelService<BusinessPersonViewModel>
    {
        private static readonly IList<string> _b2bDefaultRoles = new List<string> { RolesConstants.RoleOrderApprover, RolesConstants.RoleOrderPlacer };
        private readonly PersonService _personService;
        private readonly SecurityContextService _securityContextService;
        private readonly LoginService _loginService;
        private readonly MailService _mailService;
        private readonly FieldTemplateService _templateService;
        private readonly RoleService _roleService;
        private readonly UserValidationService _userValidationService;
        private readonly WelcomeEmailDefinitionResolver _welcomeEmailDefinitionResolver;
        private readonly PersonStorage _personStorage;
        private readonly ILogger<PersonViewModelService> _logger;

        public PersonViewModelService(
            PersonService personService,
            SecurityContextService securityContextService,
            LoginService loginService,
            MailService mailService,
            FieldTemplateService templateService,
            RoleService roleService,
            UserValidationService userValidationService,
            WelcomeEmailDefinitionResolver welcomeEmailDefinitionResolver,
            PersonStorage personStorage,
            ILogger<PersonViewModelService> logger)
        {
            _personService = personService;
            _securityContextService = securityContextService;
            _loginService = loginService;
            _mailService = mailService;
            _templateService = templateService;
            _roleService = roleService;
            _userValidationService = userValidationService;
            _welcomeEmailDefinitionResolver = welcomeEmailDefinitionResolver;
            _personStorage = personStorage;
            _logger = logger;
        }

        public bool Create(BusinessPersonViewModel viewModel, ModelStateDictionary modelState)
        {
            var selectedRole = GetRoles().First(x => x.Id == viewModel.Role);

            try
            {
                var person = viewModel.MapTo<Person>();
                person.LoginCredential.PasswordExpirationDate = DateTimeOffset.UtcNow;
                person.OrganizationLinks = new List<PersonToOrganizationLink>()
                {
                    new PersonToOrganizationLink(_personStorage.CurrentSelectedOrganization.SystemId)
                    {
                        RoleSystemIds = new HashSet<Guid>() {selectedRole.SystemId}
                    }
                };

                var template = _templateService.Get<PersonFieldTemplate>(typeof(CustomerArea), DefaultWebsiteFieldValueConstants.CustomerTemplateId);
                if (template is null)
                {
                    throw new NullReferenceException($"Customer template with Id '{DefaultWebsiteFieldValueConstants.CustomerTemplateId}' does not exist");
                }
                person.FieldTemplateSystemId = template.SystemId;

                person.LoginCredential.NewPassword = _loginService.GeneratePassword();

                using (_securityContextService.ActAsSystem())
                {
                    _personService.Create(person);
                }

                _mailService.SendEmail(_welcomeEmailDefinitionResolver.Get(person.MapTo<WelcomeEmailModel>(), person.Email), false);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Message}", ex.Message);
                modelState.AddModelError("general", "mypage.person.unabletocreate".AsWebsiteText());
                return false;
            }
        }

        public bool Update(BusinessPersonViewModel viewModel, ModelStateDictionary modelState)
        {
            try
            {
                var person = _personService.Get(viewModel.SystemId).MakeWritableClone();
                person.MapFrom(viewModel);

                var selectedRole = GetRoles().First(x => x.Id == viewModel.Role);
                AssignRole(person, selectedRole);

                using (_securityContextService.ActAsSystem())
                {
                    _personService.Update(person);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Message}", ex.Message);
                modelState.AddModelError("general", "mypage.user.unabletoupdate".AsWebsiteText());
                return false;
            }
        }

        public void Delete(Guid id)
        {
            var currentOrganizationSystemId = _personStorage.CurrentSelectedOrganizationSystemId;
            var person = _personService.Get(id).MakeWritableClone();
            person.OrganizationLinks.Remove(person.OrganizationLinks.First(x => x.OrganizationSystemId == currentOrganizationSystemId));
            using (_securityContextService.ActAsSystem())
            {
                _personService.Update(person);
            }
        }

        public bool Validate(BusinessPersonViewModel viewModel, ModelStateDictionary modelState)
        {
            var validationRules = new List<ValidationRuleItem<BusinessPersonViewModel>>()
            {
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.FirstName), Rule = model => !string.IsNullOrEmpty(model.FirstName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.LastName), Rule = model => !string.IsNullOrEmpty(model.LastName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.Email), Rule = model => !string.IsNullOrEmpty(model.Email), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.Email), Rule = model => _userValidationService.IsValidEmail(model.Email), ErrorMessage = () => "validation.email".AsWebsiteText()},
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.Email), Rule = IsValidUserName, ErrorMessage = () => "validation.unique".AsWebsiteText()},
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.Phone), Rule = model => !string.IsNullOrEmpty(model.Phone), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessPersonViewModel>{Field = nameof(BusinessPersonViewModel.Phone), Rule = model => _userValidationService.IsValidPhone(model.Phone), ErrorMessage = () => "validation.phone".AsWebsiteText()},

            };

            return viewModel.IsValid(validationRules, modelState);
        }


        private List<Role> GetRoles()
        {
            return _b2bDefaultRoles
                .Select(roleId => _roleService.Get(roleId))
                .Where(role => role != null).ToList();
        }

        private void AssignRole(Person person, Role selectedRole)
        {
            var defaultRoles = GetRoles();
            var currentSelectedOrganizationSystemId = _personStorage.CurrentSelectedOrganizationSystemId.GetValueOrDefault();
            var assignedRoles = person.OrganizationLinks.First(x => x.OrganizationSystemId == currentSelectedOrganizationSystemId).RoleSystemIds;

            bool isInDefaultRoles(Guid guid) => defaultRoles.Any(defaultRole => defaultRole.SystemId == guid);

            if (isInDefaultRoles(selectedRole.SystemId) &&
                assignedRoles.All(assignedRole => assignedRole != selectedRole.SystemId))
            {
                assignedRoles.Add(selectedRole.SystemId);
            }

            var rolesToDelete = assignedRoles.Where(role => isInDefaultRoles(role) && role != selectedRole.SystemId).ToList();
            foreach (var guid in rolesToDelete)
            {
                assignedRoles.Remove(guid);
            }
        }

        private Person GetUser(string userName)
        {
            var personId = _securityContextService.GetPersonSystemId(userName);
            return personId != null ? _personService.Get(personId.Value) : null;
        }

        private bool IsValidUserName(BusinessPersonViewModel model)
        {
            var user = GetUser(model.Email);
            if (user is null || user.SystemId == model.SystemId)
            {
                return true;
            }
            return false;
        }
    }
}
