using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Blocks;

namespace Litium.Accelerator.Definitions.Blocks
{
    public class BlockCategorySetupImpl : BlockCategorySetup
    {
        public override IEnumerable<Category> GetCategories()
        {
            return new[]
            {
                new Category
                {
                    Id = BlockCategoryNameConstants.Customers,
                    Localizations =
                    {
                        ["sv-SE"] = {Name = "Kunder"},
                        ["en-US"] = {Name = "Customers" }
                    },
                },
                new Category
                {
                    Id = BlockCategoryNameConstants.Media,
                    Localizations =
                    {
                        ["sv-SE"] = {Name = "Media"},
                        ["en-US"] = {Name = "Media" }
                    }
                },
                new Category
                {
                    Id = BlockCategoryNameConstants.Pages,
                    Localizations =
                    {
                        ["sv-SE"] = {Name = "Sidor"},
                        ["en-US"] = {Name = "Pages" }
                    }
                },
                new Category
                {
                    Id = BlockCategoryNameConstants.Products,
                    Localizations =
                    {
                        ["sv-SE"] = {Name = "Produkter"},
                        ["en-US"] = {Name = "Products" }
                    }
                },
            };
        }
    }
}