using Litium.Websites;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.Definitions.Websites
{
    internal class AcceleratorWebsiteFieldDefinitionSetup : FieldDefinitionSetup
    {
        public override IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var fields = new[]
            {
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.LogotypeMain, SystemFieldTypeConstants.MediaPointerImage)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.LogotypeIcon, SystemFieldTypeConstants.MediaPointerImage)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.HeaderLayout, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = HeaderLayoutConstants.OneRow,
                                Name = new Dictionary<string, string> { { "en-US", "One row" }, { "sv-SE", "En rad" } }
                            },
                            new TextOption.Item
                            {
                                Value = HeaderLayoutConstants.TwoRows,
                                Name = new Dictionary<string, string> { { "en-US", "Two rows" }, { "sv-SE", "Två rader" } }
                            }
                        }
                    }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.CheckoutPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.MyPagesPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.AdditionalHeaderLinks, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = true }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.SearchResultPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.FooterHeader, SystemFieldTypeConstants.LimitedText)
                {
                    Editable = true,
                    MultiCulture = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.FooterLinkList, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = true }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.FooterText, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true,
                },
                 new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.Footer, SystemFieldTypeConstants.MultiField)
                {
                     Editable = true,
                    Option = new MultiFieldOption { IsArray = true, Fields = new List<string>(){ AcceleratorWebsiteFieldNameConstants.FooterHeader, AcceleratorWebsiteFieldNameConstants.FooterLinkList, AcceleratorWebsiteFieldNameConstants.FooterText } }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.NavigationTheme, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = NavigationConstants.CategoryBased,
                                Name = new Dictionary<string, string> { { "en-US", "Category based" }, { "sv-SE", "Baserat på kategori" } }
                            },
                            new TextOption.Item
                            {
                                Value = NavigationConstants.FilterBased,
                                Name = new Dictionary<string, string> { { "en-US", "Filter based" }, { "sv-SE", "Baserat på filter" } }
                            }
                        }
                    }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.InFirstLevelCategories, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.InBrandPages, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.InProductListPages, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.InArticlePages, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.ProductsPerPage, SystemFieldTypeConstants.Int)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.ShowBuyButton, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.ShowQuantityFieldProductList, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.ShowQuantityFieldProductPage, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.FiltersOrdering, FieldTypeConstants.Filters)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.FiltersIndexedBySearchEngines, FieldTypeConstants.Filters)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.CheckoutMode, SystemFieldTypeConstants.IntOption)
                {
                    Editable = true,
                    Option = new IntOption
                    {
                        MultiSelect = false,
                        Items = new List<IntOption.Item>
                        {
                            new IntOption.Item
                            {
                                Value = (int)CheckoutMode.PrivateCustomers,
                                Name = new Dictionary<string, string> { { "en-US", "Private customers / B2C only" }, { "sv-SE", "Private customers / B2C only" } }
                            },
                            new IntOption.Item
                            {
                                Value = (int)CheckoutMode.CompanyCustomers,
                                Name = new Dictionary<string, string> { { "en-US", "Company customers / B2B only" }, { "sv-SE", "Företag/endast B2B" } }
                            },
                            new IntOption.Item
                            {
                                Value = (int)CheckoutMode.Both,
                                Name = new Dictionary<string, string> { { "en-US", "Both" }, { "sv-SE", "Båda" } }
                            }
                        }
                    }
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.AllowCustomersEditLogin, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.SenderEmailAddress, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(AcceleratorWebsiteFieldNameConstants.OrderConfirmationPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(CheckoutPageFieldNameConstants.TermsAndConditionsPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(CheckoutPageFieldNameConstants.UseConfirmationWidget, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                    CanBeGridColumn = false,
                    CanBeGridFilter = false,
                    MultiCulture = false,
                },
            };
            foreach (var field in fields)
            {
                field.UseInStorefront = true;
                field.UseInSearchEngine = true;
            }
            return fields;
        }
    }
}
