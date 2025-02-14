using System;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.ViewModels;
using Litium.Accelerator.ViewModels.Product;
using Litium.Products;

namespace Litium.Accelerator.Builders.Product
{
    public class CategoryItemViewModelBuilder : IViewModelBuilder<CategoryItemViewModel>
    {
        private readonly CategoryService _categoryService;

        public CategoryItemViewModelBuilder(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public CategoryItemViewModel Build(Guid categorySystemId, DataFilterBase dataFilter = null)
        {
            if (categorySystemId == Guid.Empty)
            {
                return null;
            }
            var entity = _categoryService.Get(categorySystemId);
            if (entity == null)
            {
                return null;
            }
            var pageModel = new CategoryItemViewModel() { SystemId = categorySystemId };
            BuildFields(pageModel, entity, dataFilter?.Culture);
            return pageModel;
        }

        private void BuildFields(CategoryItemViewModel pageModel, Category entity, string culture)
        {
            var fields = entity.Fields;
            pageModel.Name = fields.GetName(culture);
            pageModel.Description = fields.GetDescription(culture);
            pageModel.Images = fields.GetImageUrls();
        }
    }
}
