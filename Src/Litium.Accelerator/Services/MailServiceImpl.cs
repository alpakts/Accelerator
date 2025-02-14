using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Litium.Accelerator.Configuration;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Mailing;
using Litium.Globalization;
using Litium.Runtime.DependencyInjection;
using Litium.Scheduler;
using Litium.Web;
using Litium.Websites;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Litium.Accelerator.Services
{
    internal class MailServiceImpl : MailService
    {
        private readonly DomainNameService _domainNameService;
        private readonly UrlService _urlService;
        private readonly WebsiteService _websiteService;
        private readonly ChannelService _channelService;
        private readonly ILogger<MailService> _logger;
        private readonly SchedulerService _schedulerService;

        public MailServiceImpl(SchedulerService schedulerService,
            UrlService urlService,
            WebsiteService websiteService,
            ChannelService channelService,
            ILogger<MailService> logger,
            DomainNameService domainNameService)
        {
            _schedulerService = schedulerService;
            _urlService = urlService;
            _websiteService = websiteService;
            _channelService = channelService;
            _logger = logger;
            _domainNameService = domainNameService;
        }

        public override void SendEmail(MailDefinition mail, bool throwException)
        {
            var channel = _channelService.Get(mail.ChannelSystemId);
            var mailSender = _websiteService.Get(channel.WebsiteSystemId.Value)?.Fields.GetValue<string>(AcceleratorWebsiteFieldNameConstants.SenderEmailAddress);

            switch (mail)
            {
                case PlainMailDefinition plainTextMail:
                    var messageInfoPlainText = new MessageInfo
                    {
                        FromEmail = mailSender,
                        ToEmail = plainTextMail.ToEmail,
                        Subject = plainTextMail.Subject,
                        Body = plainTextMail.Body,
                        IsHtml = false,
                    };
                    _schedulerService.ScheduleJob<MailServiceProcessor>(x => x.Process(messageInfoPlainText));
                    return;
                case HtmlMailDefinition htmlTextMail:
                    var messageInfoHtmlText = new MessageInfo
                    {
                        FromEmail = mailSender,
                        ToEmail = htmlTextMail.ToEmail,
                        Subject = htmlTextMail.Subject,
                        Body = htmlTextMail.Body,
                        IsHtml = true,
                    };
                    _schedulerService.ScheduleJob<MailServiceProcessor>(x => x.Process(messageInfoHtmlText));
                    return;
            }

            if (!(mail is PageMailDefinition pageMail))
            {
                return;
            }

            var page = pageMail.Page;
            var pageUrl = _urlService.GetUrl(page, new PageUrlArgs(pageMail.ChannelSystemId) { AbsoluteUrl = true });
            var url = pageMail.UrlTransform(pageUrl);

            var domainName = _domainNameService.Get(channel.DomainNameLinks[0].DomainNameSystemId);
            var baseUrl = _urlService.GetUrl(domainName);
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                return;
            }

            if (string.IsNullOrEmpty(mailSender))
            {
                _logger.LogError("Sending e-mail failed. Mail sender was empty. Check mail configuration settings");
                if (throwException)
                {
                    throw new Exception("Mail sender was empty. Check mail configuration settings");
                }

                return;
            }

            var messageInfo = new MessageInfo
            {
                FromEmail = mailSender,
                ToEmail = pageMail.ToEmail,
                Subject = pageMail.Subject,
                Url = url,
                PageSystemId = page.SystemId,
                BaseUrl = baseUrl
            };
            _schedulerService.ScheduleJob<MailServiceProcessor>(x => x.Process(messageInfo));
        }

        internal class MessageInfo
        {
            public string BaseUrl { get; set; }
            public string Body { get; set; }
            public bool IsHtml { get; set; }
            public Guid PageSystemId { get; set; }
            public string Subject { get; set; }
            public string ToEmail { get; set; }
            public string Url { get; set; }
            public string FromEmail { get; set; }
        }

        [Service(ServiceType = typeof(MailServiceProcessor), Lifetime = DependencyLifetime.Singleton)]
        internal class MailServiceProcessor
        {
            private readonly ILogger<MailServiceProcessor> _logger;
            private readonly IOptions<SmtpConfig> _smtpConfig;
            private readonly IHttpClientFactory _httpClientFactory;

            public MailServiceProcessor(
                ILogger<MailServiceProcessor> logger,
                IOptions<SmtpConfig> smtpConfig,
                IHttpClientFactory httpClientFactory)
            {
                _logger = logger;
                _smtpConfig = smtpConfig;
                _httpClientFactory = httpClientFactory;
            }

            public async Task Process(MessageInfo message)
            {
                var data = message;
                if (data.Url == null)
                {
                    await SendEmail(data.FromEmail, data.ToEmail, data.Subject, data.Body, data.IsHtml, false);
                }
                else
                {
                    await SendEmail(data.FromEmail, data.ToEmail, data.Subject, data.Url, data.BaseUrl, false);
                }
            }

            public virtual async Task SendEmail(string fromEmail, string toEmail, string subject, string body, bool htmlFormat, bool throwException)
            {
                try
                {
                    await SendMail(fromEmail, toEmail, subject, body, htmlFormat);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Message}", ex.Message);
                    if (throwException)
                    {
                        throw;
                    }
                }
            }

            public virtual async Task SendEmail(string fromEmail, string toEmail, string subject, string url, string baseUrl, bool throwException)
            {
                // Get html content of page as email body
                var emailBody = await GetWebPageContent(url);

                if (string.IsNullOrEmpty(emailBody))
                {
                    _logger.LogError("Could not find email template at {Url}", url);
                    if (throwException)
                    {
                        throw new Exception("Could not find email template at " + url);
                    }

                    return;
                }

                // Replace relative paths with absolute paths in email body
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                emailBody = emailBody.Replace("href=\"/", "href=\"" + baseUrl);
                emailBody = emailBody.Replace("src=\"/", "src=\"" + baseUrl);

                await SendEmail(fromEmail, toEmail, subject, emailBody, true, throwException);
            }

            /// <summary>
            /// Sends an email
            /// </summary>
            /// <param name="from">From address.</param>
            /// <param name="to">To address</param>
            /// <param name="subject">Subject</param>
            /// <param name="body">Body</param>
            /// <param name="isBodyHtml">Specifies wether the mail contains HTML or not</param>
            /// <exception cref="SmtpException">Thrown when there is an SMTP error.</exception>
            public async Task SendMail(string from, string to, string subject, string body, bool isBodyHtml)
            {
                await SendMail(from, to, string.Empty, string.Empty, subject, body, isBodyHtml, SmtpDeliveryMethod.PickupDirectoryFromIis);
            }

            /// <summary>
            /// Sends the mail.
            /// </summary>
            /// <param name="from">From.</param>
            /// <param name="to">To.</param>
            /// <param name="cc">The cc.</param>
            /// <param name="bcc">The BCC.</param>
            /// <param name="subject">The subject.</param>
            /// <param name="body">The body.</param>
            /// <param name="isBodyHtml">Specifies wether the mail contains HTML or not</param>
            /// <exception cref="SmtpException">Thrown when there is an SMTP error.</exception>
            public async Task SendMail(string from, string to, string cc, string bcc, string subject, string body, bool isBodyHtml)
            {
                await SendMail(from, to, cc, bcc, subject, body, isBodyHtml, SmtpDeliveryMethod.PickupDirectoryFromIis);
            }

            /// <summary>
            /// Sends an email
            /// </summary>
            /// <param name="from">From address.</param>
            /// <param name="to">To address</param>
            /// <param name="subject">Subject</param>
            /// <param name="body">Body</param>
            /// <param name="isBodyHtml">Specifies wether the mail contains HTML or not</param>
            /// <param name="deliveryMethod">The delivery method.</param>
            /// <exception cref="SmtpException">Thrown when there is an SMTP error.</exception>
            public async Task SendMail(string from, string to, string subject, string body, bool isBodyHtml, SmtpDeliveryMethod deliveryMethod)
            {
                await SendMail(from, to, string.Empty, string.Empty, subject, body, isBodyHtml, deliveryMethod);
            }

            /// <summary>
            /// Sends an email
            /// </summary>
            /// <param name="from">From address.</param>
            /// <param name="to">To address</param>
            /// <param name="cc">CC address</param>
            /// <param name="bcc">BCC address</param>
            /// <param name="subject">Subject</param>
            /// <param name="body">Body</param>
            /// <param name="isBodyHtml">Specifies wether the mail contains HTML or not</param>
            /// <param name="deliveryMethod">The delivery method.</param>
            /// <exception cref="SmtpException">Thrown when there is an SMTP error.</exception>
            public async Task SendMail(string from, string to, string cc, string bcc, string subject, string body, bool isBodyHtml, SmtpDeliveryMethod deliveryMethod)
            {
                try
                {
                    var mailClient = new SmtpClient();
                    var smtpConfig = _smtpConfig.Value;
                    //Set mail client host to local host if empty
                    if (!string.IsNullOrEmpty(smtpConfig.Host))
                    {
                        mailClient.Host = smtpConfig.Host;
                        if (smtpConfig.Port > 0)
                        {
                            mailClient.Port = smtpConfig.Port;
                        }
                        mailClient.EnableSsl = smtpConfig.EnableSecureCommunication;
                        mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    }
                    else
                    {
                        mailClient.Host = "127.0.0.1";
                        mailClient.DeliveryMethod = deliveryMethod;
                    }

                    if (string.IsNullOrEmpty(from))
                    {
                        throw new Exception("From address is required");
                    }

                    using (var message = new MailMessage(from, to, subject, body))
                    {
                        if (!string.IsNullOrEmpty(cc))
                        {
                            var ccAddress = new MailAddress(cc);
                            message.CC.Add(ccAddress);
                        }
                        if (!string.IsNullOrEmpty(bcc))
                        {
                            var bccAddress = new MailAddress(bcc);
                            message.Bcc.Add(bccAddress);
                        }
                        message.IsBodyHtml = isBodyHtml;

                        if (!string.IsNullOrEmpty(smtpConfig.Username) && !string.IsNullOrEmpty(smtpConfig.Password))
                        {
                            mailClient.Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password);
                        }
                        else
                        {
                            mailClient.UseDefaultCredentials = true;
                        }

                        await mailClient.SendMailAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Message}", ex.Message);
                }
            }

            public async Task<string> GetWebPageContent(string url)
            {
                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.GetAsync(url);
                using var myStream = await response.Content.ReadAsStreamAsync();

                using var readStream = new StreamReader(myStream, Encoding.UTF8);
                var result = await readStream.ReadToEndAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Can't fetch mail to send, status code {response.StatusCode}", new Exception(result));
                }

                return result;
            }
        }
    }
}
