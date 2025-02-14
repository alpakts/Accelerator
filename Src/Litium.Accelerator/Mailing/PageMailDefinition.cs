using Litium.Websites;

namespace Litium.Accelerator.Mailing
{
    /// <summary>
    /// Mail from an publishing page
    /// </summary>
    public abstract class PageMailDefinition : MailDefinition
    {
        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        public abstract Page Page { get; }

        /// <summary>
        /// URLs the transform.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The transformed url.</returns>
        public virtual string UrlTransform(string url)
        {
            return url;
        }
    }
}
