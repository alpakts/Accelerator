using System;
using System.Globalization;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Mailing.Models;
using Litium.Websites;

namespace Litium.Accelerator.Mailing
{
    public class WelcomeEmailDefinition : HtmlMailDefinition<WelcomeEmailModel>
    {
        private readonly Page _page;

        public WelcomeEmailDefinition(Page page, Guid channelSystemId, WelcomeEmailModel model, string toEmail) : base(channelSystemId, model, toEmail)
        {
            _page = page;
        }

        protected override string RawBodyText => _page.Fields.GetValue<string>(WelcomeEmailPageFieldNameConstants.WelcomeEmailText, CultureInfo.CurrentUICulture);
        protected override string RawSubjectText => _page.Fields.GetValue<string>(WelcomeEmailPageFieldNameConstants.Subject, CultureInfo.CurrentUICulture);
    }
}
