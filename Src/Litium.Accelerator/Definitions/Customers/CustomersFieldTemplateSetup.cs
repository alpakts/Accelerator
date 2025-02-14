using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Customers;
using Litium.FieldFramework;

namespace Litium.Accelerator.Definitions.Customers
{
    internal class CustomersFieldTemplateSetup : FieldTemplateSetup
    {
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var items = new FieldTemplate[]
            {
                new PersonFieldTemplate(CustomerTemplateIdConstants.B2CPersonTemplate)
                {
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.FirstName,
                                SystemFieldDefinitionConstants.LastName,
                                SystemFieldDefinitionConstants.Email,
                                SystemFieldDefinitionConstants.Phone,
                                "SocialSecurityNumber"
                            }
                        }
                    }
                },new PersonFieldTemplate(CustomerTemplateIdConstants.B2BPersonTemplate)
               {
                   FieldGroups = new []
                   {
                       new FieldTemplateFieldGroup()
                       {
                           Id = "General",
                           Collapsed = false,
                           Fields =
                           {
                               SystemFieldDefinitionConstants.FirstName,
                               SystemFieldDefinitionConstants.LastName,
                               SystemFieldDefinitionConstants.Email,
                               SystemFieldDefinitionConstants.Phone
                           }
                       }
                   }
               },
                new GroupFieldTemplate(CustomerTemplateIdConstants.GroupTemplate)
                {
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.NameInvariantCulture,
                                SystemFieldDefinitionConstants.Description
                            }
                        }
                    }
                },
                new OrganizationFieldTemplate(CustomerTemplateIdConstants.OrganizationTemplate)
                {
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.NameInvariantCulture,
                                SystemFieldDefinitionConstants.Description,
                                "LegalRegistrationNumber"
                            }
                        }
                    }
                },
                new TargetGroupFieldTemplate(CustomerTemplateIdConstants.TargetGroupTemplate)
                {
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.NameInvariantCulture,
                                SystemFieldDefinitionConstants.Description
                            }
                        }
                    }
                },
            };
            return items;
        }
    }
}
