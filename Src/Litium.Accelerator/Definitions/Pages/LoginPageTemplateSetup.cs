using Litium.Websites;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class LoginPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new FieldTemplate[]
            {
                new PageFieldTemplate(PageTemplateNameConstants.Login)
                {
                    IndexThePage = false,
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
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "Login",
                            Collapsed = false,
                            Fields =
                            {
                               PageFieldNameConstants.Title,
                               LoginPageFieldNameConstants.RedirectLink,
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "ForgotPassword",
                            Collapsed = false,
                            Fields =
                            {
                                 LoginPageFieldNameConstants.ForgottenPasswordLink,
                            }
                        }
                    }
                },
            };
            return templates;
        }
    }
}
