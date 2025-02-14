using Litium.Accelerator.Caching;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Websites;
using System;
using System.Globalization;

namespace Litium.Accelerator.Mailing
{
    public class OrderConfirmationEmail : PageMailDefinition
    {
        private static readonly LazyService<PageByFieldTemplateCache<OrderConfirmationPageByFieldTemplateCache>> _orderConfirmationService
            = new LazyService<PageByFieldTemplateCache<OrderConfirmationPageByFieldTemplateCache>>();

        private readonly Guid _channelSystemId;
        private readonly Guid _orderId;
        private readonly string _toEmail;
        private Page _page;

        public OrderConfirmationEmail(Guid channelSystemId, Guid orderId, string toEmail)
        {
            _channelSystemId = channelSystemId;
            _orderId = orderId;
            _toEmail = toEmail;
        }

        public override Guid ChannelSystemId => _channelSystemId;

        public override string ToEmail => _toEmail;

        public override string Subject
        {
            get
            {
                var channel = _channelSystemId.MapTo<Channel>();
                var cultureInfo = channel.WebsiteLanguageSystemId.MapTo<Language>()?.CultureInfo;
                if (cultureInfo != null)
                {
                    CultureInfo.CurrentUICulture = cultureInfo;
                }
                return "orderconfirmation.emailsubject".AsWebsiteText(channel.WebsiteSystemId.MapTo<Website>());
            }
        }

        public override Page Page
        {
            get
            {
                if (_page == null)
                {
                    _orderConfirmationService.Value.TryGetPage(_channelSystemId, orderConfirmation =>
                    {
                        _page = orderConfirmation;
                        return true;
                    }, out _);
                }

                return _page;
            }
        }

        public override string UrlTransform(string url)
        {
            return $"{url}?orderId={_orderId}&isEmail={true}";
        }
    }
}
