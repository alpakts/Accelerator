using System;
using System.Linq.Expressions;
using Litium.Accelerator.ViewModels.Product;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Litium.Web;

namespace Litium.Accelerator.Mvc.Extensions
{
    public static class ProductItemViewModelHtmlExtensions
    {
        public static IHtmlContent BuyButton<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, ProductItemViewModel>> expression, string cssClass = null, bool? isBuyButton = null)
        {
            var expressionMetadataProvider = htmlHelper.ViewContext.HttpContext.RequestServices.GetService<ModelExpressionProvider>();
            var metadata = expressionMetadataProvider.CreateModelExpression(htmlHelper.ViewData, expression);
            return htmlHelper.BuyButton((ProductItemViewModel)metadata.Model, cssClass, isBuyButton);
        }

        public static IHtmlContent BuyButton(this IHtmlHelper<ProductItemViewModel> htmlHelper, string cssClass = null, bool? isBuyButton = null)
        {
            return htmlHelper.BuyButton(htmlHelper.ViewData.Model, cssClass, isBuyButton);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static IHtmlContent BuyButton(this IHtmlHelper htmlHelper, ProductItemViewModel model, string cssClass, bool? isBuyButton)
        {
            var canBuy = (isBuyButton.HasValue && isBuyButton.Value || model.UseVariantUrl) && model.IsInStock;
            var buyButtonTag = new TagBuilder("buy-button");
            var label = canBuy || (isBuyButton.HasValue && isBuyButton.Value) ? "product.buy".AsWebsiteText() : "product.show".AsWebsiteText();

            cssClass = $"button {(canBuy || (isBuyButton.HasValue && isBuyButton.Value) ? "buy-button" : "show-button")} {cssClass ?? string.Empty}".Trim();
            //Shows BuyButton on the product detail page
            if (model.ShowBuyButton || (isBuyButton.HasValue && isBuyButton.Value))
            {
                if (canBuy)
                {
                    var enabled = !string.IsNullOrEmpty(model.Url) && model.Price.HasPrice && model.IsInStock;
                    if (enabled)
                    {
                        var quantityFieldId = model.ShowQuantityField ? $"{model.QuantityFieldId}" : string.Empty;
                        buyButtonTag.Attributes.Add("data-article-number", model.Id);
                        buyButtonTag.Attributes.Add("data-quantity-field-id", quantityFieldId);
                    }
                    else
                    {
                        cssClass += " disabled";
                    }
                }
                else
                {
                    if (isBuyButton.HasValue && isBuyButton.Value)
                    {
                        cssClass += " disabled";
                    }
                    else
                    {
                        buyButtonTag.Attributes.Add("data-href", model.Url);
                    }
                }
            }
            else if (string.IsNullOrEmpty(model.Url))
            {
                cssClass += " disabled";
            }

            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                buyButtonTag.Attributes.Add("data-css-class", cssClass);
            }
            buyButtonTag.InnerHtml.AppendHtml($"<span><a class='{cssClass}' href='#'>{label}</a></span>");
            buyButtonTag.Attributes.Add("data-label", label);

            return buyButtonTag;
        }
    }
}
