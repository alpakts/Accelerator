using System.Collections.Generic;
using Litium.Products;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Definitions
{
    [Service(ServiceType = typeof(RelationshipTypeSetup))]
    public abstract class RelationshipTypeSetup
    {
        public abstract IEnumerable<RelationshipType> GetRelationshipTypes();
    }
}
