using System;

namespace Litium.Accelerator.Services
{
    /// <summary>
    /// The exception supports to display more information on Error Page.
    /// </summary>
    public class ErrorPageMoreInfoException : Exception
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="ErrorPageMoreInfoException"/> class.
		/// </summary>
        public ErrorPageMoreInfoException()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="ErrorPageMoreInfoException"/> class.
		/// </summary>
        /// <param name="message">The exception message.</param>
        public ErrorPageMoreInfoException(string message) : base(message)
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="ErrorPageMoreInfoException"/> class.
		/// </summary>
        /// <param name="message">The exception message.</param>
		/// <param name="innerException">The inner exception.</param>
        public ErrorPageMoreInfoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
