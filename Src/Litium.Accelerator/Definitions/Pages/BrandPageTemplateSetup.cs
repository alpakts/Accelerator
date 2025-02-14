using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using Litium.Websites;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class BrandPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.Brand)
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
                                PageFieldNameConstants.Title,
                                PageFieldNameConstants.Image,
                                PageFieldNameConstants.Text,
                            }
                        }
                    },
                    Containers = new List<BlockContainerDefinition>
                    {
                        new BlockContainerDefinition()
                        {
                            Id = BlockContainerNameConstant.Header,
                            Name =
                            {
                                ["sv-SE"] = "Huvud",
                                ["en-US"] = "Header",
                            }
                        }
                    }
                },
            };
            return templates;
        }
    }
}
