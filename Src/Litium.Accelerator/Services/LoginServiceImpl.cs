using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Mailing.Models;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Login;
using Litium.Customers;
using Litium.Events;
using Litium.Runtime;
using Litium.Security;
using Litium.Security.Events;
using Litium.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Services
{
    public class LoginServiceImpl : LoginService
    {
        private readonly AuthenticationService _authenticationService;
        private readonly SecurityContextService _securityContextService;
        private readonly PersonService _personService;
        private readonly OrganizationService _organizationService;
        private readonly RoleService _roleService;
        private readonly MailService _mailService;
        private readonly UserValidationService _userValidationService;
        private readonly PersonStorage _personStorage;
        private readonly CheckoutState _checkoutState;
        private readonly ILogger<LoginServiceImpl> _logger;
        private readonly ApplicationJsonConverter _applicationJsonConverter;
        private readonly ForgotPasswordEmailDefinitionResolver _forgotPasswordEmailDefinitionResolver;
        private readonly IDataProtector _magicLinkProtector;
        private readonly EventBroker _eventBroker;

        public LoginServiceImpl(
            AuthenticationService authenticationService,
            SecurityContextService securityContextService,
            PersonService personService,
            OrganizationService organizationService,
            RoleService roleService,
            MailService mailService,
            UserValidationService userValidationService,
            PersonStorage personStorage,
            CheckoutState checkoutState,
            ILogger<LoginServiceImpl> logger,
            IDataProtectionProvider dataProtectionProvider,
            ApplicationJsonConverter applicationJsonConverter,
            ForgotPasswordEmailDefinitionResolver forgotPasswordEmailDefinitionResolver,
            EventBroker eventBroker)
        {
            _authenticationService = authenticationService;
            _securityContextService = securityContextService;
            _personService = personService;
            _organizationService = organizationService;
            _roleService = roleService;
            _mailService = mailService;
            _userValidationService = userValidationService;
            _personStorage = personStorage;
            _checkoutState = checkoutState;
            _logger = logger;
            _applicationJsonConverter = applicationJsonConverter;
            _forgotPasswordEmailDefinitionResolver = forgotPasswordEmailDefinitionResolver;
            _magicLinkProtector = dataProtectionProvider.CreateProtector(nameof(LoginService), "magic-link");
            _eventBroker = eventBroker;
        }

        public override string GeneratePassword()
        {
            var pass = Security.RandomStringGenerator.Generate(4, 4, 0, 2);
            return _userValidationService.IsValidPasswordComplexity(pass) ? pass : Security.RandomStringGenerator.Generate(4, 4, 1, 4);
        }

        public override bool Login(string loginName, string password)
        {
            return Login(loginName, password, null);
        }

        public override bool Login(string loginName, string password, string newPassword)
        {
            var result = _authenticationService.PasswordSignIn(loginName, password, newPassword);
            switch (result)
            {
                case AuthenticationResult.Success:
                    _checkoutState.ClearState();
                    return true;
                case AuthenticationResult.RequiresChangedPassword:
                    throw new ChangePasswordException("You need to change password.");
                default:
                    return false;
            }
        }

        public override void Logout()
        {
            _checkoutState.ClearState();
            _personStorage.CurrentSelectedOrganizationSystemId = null;
            _authenticationService.SignOut();
        }

        public override Person GetUser(string userInfo)
        {
            var personId = _securityContextService.GetPersonSystemId(userInfo);
            return personId != null ? _personService.Get(personId.Value) : null;
        }

        public override bool IsBusinessCustomer(Person person, out List<Organization> organizations)
        {
            _personStorage.CurrentSelectedOrganizationSystemId = null;
            organizations = new List<Organization>();
            if (person.OrganizationLinks.Count == 0) return false;

            var organizationIds = new List<Guid>();
            foreach (var personToOrganizationLink in person.OrganizationLinks)
            {
                foreach (var roleSystemId in personToOrganizationLink.RoleSystemIds)
                {
                    var role = _roleService.Get(roleSystemId);
                    if (role.Id != RolesConstants.RoleOrderApprover &&
                        role.Id != RolesConstants.RoleOrderPlacer)
                    {
                        continue;
                    }
                    if (organizationIds.Contains(personToOrganizationLink.OrganizationSystemId))
                    {
                        continue;
                    }
                    organizationIds.Add(personToOrganizationLink.OrganizationSystemId);
                    break;
                }
            }

            if (organizationIds.Count <= 0)
            {
                return false;
            }

            organizations.AddRange(organizationIds.Select(x => _organizationService.Get(x)));
            _personStorage.CurrentSelectedOrganization = organizations[0];
            return true;
        }

        public override bool IsValidLoginForm(ModelStateDictionary modelState, LoginFormViewModel loginForm)
        {
            var prefix = nameof(loginForm);
            var userNameField = $"{prefix}.{nameof(loginForm.UserName)}";
            var passwordField = $"{prefix}.{nameof(loginForm.Password)}";

            var validationRules = new List<ValidationRuleItem<LoginFormViewModel>>()
            {
                new ValidationRuleItem<LoginFormViewModel>{Field = userNameField, Rule = model => !string.IsNullOrEmpty(model.UserName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<LoginFormViewModel>{Field = passwordField, Rule = model => !string.IsNullOrEmpty(model.Password), ErrorMessage = () => "validation.required".AsWebsiteText()}
            };

            return loginForm.IsValid(validationRules, modelState);
        }

        public override bool IsValidForgotPasswordForm(ModelStateDictionary modelState, ForgotPasswordFormViewModel forgotPasswordForm)
        {
            var prefix = nameof(forgotPasswordForm);
            var emailField = $"{prefix}.{nameof(forgotPasswordForm.Email)}";

            var validationRules = new List<ValidationRuleItem<ForgotPasswordFormViewModel>>()
            {
                new ValidationRuleItem<ForgotPasswordFormViewModel>{Field = emailField, Rule = model => !string.IsNullOrEmpty(model.Email), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<ForgotPasswordFormViewModel>{Field = emailField, Rule = model => _userValidationService.IsValidEmail(model.Email), ErrorMessage = () => "validation.email".AsWebsiteText()}
            };

            return forgotPasswordForm.IsValid(validationRules, modelState);
        }

        public override void SendMagicLink(Person user, Guid channelSystemId, string url)
        {
            var model = new MagicLinkData
            {
                ChannelSystemId = channelSystemId,
                Expire = DateTimeOffset.Now.AddMinutes(5),
                PersonSystemId = user.SystemId,
            };

            var modelString = _applicationJsonConverter.ConvertObjectToJson(model);
            var protectedLink = _magicLinkProtector.Protect(System.Text.Encoding.UTF8.GetBytes(modelString));
            var magicLink = Base64UrlTextEncoder.Encode(protectedLink);

            var emailModel = new ForgotPasswordEmailModel
            {
                Expires = model.Expire,
                Link = url.Replace("{link}", magicLink),
            };

            var emailDefinition = _forgotPasswordEmailDefinitionResolver.Get(emailModel, user.Email, user);
            _mailService.SendEmail(emailDefinition, false);
        }

        public override bool LoginWithMagicLink(string link, Guid channelSystemId, out Person person)
        {
            person = default;

            MagicLinkData model;
            try
            {
                var protectedLink = Base64UrlTextEncoder.Decode(link);
                var modelString = Encoding.UTF8.GetString(_magicLinkProtector.Unprotect(protectedLink));
                model = _applicationJsonConverter.ConvertJsonToObject<MagicLinkData>(modelString);
            }
            catch
            {
                return false;
            }

            if (model is null
                || model.Expire < DateTimeOffset.Now
                || model.ChannelSystemId != channelSystemId)
            {
                return false;
            }

            person = _personService.Get(model.PersonSystemId);
            if (person is null)
            {
                return false;
            }

            var claimsIdentity = _securityContextService.CreateClaimsIdentity(person.LoginCredential.Username, person.SystemId);
            _securityContextService.ActAs(claimsIdentity, extraInfo: "magic-link");
            _authenticationService.RefreshSignIn();
            _eventBroker.Publish(EventScope.Context, new PersonSignedIn(person.SystemId));

            return true;
        }

        private class MagicLinkData
        {
            public Guid PersonSystemId { get; set; }
            public DateTimeOffset Expire { get; set; }
            public Guid ChannelSystemId { get; set; }
        }
    }
}
