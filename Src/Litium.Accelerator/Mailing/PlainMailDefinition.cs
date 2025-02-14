namespace Litium.Accelerator.Mailing
{
    public abstract class PlainMailDefinition : MailDefinition
    {
        /// <summary>
        /// Get the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public abstract string Body { get; }
    }
}
