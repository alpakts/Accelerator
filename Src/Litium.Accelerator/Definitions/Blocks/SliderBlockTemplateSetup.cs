using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class SliderBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;

        public SliderBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var pageCategoryId = _categoryService.Get(BlockCategoryNameConstants.Pages)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.Slider)
                {
                    CategorySystemId = pageCategoryId,
                    Icon = "fas fa-images",
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
                            Id = "Slides",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.Slider,
                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
}
