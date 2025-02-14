using System.Collections.Generic;
using Litium.Customers;
using Litium.FieldFramework;

namespace Litium.Accelerator.Definitions.Customers
{
    internal class CustomersFieldDefinitionSetup : FieldDefinitionSetup
    {
        public override IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<CustomerArea>("SocialSecurityNumber", SystemFieldTypeConstants.Text)
                {
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                    Editable = true,
                },
                new FieldDefinition<CustomerArea>("LegalRegistrationNumber", SystemFieldTypeConstants.Text)
                {
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                    Editable = true,
                }
            };
            return fields;
        }
    }
}
