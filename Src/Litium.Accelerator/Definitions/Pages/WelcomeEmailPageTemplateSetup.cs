using Litium.Websites;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class WelcomeEmailPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.WelcomeEmail)
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
                                SystemFieldDefinitionConstants.Url,
                                WelcomeEmailPageFieldNameConstants.Subject,
                                WelcomeEmailPageFieldNameConstants.WelcomeEmailText
                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
}
