using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Customers;

namespace Litium.Accelerator.Definitions.Customers
{
    internal class CustomersRoleSetup : RoleSetup
    {
        public override IEnumerable<Role> GetRoles()
        {
            var items = new List<Role>
            {
                new Role()
                {
                    Id = RolesConstants.RoleOrderApprover,
                },
                new Role()
                {
                    Id = RolesConstants.RoleOrderPlacer,
                }
            };
            return items;
        }
    }
}
