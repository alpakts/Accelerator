using System.Collections.Generic;
using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(FieldDefinitionSetup))]
    public abstract class FieldDefinitionSetup
    {
        public abstract IEnumerable<FieldDefinition> GetFieldDefinitions();
    }
}
