using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Websites;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class CheckoutPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.Checkout)
                {
                    IndexThePage = false,
                    TemplatePath = "",
                    FieldGroups = new FieldTemplateFieldGroup[]
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "TermsAndConditions",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                SystemFieldDefinitionConstants.Url,
                                CheckoutPageFieldNameConstants.TermsAndConditionsPage,
                            }
                        },
                    }
                },
            };
            return templates;
        }
    }
}
