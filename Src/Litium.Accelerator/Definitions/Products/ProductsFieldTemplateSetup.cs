using System.Collections.Generic;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.Definitions.Products
{
    internal class ProductsFieldTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var fieldTemplates = new FieldTemplate[]
            {
                new CategoryFieldTemplate(ProductTemplateNameConstants.Category)
                {
                    CategoryFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Description,
                                SystemFieldDefinitionConstants.Url,
                                SystemFieldDefinitionConstants.SeoTitle,
                                SystemFieldDefinitionConstants.SeoDescription,
                                ProductFieldNameConstants.AcceleratorFilterFields,
                                ProductFieldNameConstants.OrganizationsPointer
                            }
                        }
                    }
                },
                new ProductFieldTemplate(ProductTemplateNameConstants.ProductWithOneVariant)
                {
                    UseVariantUrl = true,
                    ProductFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                ProductFieldNameConstants.OrganizationsPointer,
                            },
                        }
                    },
                    VariantFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Description,
                                SystemFieldDefinitionConstants.Url,
                                SystemFieldDefinitionConstants.SeoTitle,
                                SystemFieldDefinitionConstants.SeoDescription,
                            },
                        },
                        new FieldTemplateFieldGroup
                        {
                            Id = "Product information",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Produktinformation" },
                                ["en-US"] = { Name = "Product information" }
                            },
                            Fields =
                            {
                                ProductFieldNameConstants.News,
                                ProductFieldNameConstants.Brand,
                                ProductFieldNameConstants.Color,
                                ProductFieldNameConstants.Size,
                                ProductFieldNameConstants.ProductSheet,
                            },
                            UseInStorefront = true,
                        },
                        new FieldTemplateFieldGroup
                        {
                            Id = "Product specification",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Produktspecifikation" },
                                ["en-US"] = { Name = "Product specification" }
                            },
                            Fields =
                            {
                                ProductFieldNameConstants.Specification,
                                ProductFieldNameConstants.Weight,
                            },
                            UseInStorefront = true,
                        }
                    }
                },
                new ProductFieldTemplate(ProductTemplateNameConstants.ProductWithVariants)
                {
                    UseVariantUrl = true,
                    ProductFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Description,
                                ProductFieldNameConstants.Brand,
                                ProductFieldNameConstants.Type,
                                ProductFieldNameConstants.OrganizationsPointer,
                            },
                        },
                        new FieldTemplateFieldGroup
                        {
                            Id = "Product information",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Produktinformation" },
                                ["en-US"] = { Name = "Product information" }
                            },
                            Fields =
                            {
                                ProductFieldNameConstants.News,
                                ProductFieldNameConstants.ProductSheet,
                            },
                            UseInStorefront = true,
                        }
                    },
                    VariantFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Description,
                                SystemFieldDefinitionConstants.Url,
                                SystemFieldDefinitionConstants.SeoTitle,
                                SystemFieldDefinitionConstants.SeoDescription,
                            },
                        },
                        new FieldTemplateFieldGroup
                        {
                            Id = "Product information",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Produktinformation" },
                                ["en-US"] = { Name = "Product information" }
                            },
                            Fields =
                            {
                                ProductFieldNameConstants.Color,
                                ProductFieldNameConstants.Size,
                            },
                            UseInStorefront = true,
                        }
                    }
                },
                new ProductFieldTemplate(ProductTemplateNameConstants.ProductWithVariantsList)
                {
                    UseVariantUrl = false,
                    ProductFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Description,
                                ProductFieldNameConstants.Brand,
                                ProductFieldNameConstants.Type,
                                SystemFieldDefinitionConstants.Url,
                                SystemFieldDefinitionConstants.SeoTitle,
                                SystemFieldDefinitionConstants.SeoDescription,
                                ProductFieldNameConstants.OrganizationsPointer,
                            },
                        },
                        new FieldTemplateFieldGroup
                        {
                            Id = "Product specification",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Produktspecifikation" },
                                ["en-US"] = { Name = "Product specification" }
                            },
                            Fields =
                            {
                                ProductFieldNameConstants.Specification,
                                ProductFieldNameConstants.ProductSheet,
                            },
                            UseInStorefront = true,
                        }
                    },
                    VariantFieldGroups = new[]
                    {
                        new FieldTemplateFieldGroup
                        {
                            Id = "General",
                            Collapsed = false,
                            Localizations =
                            {
                                ["sv-SE"] = { Name = "Allmänt" },
                                ["en-US"] = { Name = "General" }
                            },
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                ProductFieldNameConstants.Color,
                                ProductFieldNameConstants.Size,
                            },
                        }
                    }
                }
            };
            return fieldTemplates;
        }
    }
}
