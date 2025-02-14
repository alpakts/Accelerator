using System;
using System.Globalization;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Mailing.Models;
using Litium.Customers;
using Litium.Websites;

namespace Litium.Accelerator.Mailing
{
    public class ForgotPasswordEmailDefinition : HtmlMailDefinition<ForgotPasswordEmailModel>
    {
        private readonly Page _page;
        private readonly Person _person;
        public ForgotPasswordEmailDefinition(Page page, Guid channelSystemId, ForgotPasswordEmailModel model, string toEmail, Person person)
            : base(channelSystemId, model, toEmail)
        {
            _page = page;
            _person = person;
        }

        protected override string RawBodyText => _page.Fields.GetValue<string>(GetBodyText(_person), CultureInfo.CurrentUICulture);
        protected override string RawSubjectText => _page.Fields.GetValue<string>(GetSubjectText(_person), CultureInfo.CurrentUICulture);

        private string GetBodyText(Person person)
        {
            if (person.LoginCredential.LockoutEndDate is not null && DateTimeOffset.Compare(person.LoginCredential.LockoutEndDate.Value, DateTimeOffset.Now) > 0)
            {
                return LoginPageFieldNameConstants.ForgottenPasswordLockedBody;
            }
            else
            {
                return LoginPageFieldNameConstants.ForgottenPasswordBody;
            }
        }

        private string GetSubjectText(Person person)
        {
            if (person.LoginCredential.LockoutEndDate is not null && DateTimeOffset.Compare(person.LoginCredential.LockoutEndDate.Value, DateTimeOffset.Now) > 0)
            {
                return LoginPageFieldNameConstants.ForgottenPasswordLockedSubject;
            }
            else
            {
                return LoginPageFieldNameConstants.ForgottenPasswordSubject;
            }
        }
    }
}
