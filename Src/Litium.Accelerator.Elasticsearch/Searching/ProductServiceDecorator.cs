using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.Search;
using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.DependencyInjection;
using Litium.Search;
using Litium.Web.Models.Products;

namespace Litium.Accelerator.Search.Searching
{
    [ServiceDecorator(typeof(ProductService))]
    internal class ProductServiceDecorator : ProductService
    {
        private readonly ProductService _parent;
        private readonly SearchClientService _searchClientService;
        private readonly ProductSearchService _productSearchService;

        public ProductServiceDecorator(
            ProductService parent,
            ProductSearchService productSearchService,
            SearchClientService searchClientService)
        {
            _parent = parent;
            _productSearchService = productSearchService;
            _searchClientService = searchClientService;
        }

        public override async Task<IEnumerable<ProductModel>> GetMostSoldProductsAsync(Guid channelId, string articleNumber, int numberOfProducts)
        {
            if (!_searchClientService.IsConfigured)
            {
                return await _parent.GetMostSoldProductsAsync(channelId, articleNumber, numberOfProducts);
            }

            var excludeArticleNumbers = new string[] { articleNumber }.Where(n => !string.IsNullOrEmpty(n)).Distinct();
            var rawSearchResponse = await _searchClientService
                .SearchAsync<PurchaseHistoryDocument>(selector => selector
                    .Size(0)
                    .Query(queryContainerDescriptor =>
                        queryContainerDescriptor.Term(t => t.Field(x => x.ArticleNumbers).Value(articleNumber))
                        && queryContainerDescriptor.Term(t => t.Field(x => x.ChannelSystemId).Value(channelId))
                     )
                    .Aggregations(aggregationDescriptor => aggregationDescriptor.SignificantTerms("product_recommendations", selector => selector
                                                                                    .Field(x => x.ArticleNumbers)
                                                                                    .Exclude(excludeArticleNumbers)
                                                                                    .MinimumDocumentCount(2)
                                                                                    .Size(10)))
                );
            var articleNumbers = rawSearchResponse.Aggregations.SignificantTerms("product_recommendations")
                .Buckets
                .OrderByDescending(i => i.Score)
                .Select(i => i.Key)
                .ToList();
            if (articleNumbers.Count < 1)
            {
                return Enumerable.Empty<ProductModel>();
            }

            var productSearchQuery = new SearchQuery()
            {
                PageSize = numberOfProducts,
                ArticleNumbers = articleNumbers,
            };
            var searchResult = await _productSearchService.SearchAsync(productSearchQuery, addCategoryFilterTags: true);

            return searchResult.Items.Value.OfType<ProductSearchResult>()
                .Select(x => x.Item);
        }

        public override RelatedModel GetProductRelationships(ProductModel productModel, string relationTypeName, bool includeBaseProductRelations = true, bool includeVariantRelations = true)
        {
            return _parent.GetProductRelationships(productModel, relationTypeName, includeBaseProductRelations, includeVariantRelations);
        }
    }
}
