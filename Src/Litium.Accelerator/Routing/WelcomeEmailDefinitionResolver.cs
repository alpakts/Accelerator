using Litium.Accelerator.Caching;
using Litium.Accelerator.Mailing;
using Litium.Accelerator.Mailing.Models;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Globalization;
using Litium.Websites;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Routing
{
    [Service(ServiceType = typeof(WelcomeEmailDefinitionResolver))]
    public class WelcomeEmailDefinitionResolver
    {
        private readonly PageByFieldTemplateCache<WelcomeEmailPageTemplateCache> _pageByFieldType;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly ILogger<WelcomeEmailDefinitionResolver> _logger;

        private ChannelModel ChannelModel => _requestModelAccessor.RequestModel.ChannelModel;

        public WelcomeEmailDefinitionResolver(PageByFieldTemplateCache<WelcomeEmailPageTemplateCache> pageByFieldType, RequestModelAccessor requestModelAccessor, ILogger<WelcomeEmailDefinitionResolver> logger)
        {
            _pageByFieldType = pageByFieldType;
            _requestModelAccessor = requestModelAccessor;
            _logger = logger;
        }

        public WelcomeEmailDefinition Get(WelcomeEmailModel model, string toEmail)
        {
            var page = GetWelcomeEmailPage();
            if (page == null)
            {
                _logger.LogError("Cannot find the welcome email page at channel({SystemId}) and website({WebsiteSystemId}). Cannot send email to {ToEmail}.", ChannelModel.SystemId, ChannelModel.Channel.WebsiteSystemId, toEmail);
            }

            return new WelcomeEmailDefinition(page, ChannelModel.SystemId, model, toEmail);
        }

        private Page GetWelcomeEmailPage()
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
