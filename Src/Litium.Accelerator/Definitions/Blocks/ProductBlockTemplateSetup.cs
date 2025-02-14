using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class ProductBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;
        public ProductBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var productCategoryId = _categoryService.Get(BlockCategoryNameConstants.Products)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.Product)
                {
                    CategorySystemId = productCategoryId,
                    Icon = "fas fa-tag",
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
                            Id = "Products",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.BlockTitle,
                                BlockFieldNameConstants.CategoryLink,
                                BlockFieldNameConstants.ProductListLink,
                                BlockFieldNameConstants.ProductsLinkList,
                                BlockFieldNameConstants.NumberOfProducts,
                                BlockFieldNameConstants.ProductSorting,
                                BlockFieldNameConstants.LinkText,
                                BlockFieldNameConstants.LinkToCategory,
                                BlockFieldNameConstants.LinkToPage,
                                BlockFieldNameConstants.ShowProductToTheRight
                            }
                        },
                    }
                },
            };
            return templates;
        }
    }
}
