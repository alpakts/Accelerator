using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Websites;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class PageFieldDefinitionSetup : FieldDefinitionSetup
    {
        public override IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var fields = new List<FieldDefinition>();

            fields.AddRange(GeneralFields());
            fields.AddRange(LoginPageFields());
            fields.AddRange(MegaMenuPageFields());
            fields.ForEach(field => {
                field.UseInStorefront = true;
                field.UseInSearchEngine = true;
            });
            return fields;
        }

        private IEnumerable<FieldDefinition> GeneralFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.Title, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                    MultiCulture = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.Introduction, SystemFieldTypeConstants.MultirowText)
                {
                    Editable = true,
                    MultiCulture = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.Text, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.Links, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = true }
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.Image, SystemFieldTypeConstants.MediaPointerImage)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.Files, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.MediaFile, MultiSelect = true }
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.PageSize, SystemFieldTypeConstants.Int)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.TitleFilterSelector, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.ErrorMessage, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.NumberOfNewsPerPage, SystemFieldTypeConstants.Int)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.NewsDate, SystemFieldTypeConstants.DateTime)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.CategoryPointer, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory, MultiSelect = false }
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.AlternativeImageDescription, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(WelcomeEmailPageFieldNameConstants.Subject, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(WelcomeEmailPageFieldNameConstants.WelcomeEmailText, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.NumberOfOrdersPerPage, SystemFieldTypeConstants.Int)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.OrderLink, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = false }
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.ProductListPointer, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsProductList, MultiSelect = false }
                },
                new FieldDefinition<WebsiteArea>(PageFieldNameConstants.MayUserEditLogin, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                }
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> LoginPageFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<WebsiteArea>(LoginPageFieldNameConstants.ForgottenPasswordLink, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(LoginPageFieldNameConstants.RedirectLink, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = false }
                },
                new FieldDefinition<WebsiteArea>(LoginPageFieldNameConstants.ForgottenPasswordSubject, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(LoginPageFieldNameConstants.ForgottenPasswordBody, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true
                },
                 new FieldDefinition<WebsiteArea>(LoginPageFieldNameConstants.ForgottenPasswordLockedSubject, SystemFieldTypeConstants.Text)
                {
                     Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(LoginPageFieldNameConstants.ForgottenPasswordLockedBody, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true
                }
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> MegaMenuPageFields()
        {
            var fields = new List<FieldDefinition<WebsiteArea>>
            {
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuCategory, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory }
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuShowContent, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuShowSubCategories, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuColumnHeader, SystemFieldTypeConstants.LimitedText)
                {
                    Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuCategories, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory, MultiSelect = true }
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuPages, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = true }
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuFilters, FieldTypeConstants.Filters)
                {
                    Editable = true,
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuEditor, SystemFieldTypeConstants.Editor)
                {
                    Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuAdditionalLink, SystemFieldTypeConstants.LimitedText)
                {
                    Editable = true,
                    MultiCulture = true
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuLinkToCategory, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory }
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuLinkToPage, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage }
                },
                new FieldDefinition<WebsiteArea>(MegaMenuPageFieldNameConstants.MegaMenuColumn, SystemFieldTypeConstants.MultiField)
                {
                     Option = new MultiFieldOption { IsArray = true, Fields = new List<string>(){ MegaMenuPageFieldNameConstants.MegaMenuColumnHeader,
                        MegaMenuPageFieldNameConstants.MegaMenuCategories, MegaMenuPageFieldNameConstants.MegaMenuPages, MegaMenuPageFieldNameConstants.MegaMenuFilters, MegaMenuPageFieldNameConstants.MegaMenuEditor,
                        MegaMenuPageFieldNameConstants.MegaMenuAdditionalLink, MegaMenuPageFieldNameConstants.MegaMenuLinkToCategory, MegaMenuPageFieldNameConstants.MegaMenuLinkToPage} },
                     Editable = true,
                },
            };

            return fields;
        }
    }
}
