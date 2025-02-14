using Litium.Accelerator.Mailing;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Services
{
    [Service(ServiceType = typeof(MailService), Lifetime = DependencyLifetime.Singleton)]
    public abstract class MailService
    {
        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="mail">The mail.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        public abstract void SendEmail(MailDefinition mail, bool throwException);
    }
}
