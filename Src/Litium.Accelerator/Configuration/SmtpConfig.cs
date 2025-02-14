using Litium.ComponentModel;

namespace Litium.Accelerator.Configuration
{
    /// <summary>
    /// SmtpServer configuration.
    /// </summary>
    public class SmtpConfig
    {
        /// <summary>
        /// Gets or sets the hostname or ip-address of the smtp service.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port for the smtp service.
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// Gets or sets the password for the smtp service.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the username for the smtp service.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets if TLS communication should be used for smtp service communication.
        /// </summary>
        public bool EnableSecureCommunication { get; set; }

    }
}
