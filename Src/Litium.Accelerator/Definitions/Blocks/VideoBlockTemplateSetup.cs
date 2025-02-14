using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class VideoBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;

        public VideoBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var pageCategoryId = _categoryService.Get(BlockCategoryNameConstants.Pages)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.Video)
                {
                    CategorySystemId = pageCategoryId,
                    Icon = "fas fa-file-video",
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
                            Id = "Video",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.BlockVideo,
                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
}
