using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litium.Products;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Services
{
    [Service(ServiceType = typeof(ProductService), Lifetime = DependencyLifetime.Scoped)]
    public abstract class ProductService
    {
        public abstract Task<IEnumerable<ProductModel>> GetMostSoldProductsAsync(Guid channelId, string articleNumber, int numberOfProducts = 4);
        public abstract RelatedModel GetProductRelationships(ProductModel productModel, string relationTypeName, bool includeBaseProductRelations = true, bool includeVariantRelations = true);
    }
}
