using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Web.Models.Products;

namespace Litium.Accelerator
{
    /// <summary>
    /// ProductModel extensions
    /// </summary>
    public static class ProductsExtensions
    {
        private static readonly LazyService<CategoryService> _categoryService = new LazyService<CategoryService>();
        private static readonly LazyService<UrlService> _urlService = new LazyService<UrlService>();
        private static readonly LazyService<VariantService> _variantService = new LazyService<VariantService>();

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="showEmptyCategories">If set to false, only the categories with products in the channel given in <paramref name="channelSystemId" />.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <param name="channelSystemId">The channel system Id.</param>
        /// <returns></returns>
        public static IEnumerable<Category> GetChildren(this Category arg, bool showEmptyCategories = false, bool recursive = false, Guid? channelSystemId = null)
        {
            var categoryService = _categoryService.Value;
            var result = recursive ? GetChildrenRecursive(arg, categoryService) : categoryService.GetChildCategories(arg.SystemId);

            return showEmptyCategories
                ? result
                : result.Where(x => !x.IsEmpty(channelSystemId));
        }

        /// <summary>
        /// Gets the main category under which this variant is published.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The channel identifier.</param>
        /// <returns></returns>
        public static Category GetMainCategory(this BaseProduct arg, Guid? channelSystemId)
        {
            var linkedCategories = _categoryService.Value.GetByBaseProduct(arg.SystemId);
            return linkedCategories.FirstOrDefault(x => x.IsPublished(channelSystemId) && x.ProductLinks.Any(l => l.BaseProductSystemId == arg.SystemId && l.MainCategory));
        }

        /// <summary>
        /// Gets the main category under which this variant is published.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The web site identifier.</param>
        /// <returns></returns>
        public static Category GetMainCategory(this Variant arg, Guid channelSystemId)
        {
            var baseProduct = arg.BaseProductSystemId.MapTo<BaseProduct>();
            return baseProduct.GetMainCategory(channelSystemId);
        }

        /// <summary>
        /// Gets the parents.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static IEnumerable<Category> GetParents(this Category arg)
        {
            var categoryService = _categoryService.Value;
            var result = new List<Category>();
            var current = categoryService.Get(arg.ParentCategorySystemId);
            while (current != null)
            {
                result.Add(current);
                current = categoryService.Get(current.ParentCategorySystemId);
            }
            return result;
        }

        /// <summary>
        /// Gets the variants that are published in the <paramref name="websiteSystemId"/>.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="websiteSystemId">The website system identifier.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <returns></returns>
        public static IEnumerable<Variant> GetPublishedVariants(this BaseProduct arg, Guid websiteSystemId, Guid? channelSystemId = null)
        {
            return _variantService.Value
                .GetByBaseProduct(arg.SystemId)
                .Where(x => x.IsPublished(channelSystemId));
        }

        /// <summary>
        /// Gets the products name.
        /// If <see cref="ProductModel.UseVariantUrl" /> is true, priority is given to the field value from Variant.
        /// </summary>
        /// <param name="productModel">The product model.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public static string GetName(this ProductModel productModel, CultureInfo cultureInfo)
        {
            return productModel.GetValue<string>(SystemFieldDefinitionConstants.Name, cultureInfo);
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="productModel">The product model.</param>
        /// <param name="webSiteSystemId">The web site system identifier.</param>
        /// <param name="currentCategory">The current category.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <returns>System.String.</returns>
        public static string GetUrl(this ProductModel productModel, Guid webSiteSystemId, Category currentCategory = null, Guid? channelSystemId = null)
        {
            return productModel.UseVariantUrl ? productModel.SelectedVariant.GetUrl(currentCategory: currentCategory, channelSystemId: channelSystemId) : productModel.BaseProduct.GetUrl(webSiteSystemId, currentCategory: currentCategory, channelSystemId: channelSystemId);
        }


        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="websiteSystemId">The website system identifier.</param>
        /// <param name="absoluteUrl">if set to <c>true</c> [absolute URL].</param>
        /// <param name="currentCategory">The current category.</param>
        /// <param name="channelSystemId">The channel identifier.</param>
        /// <returns>System.String.</returns>
        public static string GetUrl(this BaseProduct arg, Guid websiteSystemId, bool absoluteUrl = false, Category currentCategory = null, Guid? channelSystemId = null)
        {
            if (channelSystemId == null)
            {
                return _urlService.Value.GetUrl(arg, opt =>
                {
                    opt.AbsoluteUrl = absoluteUrl;
                    opt.CurrentCategory = currentCategory;
                });
            }
            return _urlService.Value.GetUrl(arg, new ProductUrlArgs(channelSystemId.Value) { AbsoluteUrl = absoluteUrl, CurrentCategory = currentCategory });
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <param name="absoluteUrl">if set to <c>true</c> [absolute URL].</param>
        /// <returns>System.String.</returns>
        public static string GetUrl(this Category arg, Guid? channelSystemId = null, bool absoluteUrl = false)
        {
            if (channelSystemId == null)
            {
                return _urlService.Value.GetUrl(arg, opt =>
                {
                    opt.AbsoluteUrl = absoluteUrl;
                });
            }
            return _urlService.Value.GetUrl(arg, new CategoryUrlArgs(channelSystemId.Value) { AbsoluteUrl = absoluteUrl });
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <param name="absoluteUrl">if set to <c>true</c> [absolute URL].</param>
        /// <param name="currentCategory">The current category.</param>
        /// <returns></returns>
        public static string GetUrl(this Variant arg, bool absoluteUrl = false, Category currentCategory = null, Guid? channelSystemId = null)
        {
            if (channelSystemId == null)
            {
                return _urlService.Value.GetUrl(arg, opt =>
                {
                    opt.AbsoluteUrl = absoluteUrl;
                    opt.CurrentCategory = currentCategory;
                });
            }
            return _urlService.Value.GetUrl(arg, new ProductUrlArgs(channelSystemId.Value) { AbsoluteUrl = absoluteUrl, CurrentCategory = currentCategory });
        }


        /// <summary>
        /// Determines whether the category has any published products in the given website.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <returns></returns>
        public static bool IsEmpty(this Category arg, Guid? channelSystemId = null)
        {
            if (!arg.ProductLinks.Any())
            {
                return true;
            }

            var variantService = _variantService.Value;
            return !arg.ProductLinks.Any(y => y.ActiveVariantSystemIds.Any(v => variantService.Get(v).IsPublished(channelSystemId)));
        }

        /// <summary>
        /// Determines whether the specified argument is published.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <returns></returns>
        public static bool IsPublished(this Category arg, Guid? channelSystemId = null)
        {
            return !string.IsNullOrEmpty(arg?.GetUrl(channelSystemId));
        }

        /// <summary>
        /// Determines whether this variant is published in the given website.
        /// To be published, atleast one category should exists, that enables this variant on it,
        /// and the variant should also contain website link.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="channelSystemId">The channel system identifier.</param>
        /// <returns></returns>
        public static bool IsPublished(this Variant arg, Guid? channelSystemId = null)
        {
            return !string.IsNullOrEmpty(arg.GetUrl(channelSystemId: channelSystemId))
                   && (!channelSystemId.HasValue || arg.IsActive(channelSystemId.Value));
        }

        private static IEnumerable<Category> GetChildrenRecursive(Category arg, CategoryService service)
        {
            var stack = new Stack<Category>();
            stack.Push(arg);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in service.GetChildCategories(next.SystemId))
                    stack.Push(child);
            }
        }
    }
}
