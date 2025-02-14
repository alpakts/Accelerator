using Litium.Accelerator.Caching;
using Litium.Accelerator.Mailing;
using Litium.Accelerator.Mailing.Models;
using Litium.Customers;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Globalization;
using Litium.Websites;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Routing
{
    [Service(ServiceType = typeof(ForgotPasswordEmailDefinitionResolver))]
    public class ForgotPasswordEmailDefinitionResolver
    {
        private readonly PageByFieldTemplateCache<ForgotPasswordPageTemplateCache> _pageByFieldType;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ILogger _logger;

        private ChannelModel ChannelModel => _requestModelAccessor.RequestModel.ChannelModel;

        public ForgotPasswordEmailDefinitionResolver(
            PageByFieldTemplateCache<ForgotPasswordPageTemplateCache> pageByFieldType,
            RequestModelAccessor requestModelAccessor,
            ILogger<ForgotPasswordEmailDefinitionResolver> logger)
        {
            _pageByFieldType = pageByFieldType;
            _requestModelAccessor = requestModelAccessor;
            _logger = logger;
        }

        public ForgotPasswordEmailDefinition Get(ForgotPasswordEmailModel model, string toEmail, Person person)
        {
            var page = GetPage();
            if (page == null)
            {
                _logger.LogError("Cannot find the forgot password email page at channel({SystemId}) and website({WebsiteSystemId}). Cannot send email to {ToEmail}.", ChannelModel.SystemId, ChannelModel.Channel.WebsiteSystemId, toEmail);
            }

            return new ForgotPasswordEmailDefinition(page, ChannelModel.SystemId, model, toEmail, person);
        }

        private Page GetPage()
        {
            Page result = null;
            _pageByFieldType.TryFindPage(page =>
            {
                result = page;
                return true;
            });

            return result;
        }
    }
}
