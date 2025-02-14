using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using Litium.Websites;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class LandingPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.Landing)
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
                                PageFieldNameConstants.CategoryPointer
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
                        },
                        new BlockContainerDefinition()
                        {
                            Id = BlockContainerNameConstant.Main,
                            Name =
                            {
                                ["sv-SE"] = "Huvudsaklig",
                                ["en-US"] = "Main",
                            }
                        }
                    }
                },
            };
            return templates;
        }
    }
}
