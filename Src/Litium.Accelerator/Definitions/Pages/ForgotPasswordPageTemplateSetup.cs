using Litium.Websites;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class ForgotPasswordPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new FieldTemplate[]
            {
                new PageFieldTemplate(PageTemplateNameConstants.ForgotPassword)
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
                            Id = "ForgotPassword",
                            Collapsed = false,
                            Fields =
                            {
                                 PageFieldNameConstants.Title,
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "Email",
                            Collapsed = false,
                            Fields =
                            {
                                LoginPageFieldNameConstants.ForgottenPasswordSubject,
                                LoginPageFieldNameConstants.ForgottenPasswordBody
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "EmailLocked",
                            Collapsed = false,
                            Fields =
                            {
                                LoginPageFieldNameConstants.ForgottenPasswordLockedSubject,
                                LoginPageFieldNameConstants.ForgottenPasswordLockedBody
                            }
                        }
                         
                    }
                },
            };
            return templates;
        }
    }
}
