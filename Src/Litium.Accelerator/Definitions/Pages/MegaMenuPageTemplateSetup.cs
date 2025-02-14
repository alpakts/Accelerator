using Litium.Websites;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class MegaMenuPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var template = new PageFieldTemplate(PageTemplateNameConstants.MegaMenu)
            {
                IndexThePage = false,
                FieldGroups = new List<FieldTemplateFieldGroup>
                {
                    new FieldTemplateFieldGroup()
                    {
                        Id = "General",
                        Collapsed = false,
                        Fields =
                        {
                            SystemFieldDefinitionConstants.Name,
                            SystemFieldDefinitionConstants.Url
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "Link",
                        Collapsed = false,
                        Fields =
                        {
                            MegaMenuPageFieldNameConstants.MegaMenuCategory,
                            MegaMenuPageFieldNameConstants.MegaMenuPage
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "MegaMenu",
                        Collapsed = false,
                        Fields =
                        {
                            MegaMenuPageFieldNameConstants.MegaMenuShowContent,
                            MegaMenuPageFieldNameConstants.MegaMenuShowSubCategories
                        }
                    },
                    new FieldTemplateFieldGroup()
                    {
                        Id = "MegaMenuColumn",
                        Collapsed = false,
                        Fields =
                        {
                            MegaMenuPageFieldNameConstants.MegaMenuColumn
                        }
                    }
                }
            };

            return new List<FieldTemplate>() { template };
        }
    }
}
