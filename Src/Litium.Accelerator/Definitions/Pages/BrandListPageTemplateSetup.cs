using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Websites;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class BrandListPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.BrandList)
                {
                    IndexThePage = true,
                    TemplatePath = "",
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Url,
                                PageFieldNameConstants.TitleFilterSelector,
                            }
                        }
                    }
                },
            };
            return templates;
        }
    }
}
