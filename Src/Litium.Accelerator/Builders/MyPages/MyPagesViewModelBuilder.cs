using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.MyPages;
using Litium.Customers;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Security;
using Litium.Web.Models.Websites;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Litium.Accelerator.Builders.MyPages
{
    public class MyPagesViewModelBuilder : IViewModelBuilder<MyPagesViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly SecurityContextService _securityContextService;
        private readonly CountryService _countryService;
        private readonly PersonStorage _personStorage;
        private readonly IsoCountryService _isoCountryService;
        private readonly PersonService _personService;

        public MyPagesViewModelBuilder(
            RequestModelAccessor requestModelAccessor,
            SecurityContextService securityContextService,
            PersonService personService,
            CountryService countryService,
            PersonStorage personStorage,
            IsoCountryService isoCountryService)
        {
            _requestModelAccessor = requestModelAccessor;
            _securityContextService = securityContextService;
            _countryService = countryService;
            _personStorage = personStorage;
            _isoCountryService = isoCountryService;
            _personService = personService;
        }

        public virtual MyPagesViewModel Build()
            => Build(_requestModelAccessor.RequestModel.CurrentPageModel);

        public virtual MyPagesViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<MyPagesViewModel>();
            var currentPerson = GetCurrentPerson();
            var currentOrganization = _personStorage.CurrentSelectedOrganization;

            model.IsBusinessCustomer = currentOrganization != null;
            if (model.IsBusinessCustomer)
            {
                model.BusinessCustomerDetailsPanel = GetBusinessCustomerDetailsPanel(pageModel, currentPerson, currentOrganization);
            }
            else
            {
                model.MyDetailsPanel = GetMyDetailsPanel(pageModel, currentPerson);
            }

            if(model.MayUserEditLogin)
            {
                model.LoginInfoPanel = GetLoginInfoPanel(pageModel, currentPerson);
            }

            model.HasApproverRole = _personStorage.HasApproverRole;

            return model;
        }

        public virtual MyPagesViewModel Build(MyDetailsViewModel myDetailsPanel)
        {
            var model = Build();
            myDetailsPanel.Countries = model.MyDetailsPanel.Countries;
            model.MyDetailsPanel = myDetailsPanel;

            return model;
        }

        public virtual MyPagesViewModel Build(BusinessCustomerDetailsViewModel businessCustomerDetailsPanel)
        {
            var model = Build();
            model.BusinessCustomerDetailsPanel = businessCustomerDetailsPanel;

            return model;
        }

        private MyDetailsViewModel GetMyDetailsPanel(PageModel pageModel, Customers.Person currentPerson)
        {
            var myDetailsPanel = pageModel.MapTo<MyDetailsViewModel>();
            myDetailsPanel.MapFrom(currentPerson);
            myDetailsPanel.Countries = GetCountries();
            myDetailsPanel.IsSystemAccount = currentPerson.SystemId == SecurityContextService.System.SystemId;
            return myDetailsPanel;
        }

        private BusinessCustomerDetailsViewModel GetBusinessCustomerDetailsPanel(PageModel pageModel, Customers.Person currentPerson, Organization currentOrganization)
        {
            var businessCustomerDetailsPanel = pageModel.MapTo<BusinessCustomerDetailsViewModel>();
            businessCustomerDetailsPanel.MapFrom(currentPerson);
            businessCustomerDetailsPanel.MapFrom(currentOrganization);

            return businessCustomerDetailsPanel;
        }

        private LoginInfoViewModel GetLoginInfoPanel(PageModel pageModel, Customers.Person currentPerson)
        {
            var loginInfoPanel = pageModel.MapTo<LoginInfoViewModel>();
            loginInfoPanel.UserNameForm = currentPerson.MapTo<ChangeUserNameFormViewModel>();
            loginInfoPanel.IsSystemAccount = currentPerson.SystemId == SecurityContextService.System.SystemId;
            return loginInfoPanel;
        }

        private IEnumerable<SelectListItem> GetCountries()
        {
            var countries = _countryService.GetAll().Where(x => _requestModelAccessor.RequestModel.ChannelModel.Channel.CountryLinks.Any(y => y.CountrySystemId == x.SystemId)).Select(country => new SelectListItem
                {
                    Text = _isoCountryService.Get(country.Id)?.EnglishName,
                    Value = country.Id
                });
            return countries;
        }

        private Customers.Person GetCurrentPerson()
        {
            var currentPersonId = _securityContextService.GetIdentityUserSystemId();
            if (!currentPersonId.HasValue)
            {
                return null;
            }

            return _personService.Get(currentPersonId.Value);
        }
    }
}
