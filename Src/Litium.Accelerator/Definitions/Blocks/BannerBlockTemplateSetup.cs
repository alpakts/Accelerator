using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class BannerBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;

        public BannerBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var pageCategoryId = _categoryService.Get(BlockCategoryNameConstants.Pages)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.Banner)
                {
                    CategorySystemId = pageCategoryId,
                    Icon = "fas fa-image",
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
                            Id = "Banners",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.Banners,
                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
}
