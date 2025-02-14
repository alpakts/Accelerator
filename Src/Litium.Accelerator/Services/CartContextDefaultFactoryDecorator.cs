using System;
using System.Threading;
using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.Accelerator.Utilities;
using Litium.Common;
using Litium.Globalization;
using Litium.Runtime.DependencyInjection;
using Litium.Sales;
using Litium.Security;
using Microsoft.AspNetCore.Http;

namespace Litium.Accelerator.Services
{
    [ServiceDecorator(typeof(CartContextDefaultFactory))]
    public class CartContextDefaultFactoryDecorator : CartContextDefaultFactory
    {
        private readonly CartContextDefaultFactory _parent;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PersonStorage _personStorage;
        private readonly KeyLookupService _keyLookupService;
        private readonly SecurityContextService _securityContextService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartContextDefaultFactoryDecorator(
            CartContextDefaultFactory parent,
            RequestModelAccessor requestModelAccessor,
            PersonStorage personStorage,
            KeyLookupService keyLookupService,
            SecurityContextService securityContextService,
            IHttpContextAccessor httpContextAccessor)
        {
            _parent = parent;
            _requestModelAccessor = requestModelAccessor;
            _personStorage = personStorage;
            _keyLookupService = keyLookupService;
            _securityContextService = securityContextService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<CreateCartContextArgs> CreateAsync(CancellationToken cancellationToken = default)
        {
            var requestModel = _requestModelAccessor.RequestModel;
            if (requestModel != null)
            {
                return Task.FromResult(new CreateCartContextArgs
                {
                    ChannelSystemId = requestModel.ChannelModel.SystemId,
                    MarketSystemId = requestModel.ChannelModel.Channel.MarketSystemId ?? Guid.Empty,
                    CountryCode = requestModel.CountryModel.Country.Id,
                    CurrencyCode = _keyLookupService.TryGetId<Currency>(requestModel.CountryModel.Country.CurrencySystemId, out var currencyCode) ? currencyCode : null,
                    PersonSystemId = _securityContextService.GetIdentityUserSystemId() ?? null,
                    OrganizationSystemId = _personStorage.CurrentSelectedOrganizationSystemId,
                    ClientIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    ClientBrowser = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(),
                    ShowPricesWithVat = requestModel.ChannelModel.Channel.ShowPricesWithVat
                });
            }

            return _parent.CreateAsync(cancellationToken);
        }
    }
}
