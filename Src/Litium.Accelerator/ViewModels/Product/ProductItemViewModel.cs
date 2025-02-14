using Litium.Globalization;
using Litium.Web.Models;
using Litium.Web.Models.Products;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Product
{
    public class ProductItemViewModel : IViewModel
    {
        public string Brand { get; set; }
        public string Color { get; set; }
        public Currency Currency { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public IList<ImageModel> Images { get; set; }
        public bool IsInStock { get; set; }
        public string Name { get; set; }
        public ProductPriceModel Price { get; set; }
        public ProductModel Product { get; set; }
        public string QuantityFieldId { get; set; }
        public bool ShowBuyButton { get; set; }
        public bool ShowQuantityField { get; set; }
        public string Size { get; set; }
        public string StockStatusDescription { get; set; }
        public string Url { get; set; }
        public bool UseVariantUrl { get; set; }
    }
}
