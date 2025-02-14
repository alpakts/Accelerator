using System.Collections.Generic;
using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(FieldTemplateSetup))]
    public abstract class FieldTemplateSetup
    {
        public abstract IEnumerable<FieldTemplate> GetTemplates();
    }
}
