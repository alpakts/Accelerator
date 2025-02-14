using System;

namespace Litium.Accelerator.Mailing.Models
{
    public class ForgotPasswordEmailModel
    {
        public DateTimeOffset Expires { get; set; }
        public string Link { get; set; }
    }
}
