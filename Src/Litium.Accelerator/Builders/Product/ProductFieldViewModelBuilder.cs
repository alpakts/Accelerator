using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Litium.Accelerator.Fields;
using Litium.Accelerator.ViewModels;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models;
using Litium.Web.Models.Products;
using Litium.Web.Routing;

namespace Litium.Accelerator.Builders.Product
{
    public class ProductFieldViewModelBuilder : IViewModelBuilder<ProductFieldViewModel>
    {
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly NamedServiceFactory<FieldFormatter> _fieldFormatterServiceFactory;
        private readonly RouteRequestLookupInfoAccessor _routeRequestLookupInfoAccessor;

        public ProductFieldViewModelBuilder(FieldDefinitionService fieldDefinitionService,
            NamedServiceFactory<FieldFormatter> fieldFormatterServiceFactory,
            RouteRequestLookupInfoAccessor routeRequestLookupInfoAccessor)
        {
            _fieldDefinitionService = fieldDefinitionService;
            _fieldFormatterServiceFactory = fieldFormatterServiceFactory;
            _routeRequestLookupInfoAccessor = routeRequestLookupInfoAccessor;
        }

        public IEnumerable<ProductFieldViewModel> Build([NotNull] ProductModel productModel, [NotNull] string fieldGroup, bool includeBaseProductFields = true, bool includeVariantFields = true, bool includeHiddenFields = false, bool includeEmptyFields = false)
            => Build(productModel, fieldGroup, CultureInfo.CurrentUICulture, includeBaseProductFields, includeVariantFields, includeHiddenFields, includeEmptyFields);

        public IEnumerable<ProductFieldViewModel> Build([NotNull] ProductModel productModel, [NotNull] string fieldGroup, [NotNull] CultureInfo cultureInfo, bool includeBaseProductFields = true, bool includeVariantFields = true, bool includeHiddenFields = false, bool includeEmptyFields = false)
        {
            var result = new List<ProductFieldViewModel>();
            if (includeBaseProductFields)
            {
                var baseProductGroup = productModel.FieldTemplate.ProductFieldGroups?.FirstOrDefault(x => fieldGroup.Equals(x.Id, StringComparison.OrdinalIgnoreCase) && x.UseInStorefront);
                if (baseProductGroup is not null)
                {
                    var baseProductFields = baseProductGroup.Fields ?? Enumerable.Empty<string>();
                    foreach (var field in baseProductFields)
                    {
                        ProductFieldViewModel model = null;
                        var fieldDefinition = _fieldDefinitionService.Get<ProductArea>(field);
                        if (fieldDefinition is null || !fieldDefinition.UseInStorefront || fieldDefinition.Hidden && !includeHiddenFields)
                        {
                            continue;
                        }

                        var culture = fieldDefinition.MultiCulture ? cultureInfo.Name : "*";
                        if (productModel.BaseProduct.Fields.TryGetValue(field, culture, out var value))
                        {
                            model = CreateModel(fieldDefinition, cultureInfo, value);
                        }
                        else if (includeEmptyFields)
                        {
                            model = CreateModel(fieldDefinition, cultureInfo, culture);
                        }

                        if(model is not null)
                        {
                            result.Add(model);
                        }
                    }
                }
            }

            if (includeVariantFields)
            {
                var variantGroup = productModel.FieldTemplate.VariantFieldGroups?.FirstOrDefault(x => fieldGroup.Equals(x.Id, StringComparison.OrdinalIgnoreCase) && x.UseInStorefront);
                if (variantGroup is not null)
                {
                    var variantFields = variantGroup.Fields ?? Enumerable.Empty<string>();

                    foreach (var field in variantFields)
                    {
                        ProductFieldViewModel model = null;
                        var fieldDefinition = _fieldDefinitionService.Get<ProductArea>(field);
                        if (fieldDefinition is null || !fieldDefinition.UseInStorefront || fieldDefinition.Hidden && !includeHiddenFields)
                        {
                            continue;
                        }

                        
                        var culture = fieldDefinition.MultiCulture ? cultureInfo.Name : "*";
                        if (productModel.SelectedVariant.Fields.TryGetValue(field, culture, out var value))
                        {
                            model = CreateModel(fieldDefinition, cultureInfo, value);
                        }
                        else if (includeEmptyFields)
                        {
                            model = CreateModel(fieldDefinition, cultureInfo, culture);
                        }

                        if (model is not null)
                        {
                            var existingIndex = result.FindIndex(s => s.Id == model.Id);
                            if (existingIndex != -1)
                            {
                                result[existingIndex] = model;
                            }
                            else
                            {
                                result.Add(model);
                            }
                        }
                    }
                }             
            }

            return result;
        }

        private ProductFieldViewModel CreateModel([NotNull] FieldDefinition fieldDefinition, CultureInfo cultureInfo, object value = null)
        {
            var fieldFormatter = _fieldFormatterServiceFactory.GetService(fieldDefinition.FieldType);

            if (fieldFormatter == null)
            {
                return null;
            }

            if (fieldDefinition.FieldType == SystemFieldTypeConstants.MediaPointerFile)
            {
                return CreateModel("FileField", fieldDefinition, cultureInfo, new MediaResourceFieldFormatArgs { Culture = cultureInfo }, fieldFormatter, value);
            }

            if (fieldDefinition.FieldType == SystemFieldTypeConstants.MediaPointerImage)
            {
                return CreateModel("ImageField", fieldDefinition, cultureInfo, new MediaResourceFieldFormatArgs { Culture = cultureInfo }, fieldFormatter, value);
            }

            if (fieldDefinition.FieldType == SystemFieldTypeConstants.Editor)
            {
                return CreateModel("Field", fieldDefinition, cultureInfo, new FieldFormatArgs { Culture = cultureInfo }, fieldFormatter, value.MapTo<EditorString>().Value);
            }

            if (fieldDefinition.FieldType == SystemFieldTypeConstants.Link)
            {
                return CreateModel("LinkField", fieldDefinition, cultureInfo, new LinkFieldFormatArgs
                {
                    Culture = cultureInfo,
                    ChannelSystemId = _routeRequestLookupInfoAccessor.RouteRequestLookupInfo?.Channel?.SystemId
                }, fieldFormatter, value);
            }

            return CreateModel("Field", fieldDefinition, cultureInfo, new FieldFormatArgs { Culture = cultureInfo }, fieldFormatter, value);
        }
        private ProductFieldViewModel CreateModel(string viewName, FieldDefinition fieldDefinition, CultureInfo cultureInfo, FieldFormatArgs fieldFormatArgs, FieldFormatter fieldFormatter, object value = null)
        {
            return new ProductFieldViewModel
            {
                Id = fieldDefinition.Id,
                ViewName = viewName,
                Name = fieldDefinition.Localizations[cultureInfo].Name,
                Value = fieldFormatter.Format(fieldDefinition, value, fieldFormatArgs),
                Args = fieldFormatArgs
            };
        }
    }
}
