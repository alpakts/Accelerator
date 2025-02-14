using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Litium.Runtime.DependencyInjection;
using Litium.Security;

namespace Litium.Accelerator.Services
{
    [Service(ServiceType = typeof(UserValidationService))]
    public class UserValidationService
    {
        private readonly EmailAddressAttribute _emailValidator = new EmailAddressAttribute();
        private readonly SecurityContextService _securityContextService;
        private readonly PasswordService _passwordService;
        private readonly Regex _phoneReqex = new Regex(@"^(\+?)\(?\d{2,3}\)? *-? *\d{2,4} *-? *\d{2,4} *-? *\d{2,4}$", RegexOptions.Compiled);

        public UserValidationService(
            SecurityContextService securityContextService,
            PasswordService passwordService)
        {
            _securityContextService = securityContextService;
            _passwordService = passwordService;
        }

        public bool IsValidUserName(string userName)
        {
            var validUsername = false;

            if (!string.IsNullOrEmpty(userName))
            {
                var user = _securityContextService.GetPersonSystemId(userName);
                validUsername = user == null || user.Equals(_securityContextService.GetIdentityUserSystemId().GetValueOrDefault());
            }

            return validUsername;
        }

        public bool IsValidEmail(string email)
        {
            var validEmail = false;

            if (!string.IsNullOrEmpty(email))
            {
                validEmail = _emailValidator.IsValid(email);
            }

            return validEmail;
        }

        public bool IsValidPhone(string phone)
        {
            var validPhone = false;

            if (!string.IsNullOrEmpty(phone))
            {
                validPhone = _phoneReqex.IsMatch(phone);
            }

            return validPhone;
        }

        public bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password) && PasswordStartsAndEndsWithLegalCharacters(password);
        }

        public bool IsPasswordMatch(string password, string confirmPassword)
        {
            return password.Equals(confirmPassword);
        }

        public bool IsValidPasswordComplexity(string password)
        {
            var validPassword = false;
            
            if (!string.IsNullOrEmpty(password))
            {
                validPassword = _passwordService.ValidateComplexity(password);
            }

            return validPassword;
        }

        private bool PasswordStartsAndEndsWithLegalCharacters(string password)
        {
            return password.Trim().Length.Equals(password.Length);
        }
    }
}
