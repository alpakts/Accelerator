using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Websites;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class OrderPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.Order)
                {
                    IndexThePage = false,
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
                                SystemFieldDefinitionConstants.Url
                            }
                        }
                    }
                },
            };
            return templates;
        }
    }
}

