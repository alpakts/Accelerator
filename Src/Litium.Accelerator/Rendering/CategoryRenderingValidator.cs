using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Utilities;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;
using Litium.Web.Rendering;

namespace Litium.Accelerator.Rendering
{
    internal class CategoryRenderingValidator : IRenderingValidator<Category>
    {
        private readonly PersonStorage _personStorage;

        public CategoryRenderingValidator(PersonStorage personStorage)
        {
            _personStorage = personStorage;
        }

        public bool Validate(Category model)
        {
            var organizations = model.Fields.GetValue<IList<PointerItem>>(ProductFieldNameConstants.OrganizationsPointer)?.Select(x => x.EntitySystemId);
            if (organizations is null
                || (_personStorage.CurrentSelectedOrganizationSystemId is Guid organizationSystemId
                && organizations.Contains(organizationSystemId)))
            {
                return true;
            }

            return false;
        }
    }
}
