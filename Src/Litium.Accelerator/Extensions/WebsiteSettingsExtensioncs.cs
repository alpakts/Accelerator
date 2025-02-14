using Litium.Accelerator.Constants;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Extensions
{
    /// <summary>
    /// Represents extension fow WebsiteModel
    /// </summary>
    public static class WebsiteSettingsExtension
    {
        /// <summary>
        /// Gets the navigation type.
        /// </summary>
        public static NavigationType GetNavigationType(this WebsiteModel websiteModel)
        {
            if (websiteModel.GetValue<string>(AcceleratorWebsiteFieldNameConstants.NavigationTheme) == NavigationConstants.FilterBased)
            {
                return NavigationType.Filter;
            }

            return NavigationType.Category;
        }  
        

        /// <summary>
        /// website setting: get field `InFirstLevelCategories` 
        /// </summary>
        /// <param name="websiteModel"></param>
        /// <returns></returns>
        public static bool InFirstLevelCategories(this WebsiteModel websiteModel)
        {
            return websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.InFirstLevelCategories);
        }

        /// <summary>
        /// website setting: get field `InArticlePages` 
        /// </summary>
        /// <param name="websiteModel"></param>
        /// <returns></returns>
        public static bool InArticlePages(this WebsiteModel websiteModel)
        {
            return websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.InArticlePages);
        }

        /// <summary>
        /// website setting: get field `InBrandPages` 
        /// </summary>
        /// <param name="websiteModel"></param>
        /// <returns></returns>
        public static bool InBrandPages(this WebsiteModel websiteModel)
        {
            return websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.InBrandPages);
        }

        /// <summary>
        /// website setting: get field `InProductListPages` 
        /// </summary>
        /// <param name="websiteModel"></param>
        /// <returns></returns>
        public static bool InProductListPages(this WebsiteModel websiteModel)
        {
            return websiteModel.GetValue<bool>(AcceleratorWebsiteFieldNameConstants.InProductListPages);
        }

    }
}
