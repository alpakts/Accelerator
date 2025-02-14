using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Websites;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class OrderConfirmationPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.OrderConfirmation)
                {
                    IndexThePage = false,
                    TemplatePath = "",
                    FieldGroups =  new FieldTemplateFieldGroup[]{
                         new FieldTemplateFieldGroup()
                            {
                                Id = "Contents",
                                Collapsed = false,
                                Fields =
                                {
                                    SystemFieldDefinitionConstants.Name,
                                    SystemFieldDefinitionConstants.Url,
                                    PageFieldNameConstants.Title,
                                    PageFieldNameConstants.Text,
                                    CheckoutPageFieldNameConstants.UseConfirmationWidget,
                                }
                            }
                     }
                },
            };
            return templates;
        }
    }
}
