using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Products;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Services
{
    internal class ProductServiceImpl : ProductService
    {
        private readonly RelatedModelBuilder _relatedModelBuilder;
        private readonly RelationshipTypeService _relationshipTypeService;

        public ProductServiceImpl(
            RelationshipTypeService relationshipTypeService,
            RelatedModelBuilder relatedModelBuilder)
        {
            _relationshipTypeService = relationshipTypeService;
            _relatedModelBuilder = relatedModelBuilder;
        }

        public override Task<IEnumerable<ProductModel>> GetMostSoldProductsAsync(Guid channelId, string articleNumber, int count)
        {
            return Task.FromResult(Enumerable.Empty<ProductModel>());
        }

        public override RelatedModel GetProductRelationships(ProductModel productModel, string relationTypeName, bool includeBaseProductRelations = true, bool includeVariantRelations = true)
        {
            if (string.IsNullOrEmpty(relationTypeName))
            {
                return null;
            }
            
            var relationshipType = _relationshipTypeService.Get(relationTypeName);
            return relationshipType != null ? _relatedModelBuilder.Build(productModel, relationshipType, includeBaseProductRelations, includeVariantRelations) : null;
        }
    }
}
