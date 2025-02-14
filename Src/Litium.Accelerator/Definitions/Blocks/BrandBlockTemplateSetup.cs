using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class BrandBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;
        public BrandBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var productCategoryId = _categoryService.Get(BlockCategoryNameConstants.Products)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.Brand)
                {
                    CategorySystemId = productCategoryId,
                    Icon = "fas fa-registered",
                    FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                            }
                        },
                        new FieldTemplateFieldGroup()
                        {
                            Id = "Brands",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.BlockTitle,
                                BlockFieldNameConstants.BrandsLinkList,
                                BlockFieldNameConstants.LinkText,
                                BlockFieldNameConstants.Link,
                            }
                        },
                    }
                },
            };
            return templates;
        }
    }
}
