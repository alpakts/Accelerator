using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Websites;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Pages
{
    internal class NewsListPageTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var templates = new List<FieldTemplate>
            {
                new PageFieldTemplate(PageTemplateNameConstants.NewsList)
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
                                SystemFieldDefinitionConstants.Url
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "Contents",
                            Collapsed = false,
                            Fields =
                            {
                               PageFieldNameConstants.Title,
                               PageFieldNameConstants.Introduction,
                               PageFieldNameConstants.Text,
                               PageFieldNameConstants.NumberOfNewsPerPage
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "LinkList",
                            Collapsed = false,
                            Fields =
                            {
                                PageFieldNameConstants.Links
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "FileList",
                            Collapsed = false,
                            Fields =
                            {
                                PageFieldNameConstants.Files
                            }
                        },
                    }
                },
            };
            return templates;
        }
    }
}
