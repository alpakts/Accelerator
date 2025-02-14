using System;
using System.Collections.Generic;
using Litium.Products;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Search.Indexing.Products
{
    [Service(ServiceType = typeof(ProductIndexingContext), Lifetime = DependencyLifetime.Singleton)]
    public class ProductIndexingContext
    {
        private readonly CategoryService _categoryService;

        public ProductIndexingContext(
            CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<Category> GetParentCatagories(Guid categorySystemId)
        {
            var result = new List<Category>();
            var current = _categoryService.Get(categorySystemId);
            while (current != null)
            {
                result.Add(current);
                current = _categoryService.Get(current.ParentCategorySystemId);
            }
            return result;
        }
    }
}
