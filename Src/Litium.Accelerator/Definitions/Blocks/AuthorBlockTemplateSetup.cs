using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class AuthorBlockTemplateSetup : FieldTemplateSetup
    {
        private readonly CategoryService _categoryService;
        
        public AuthorBlockTemplateSetup(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public override IEnumerable<FieldTemplate> GetTemplates()
        {
            var pageCategoryId = _categoryService.Get(BlockCategoryNameConstants.Pages)?.SystemId ?? Guid.Empty;

            var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.Author)
                {
                    CategorySystemId = pageCategoryId,
                    Icon = "fas fa-user",
                    TemplatePath = "~/MVC:AuthorBlockController:Invoke",
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
                            Id = "AuthorInfo",
                            Collapsed = false,
                            Fields =
                            {
                                BlockFieldNameConstants.BlockImagePointer,
                                BlockFieldNameConstants.BlockTitle,
                                BlockFieldNameConstants.BlockText,
                                BlockFieldNameConstants.LinkText,
                                BlockFieldNameConstants.Link
                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
} 