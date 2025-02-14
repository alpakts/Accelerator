using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Products;

namespace Litium.Accelerator.Definitions.Products
{
    internal class ProductsRelationshipTypeSetup : RelationshipTypeSetup
    {
        public override IEnumerable<RelationshipType> GetRelationshipTypes()
        {
            var relationshipTypes = new[]
            {
                new RelationshipType
                {
                    Id = RelationTypeNameConstants.Accessory,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = "Tillbehör" },
                        ["en-US"] = { Name = "Accessories" }
                    }
                },
                new RelationshipType
                {
                    Id = RelationTypeNameConstants.SimilarProducts,
                    Localizations =
                    {
                        ["sv-SE"] = { Name = "Liknande produkter" },
                        ["en-US"] = { Name = "Similar products" }
                    },
                    Bidirectional = true
                }
            };
            return relationshipTypes;
        }
    }
}
