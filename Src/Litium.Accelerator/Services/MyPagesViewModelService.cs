using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.MyPages;
using Litium.Customers;
using Litium.Runtime.AutoMapper;
using Litium.Sales;
using Litium.Security;
using Litium.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Litium.Accelerator.Services
{
    public class MyPagesViewModelService : ViewModelService<MyPagesViewModel>
    {
        private readonly SecurityContextService _securityContextService;
        private readonly PersonService _personService;
        private readonly AuthenticationService _authenticationService;
        private readonly UserValidationService _userValidationService;
        private readonly AddressTypeService _addressTypeService;
        private readonly CheckoutState _checkoutState;
        private readonly CartContextAccessor _cartContextAccessor;
        private readonly RequestModelAccessor _requestModelAccessor;

        public MyPagesViewModelService(
            SecurityContextService securityContextService,
            PersonService personService,
            AuthenticationService authenticationService,
            UserValidationService userValidationService,
            AddressTypeService addressTypeService, 
            CartContextAccessor cartContextAccessor,
            RequestModelAccessor requestModelAccessor,
            CheckoutState checkoutState)
        {
            _securityContextService = securityContextService;
            _personService = personService;
            _authenticationService = authenticationService;
            _userValidationService = userValidationService;
            _addressTypeService = addressTypeService;
            _cartContextAccessor = cartContextAccessor;
            _checkoutState = checkoutState;
            _requestModelAccessor = requestModelAccessor;
        }

        public virtual async Task SaveMyDetails(IViewModel viewModel, bool checkCountry = true)
        {
            var person = _personService.Get(_securityContextService.GetIdentityUserSystemId().GetValueOrDefault())?.MakeWritableClone();
            if (person == null) return;

            person.MapFrom(viewModel);

            using (_securityContextService.ActAsSystem())
            {
                _personService.Update(person);
            }

            if (checkCountry)
            {
                var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                var address = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                //Check if user has the same country in the address as channel has.
                if (address != null && !address.Country.Equals(_requestModelAccessor.RequestModel.CountryModel.Country.Id, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Set user's country to the channel
                    await _cartContextAccessor.CartContext.SelectCountryAsync(new SelectCountryArgs { CountryCode = address.Country });
                }
            }
            
            _checkoutState.ClearState();
        }

        public void SaveUserName(string userName)
        {
            var person = _personService.Get(_securityContextService.GetIdentityUserSystemId().GetValueOrDefault())?.MakeWritableClone();
            if (person == null) return;

            person.LoginCredential.Username = userName;

            using (_securityContextService.ActAsSystem())
            {
                _personService.Update(person);
            }

            _authenticationService.RefreshSignIn();
            _checkoutState.ClearState();
        }

        public void SavePassword(string password)
        {
            var person = _personService.Get(_securityContextService.GetIdentityUserSystemId().GetValueOrDefault()).MakeWritableClone();
            if (person == null) return;

            person.LoginCredential.NewPassword = password;

            using (_securityContextService.ActAsSystem())
            {
                _personService.Update(person);
            }
        }

        public bool IsValidMyDetailsForm(ModelStateDictionary modelState, MyDetailsViewModel myDetailsForm)
        {
            var firstNameField = nameof(myDetailsForm.FirstName);
            var lastNameField = nameof(myDetailsForm.LastName);

            var validationRules = new List<ValidationRuleItem<MyDetailsViewModel>>()
            {
                new ValidationRuleItem<MyDetailsViewModel>{Field = firstNameField, Rule = model => !string.IsNullOrEmpty(model.FirstName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = lastNameField, Rule = model => !string.IsNullOrEmpty(model.LastName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "DeliveryAddress.CareOf", Rule = model => string.IsNullOrEmpty(model.DeliveryAddress.CareOf) || model.DeliveryAddress.CareOf?.Length <= 100
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 100).ToString()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "DeliveryAddress.ZipCode", Rule = model => string.IsNullOrEmpty(model.DeliveryAddress.ZipCode) || model.DeliveryAddress.ZipCode?.Length <= 50
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 50).ToString()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "DeliveryAddress.City", Rule = model => string.IsNullOrEmpty(model.DeliveryAddress.City) || model.DeliveryAddress.City?.Length <= 100
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 100).ToString()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "DeliveryAddress.Address", Rule = model => string.IsNullOrEmpty(model.DeliveryAddress.Address) || model.DeliveryAddress.Address?.Length <= 200
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 200).ToString()},

                new ValidationRuleItem<MyDetailsViewModel>{Field = "AlternativeDeliveryAddress.CareOf", Rule = model => string.IsNullOrEmpty(model.AlternativeDeliveryAddress.CareOf) || model.AlternativeDeliveryAddress.CareOf?.Length <= 100
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 100).ToString()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "AlternativeDeliveryAddress.ZipCode", Rule = model => string.IsNullOrEmpty(model.AlternativeDeliveryAddress.ZipCode) || model.AlternativeDeliveryAddress.ZipCode?.Length <= 50
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 50).ToString()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "AlternativeDeliveryAddress.City", Rule = model => string.IsNullOrEmpty(model.AlternativeDeliveryAddress.City) || model.AlternativeDeliveryAddress.City?.Length <= 100
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 100).ToString()},
                new ValidationRuleItem<MyDetailsViewModel>{Field = "AlternativeDeliveryAddress.Address", Rule = model => string.IsNullOrEmpty(model.AlternativeDeliveryAddress.Address) || model.AlternativeDeliveryAddress.Address?.Length <= 200
                    , ErrorMessage = () => new StringBuilder().AppendFormat("person.address.exceedslimit".AsWebsiteText(), 200).ToString()},
            };

            return myDetailsForm.IsValid(validationRules, modelState);
        }

        public bool IsValidBusinessCustomerDetailsForm(ModelStateDictionary modelState, BusinessCustomerDetailsViewModel businessCustomerDetailsForm)
        {
            var firstNameField = nameof(businessCustomerDetailsForm.FirstName);
            var lastNameField = nameof(businessCustomerDetailsForm.LastName);
            var phoneField = nameof(businessCustomerDetailsForm.Phone);

            var validationRules = new List<ValidationRuleItem<BusinessCustomerDetailsViewModel>>()
            {
                new ValidationRuleItem<BusinessCustomerDetailsViewModel>{Field = firstNameField, Rule = model => !string.IsNullOrEmpty(model.FirstName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessCustomerDetailsViewModel>{Field = lastNameField, Rule = model => !string.IsNullOrEmpty(model.LastName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessCustomerDetailsViewModel>{Field = phoneField, Rule = model => !string.IsNullOrEmpty(model.Phone), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<BusinessCustomerDetailsViewModel>{Field = phoneField, Rule = model => _userValidationService.IsValidPhone(model.Phone), ErrorMessage = () => "validation.phone".AsWebsiteText()},
            };

            return businessCustomerDetailsForm.IsValid(validationRules, modelState);
        }

        public bool IsValidUserNameForm(ModelStateDictionary modelState, ChangeUserNameFormViewModel userNameForm)
        {
            var prefix = nameof(userNameForm);
            var userNameField = $"{prefix}.{nameof(userNameForm.UserName)}";

            var validationRules = new List<ValidationRuleItem<ChangeUserNameFormViewModel>>()
            {
                new ValidationRuleItem<ChangeUserNameFormViewModel>{Field = userNameField, Rule = model => !string.IsNullOrEmpty(model.UserName), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<ChangeUserNameFormViewModel>{Field = userNameField, Rule = model => _userValidationService.IsValidEmail(model.UserName), ErrorMessage = () => "validation.email".AsWebsiteText()},
                new ValidationRuleItem<ChangeUserNameFormViewModel>{Field = userNameField, Rule = model => _userValidationService.IsValidUserName(model.UserName), ErrorMessage = () => "validation.invalidusername".AsWebsiteText()}
            };

            return userNameForm.IsValid(validationRules, modelState);
        }

        public bool IsValidPasswordForm(ModelStateDictionary modelState, ChangePasswordFormViewModel passwordForm, string oldPassword = null)
        {
            var prefix = nameof(passwordForm);
            var passwordField = $"{prefix}.{nameof(passwordForm.Password)}";

            var validationRules = new List<ValidationRuleItem<ChangePasswordFormViewModel>>()
            {
                new ValidationRuleItem<ChangePasswordFormViewModel>{Field = passwordField, Rule = model => !string.IsNullOrEmpty(model.Password), ErrorMessage = () => "validation.required".AsWebsiteText()},
                new ValidationRuleItem<ChangePasswordFormViewModel>{Field = passwordField, Rule = model => oldPassword == null || !model.Password.Equals(oldPassword), ErrorMessage = () => "changepassword.newpasswordequalstheoldpassword".AsWebsiteText()},
                new ValidationRuleItem<ChangePasswordFormViewModel>{Field = passwordField, Rule = model => _userValidationService.IsValidPassword(model.Password), ErrorMessage = () => "changepassword.invalidpasswordformat".AsWebsiteText()},
                new ValidationRuleItem<ChangePasswordFormViewModel>{Field = passwordField, Rule = model => _userValidationService.IsPasswordMatch(model.Password, model.ConfirmPassword), ErrorMessage = () => "changepassword.passwordconfirmationdoesnotmatch".AsWebsiteText()},
                new ValidationRuleItem<ChangePasswordFormViewModel>{Field = passwordField, Rule = model => _userValidationService.IsValidPasswordComplexity(model.Password), ErrorMessage = () => "changepassword.weakpassword".AsWebsiteText()},
            };

            return passwordForm.IsValid(validationRules, modelState);
        }
    }
}
