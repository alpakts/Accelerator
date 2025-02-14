using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Litium.Accelerator.Builders.Login;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Services;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Login;
using Litium.Customers;
using Litium.Globalization;
using Litium.Sales;
using Litium.Security;
using Litium.Web;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Login
{
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly MyPagesViewModelService _myPagesViewModelService;
        private readonly LoginViewModelBuilder _loginViewModelBuilder;
        private readonly ForgotPasswordViewModelBuilder _forgotPasswordViewModelBuilder;
        private readonly OrganizationService _organizationService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly AddressTypeService _addressTypeService;
        private readonly PersonStorage _personStorage;
        private readonly CountryService _countryService;
        private readonly SecurityContextService _securityContextService;
        private readonly PersonService _personService;
        private readonly UrlService _urlService;

        public LoginController(
            LoginService loginService,
            LoginViewModelBuilder loginViewModelBuilder,
            ForgotPasswordViewModelBuilder forgotPasswordViewModelBuilder,
            OrganizationService organizationService,
            MyPagesViewModelService myPagesViewModelService,
            RequestModelAccessor requestModelAccessor,
            AddressTypeService addressTypeService,
            CountryService countryService,
            SecurityContextService securityContextService,
            PersonStorage personStorage,
            PersonService personService,
            UrlService urlService)
        {
            _loginService = loginService;
            _loginViewModelBuilder = loginViewModelBuilder;
            _forgotPasswordViewModelBuilder = forgotPasswordViewModelBuilder;
            _organizationService = organizationService;
            _myPagesViewModelService = myPagesViewModelService;
            _requestModelAccessor = requestModelAccessor;
            _addressTypeService = addressTypeService;
            _personStorage = personStorage;
            _personService = personService;
            _urlService = urlService;
            _countryService = countryService;
            _securityContextService = securityContextService;
        }

        [HttpGet]
        public virtual ActionResult Login(string redirectUrl, string code)
        {
            var model = _loginViewModelBuilder.Build(redirectUrl);
            if (code != null && int.TryParse(code, out int resultCode) && Enum.IsDefined(typeof(HttpStatusCode), resultCode))
            {
                model.ErrorMessage = $"error.{resultCode}".AsWebsiteText();
                if (User.Identity.IsAuthenticated && (resultCode == (int)HttpStatusCode.Unauthorized || resultCode == (int)HttpStatusCode.Forbidden))
                {
                    model.InsufficientPermissions = true;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            var model = _loginViewModelBuilder.Build(loginViewModel);

            if (Uri.TryCreate(loginViewModel.RedirectUrl, UriKind.RelativeOrAbsolute, out var uri))
            {
                if (uri.IsAbsoluteUri)
                {
                    model.RedirectUrl = uri.PathAndQuery;
                }
                else
                {
                    model.RedirectUrl = loginViewModel.RedirectUrl;
                }
            }
            else
            {
                model.RedirectUrl = "~/";
            }

            if (!_loginService.IsValidLoginForm(ModelState, model.LoginForm))
            {
                return View(model);
            }

            var cartContext = HttpContext.GetCartContext();
            try
            {
                var loginSuccessfull = _loginService.Login(model.LoginForm.UserName, model.LoginForm.Password);
                if (loginSuccessfull)
                {
                    var person = _loginService.GetUser(model.LoginForm.UserName);
                    var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);

                    if (!_loginService.IsBusinessCustomer(person, out var organizations))
                    {
                        await SetPersonAsync(cartContext, person);
                        await SetCountryByAddressAsync(cartContext, person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId));
                        return new RedirectResult(model.RedirectUrl);
                    }

                    if (organizations.Count == 1)
                    {
                        await SetOrganizationAsync(cartContext, organizations.First().SystemId);
                        await SetCountryByAddressAsync(cartContext, organizations.First().Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId));
                        return new RedirectResult(model.RedirectUrl);
                    }

                    return RedirectToAction(nameof(SelectOrganization), new { redirectUrl = model.RedirectUrl });
                }
                model.ErrorMessage = "login.failed".AsWebsiteText();
            }
            catch (ChangePasswordException)
            {
                return View(nameof(ChangePassword), model);
            }

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult SelectOrganization(string redirectUrl)
        {
            var model = new LoginViewModel();
            var currentPersonId = _securityContextService.GetIdentityUserSystemId();
            if (currentPersonId.HasValue)
            {
                var person = _personService.Get(currentPersonId.Value);
                if (_loginService.IsBusinessCustomer(person, out var organizations))
                {
                    model.Organizations = _loginViewModelBuilder.GetOrganizations(organizations);
                    model.RedirectUrl = redirectUrl;
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LoginViewModel loginViewModel)
        {
            if (string.IsNullOrWhiteSpace(loginViewModel.LoginForm.UserName) || string.IsNullOrWhiteSpace(loginViewModel.LoginForm.Password))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = _loginViewModelBuilder.Build(loginViewModel);
            if (Uri.TryCreate(loginViewModel.RedirectUrl, UriKind.RelativeOrAbsolute, out var uri))
            {
                if (uri.IsAbsoluteUri)
                {
                    model.RedirectUrl = uri.PathAndQuery;
                }
                else
                {
                    model.RedirectUrl = loginViewModel.RedirectUrl;
                }
            }
            else
            {
                model.RedirectUrl = "~/";
            }

            if (!_myPagesViewModelService.IsValidPasswordForm(ModelState, model.PasswordForm,
                model.LoginForm.Password))
            {
                return View(model);
            }

            var loginSuccessfull = _loginService.Login(model.LoginForm.UserName, model.LoginForm.Password, model.PasswordForm.Password);
            if (!loginSuccessfull)
            {
                return View(model);
            }
            var person = _loginService.GetUser(model.LoginForm.UserName);
            if (!_loginService.IsBusinessCustomer(person, out var organizations))
            {
                return new RedirectResult(model.RedirectUrl);
            }

            if (organizations.Count <= 1)
            {
                return new RedirectResult(model.RedirectUrl);
            }
            model.Organizations = _loginViewModelBuilder.GetOrganizations(organizations);
            return View(nameof(SelectOrganization), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectOrganization(LoginViewModel loginViewModel)
        {
            var cartContext = HttpContext.GetCartContext();
            await SetOrganizationAsync(cartContext, loginViewModel.SelectedOrganization);
            if (_personStorage.CurrentSelectedOrganization is Organization organization)
            {
                var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                await SetCountryByAddressAsync(cartContext, organization.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId));
                string redirectUrl;
                if (Uri.TryCreate(loginViewModel.RedirectUrl, UriKind.RelativeOrAbsolute, out var uri))
                {
                    if (uri.IsAbsoluteUri)
                    {
                        redirectUrl = uri.PathAndQuery;
                    }
                    else
                    {
                        redirectUrl = loginViewModel.RedirectUrl;
                    }
                }
                else
                {
                    redirectUrl = "/";
                }
                return new RedirectResult(redirectUrl);
            }

            _loginService.Logout();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<ActionResult> ForgotPassword(string redirectUrl, string link)
        {
            var model = _forgotPasswordViewModelBuilder.Build();
            if (Uri.TryCreate(redirectUrl, UriKind.RelativeOrAbsolute, out var uri))
            {
                if (uri.IsAbsoluteUri)
                {
                    model.RedirectUrl = uri.PathAndQuery;
                }
                else
                {
                    model.RedirectUrl = redirectUrl;
                }
            }
            else
            {
                model.RedirectUrl = "/";
            }

            if (!string.IsNullOrEmpty(link))
            {
                var requestModel = _requestModelAccessor.RequestModel;
                if (!string.IsNullOrWhiteSpace(link)
                    && _loginService.LoginWithMagicLink(link, requestModel.ChannelModel.SystemId, out var person))
                {
                    var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);

                    var cartContext = HttpContext.GetCartContext();
                    if (!_loginService.IsBusinessCustomer(person, out var organizations))
                    {
                        await SetPersonAsync(cartContext, person);
                        await SetCountryByAddressAsync(cartContext, person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId));
                        return Redirect(model.RedirectUrl);
                    }

                    if (organizations.Count == 1)
                    {
                        await SetOrganizationAsync(cartContext, organizations.First().SystemId);
                        await SetCountryByAddressAsync(cartContext, organizations.First().Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId));
                        return Redirect(model.RedirectUrl);
                    }

                    return RedirectToAction(nameof(SelectOrganization), new { redirectUrl = model.RedirectUrl });
                }

                model.ErrorMessage = "login.magiclink.expired".AsWebsiteText();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            var model = _forgotPasswordViewModelBuilder.Build(forgotPasswordViewModel.ForgotPasswordForm);

            if (_loginService.IsValidForgotPasswordForm(ModelState, model.ForgotPasswordForm))
            {
                var user = _loginService.GetUser(model.ForgotPasswordForm.Email?.ToLower());

                if (user is not null)
                {
                    var link = _urlService.GetUrl(
                        _requestModelAccessor.RequestModel.CurrentPageModel.Page,
                        new PageUrlArgs(model.ChannelSystemId) { AbsoluteUrl = true });

                    var magicLink = $"{link}?RedirectUrl={WebUtility.UrlEncode(forgotPasswordViewModel.RedirectUrl)}&link={{link}}";
                    _loginService.SendMagicLink(user, model.ChannelSystemId, magicLink);
                }

                model.Message = "login.magiclink.sent".AsWebsiteText();
            }

            return View(model);
        }

        [HttpGet]
        public RedirectResult Logout(string redirectUrl)
        {
            _loginService.Logout();

            if (Uri.TryCreate(redirectUrl, UriKind.RelativeOrAbsolute, out var uri))
            {
                if (uri.IsAbsoluteUri)
                {
                    redirectUrl = uri.PathAndQuery;
                }
            }
            else
            {
                redirectUrl = "~/";
            }

            return new RedirectResult(redirectUrl);
        }

        private async Task SetCountryByAddressAsync(CartContext cartContext, Customers.Address address)
        {
            //Check if the country in the address is same as channel has.
            if (address != null && !string.IsNullOrEmpty(address.Country) && !address.Country.Equals(_requestModelAccessor.RequestModel.CountryModel.Country.Id, StringComparison.CurrentCultureIgnoreCase))
            {
                var country = _countryService.Get(address.Country);
                //Check if country is connected to the channel
                if (country != null && _requestModelAccessor.RequestModel.ChannelModel.Channel.CountryLinks.Any(x => x.CountrySystemId == country.SystemId))
                {
                    // Set user's country to the channel
                    await cartContext.SelectCountryAsync(new SelectCountryArgs { CountryCode = country.Id });
                }
            }
        }

        private async Task SetOrganizationAsync(CartContext cartContext, Guid organizationSystemId)
        {
            _personStorage.CurrentSelectedOrganizationSystemId = organizationSystemId;
            var personId = _securityContextService.GetIdentityUserSystemId().GetValueOrDefault();
            if (cartContext is not null
                && (cartContext.OrganizationSystemId != organizationSystemId || cartContext.PersonSystemId != personId))
            {
                await cartContext.ChangeCustomerAsync(new ChangeCustomerArgs
                {
                    CustomerNumber = _personStorage.CurrentSelectedOrganization.Id,
                    CustomerType = CustomerType.Organization,
                    PersonSystemId = personId,
                    OrganizationSystemId = organizationSystemId
                });
            }
        }

        private async Task SetPersonAsync(CartContext cartContext, Person person)
        {
            if (cartContext != null && cartContext.PersonSystemId != person.SystemId)
            {
                await cartContext.ChangeCustomerAsync(new ChangeCustomerArgs
                {
                    CustomerNumber = person.Id,
                    CustomerType = CustomerType.Person,
                    PersonSystemId = person.SystemId,
                    OrganizationSystemId = null
                });
            }
        }
    }
}
