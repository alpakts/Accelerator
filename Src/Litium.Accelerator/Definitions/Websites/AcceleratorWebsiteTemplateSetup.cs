using Litium.Websites;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.Definitions.Websites
{
    internal class AcceleratorWebsiteTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var template = new WebsiteFieldTemplate("AcceleratorWebsite")
            {
                FieldGroups = new[]
                {
                    new FieldTemplateFieldGroup()
                    {
                        Id = "General",
                        Collapsed = false,
                        Fields =
                        {
                            SystemFieldDefinitionConstants.Name
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Logotype",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.LogotypeMain,
                            AcceleratorWebsiteFieldNameConstants.LogotypeIcon
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Header",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.HeaderLayout,
                            AcceleratorWebsiteFieldNameConstants.CheckoutPage,
                            AcceleratorWebsiteFieldNameConstants.MyPagesPage,
                            AcceleratorWebsiteFieldNameConstants.AdditionalHeaderLinks
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Search",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.SearchResultPage
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Footer",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.Footer
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Navigation",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.NavigationTheme
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "LeftNavigation",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.InFirstLevelCategories,
                            AcceleratorWebsiteFieldNameConstants.InBrandPages,
                            AcceleratorWebsiteFieldNameConstants.InProductListPages,
                            AcceleratorWebsiteFieldNameConstants.InArticlePages
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "ProductLists",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.ProductsPerPage,
                            AcceleratorWebsiteFieldNameConstants.ShowBuyButton,
                            AcceleratorWebsiteFieldNameConstants.ShowQuantityFieldProductList
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "ProductPage",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.ShowQuantityFieldProductPage
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Filters",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.FiltersOrdering,
                            AcceleratorWebsiteFieldNameConstants.FiltersIndexedBySearchEngines
                        }
                    },new FieldTemplateFieldGroup()
                    {
                        Id = "Checkout",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.CheckoutMode
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Customers",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.AllowCustomersEditLogin
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Emails",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.SenderEmailAddress
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "OrderConfirmationPage",
                        Collapsed = false,
                        Fields =
                        {
                            AcceleratorWebsiteFieldNameConstants.OrderConfirmationPage
                        }
                    }
                }
            };
            return new List<FieldTemplate>() { template };
        }
    }
}
