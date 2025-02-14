using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Litium.Accelerator.ViewModels.Product;
using Litium.FieldFramework;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Builders.Product
{
    public class ProductFieldGroupViewModelBuilder : IViewModelBuilder<ProductFieldGroupViewModel>
    {
        private readonly ProductFieldViewModelBuilder _productFieldViewModelBuilder;

        public ProductFieldGroupViewModelBuilder(ProductFieldViewModelBuilder productFieldViewModelBuilder)
        {
            _productFieldViewModelBuilder = productFieldViewModelBuilder;
        }

        public virtual IEnumerable<ProductFieldGroupViewModel> Build([NotNull] ProductModel productModel)
        {
            var groupIds = productModel.FieldTemplate.ProductFieldGroups.Select(s => s.Id).ToHashSet();
            groupIds.UnionWith(productModel.FieldTemplate.VariantFieldGroups.Select(s => s.Id));

            return groupIds.Select(groupId => Build(productModel, groupId)).Where(x => x is not null);
        }

        public virtual ProductFieldGroupViewModel Build([NotNull] ProductModel productModel, [NotNull] string fieldGroup, bool includeBaseProductFields = true, bool includeVariantFields = true, bool includeHiddenFields = false, bool includeEmptyFields = false)
        {
            FieldTemplateFieldGroup productFieldGroup = default;
            if (includeBaseProductFields)
            {
                productFieldGroup = productModel.FieldTemplate.ProductFieldGroups?
                    .FirstOrDefault(x => fieldGroup.Equals(x.Id, StringComparison.OrdinalIgnoreCase) && x.UseInStorefront);
            }

            FieldTemplateFieldGroup variantFieldGroup = default;
            if (includeBaseProductFields)
            {
                variantFieldGroup = productModel.FieldTemplate.VariantFieldGroups?
                    .FirstOrDefault(x => fieldGroup.Equals(x.Id, StringComparison.OrdinalIgnoreCase) && x.UseInStorefront);
            }

            if(productFieldGroup is null && variantFieldGroup is null)
            {
                return null;
            }

            var fields = _productFieldViewModelBuilder.Build(productModel, fieldGroup, includeBaseProductFields, includeVariantFields, includeHiddenFields, includeEmptyFields);

            return new ProductFieldGroupViewModel
            {
                GroupId = productFieldGroup?.Id ?? variantFieldGroup?.Id,
                Name = productFieldGroup?.Localizations[CultureInfo.CurrentUICulture]?.Name ?? variantFieldGroup?.Localizations[CultureInfo.CurrentUICulture]?.Name,
                ProductFields = fields,
            };
        }
    }
}
