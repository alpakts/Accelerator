using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class ProductsAndBannerBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;
        public ProductsAndBannerBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var productCategoryId = _categoryService.Get(BlockCategoryNameConstants.Products)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.ProductsAndBanner)
                {
                    CategorySystemId = productCategoryId,
                    Icon = "fas fa-image",
                    FieldGroups = new List<FieldTemplateFieldGroup>()
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
                            Id = "Banner",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.BlockImagePointer,
                                BlockFieldNameConstants.LinkText,
                                BlockFieldNameConstants.BannerLinkToCategory,
                                BlockFieldNameConstants.BannerLinkToPage,
                                BlockFieldNameConstants.BannerLinkToProduct,
                                BlockFieldNameConstants.ActionText
                            }
                        }
                    }
                },
            };

            foreach (var fieldTemplateFieldGroup in GetProductBlock())
            {
                foreach (var fieldTemplate in templates)
                {
                    ((BlockFieldTemplate)fieldTemplate).FieldGroups.Add(fieldTemplateFieldGroup);
                }
            }

            return templates;
        }

        private List<FieldTemplateFieldGroup> GetProductBlock()
        {
            return new List<FieldTemplateFieldGroup>()
            {
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
                        BlockFieldNameConstants.ProductLinkText,
                        BlockFieldNameConstants.LinkToCategory,
                        BlockFieldNameConstants.LinkToPage,
                        BlockFieldNameConstants.ShowProductToTheRight
                    }
                }
            };
        }
    }
}
