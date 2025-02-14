using System;
using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Products;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.ViewModels.Brand;
using Litium.Products;

namespace Litium.Accelerator.ViewModels.Product
{
    public class ProductPageViewModel : PageViewModel, IAutoMapperConfiguration
    {
        public ProductItemViewModel ProductItem { get; set; }
        public string ColorText { get; set; }
        public List<ProductFilterItem> Colors { get; set; } = new List<ProductFilterItem>();
        public string SizeText { get; set; }
        public List<ProductFilterItem> Sizes { get; set; } = new List<ProductFilterItem>();
        public BrandViewModel BrandPage { get; set; }

        public List<ProductItemViewModel> BundleProducts { get; set; } = new List<ProductItemViewModel>();
        public KeyValuePair<RelationshipType, List<ProductItemViewModel>> AccessoriesProducts { get; set; }
        public KeyValuePair<RelationshipType, List<ProductItemViewModel>> SimilarProducts { get; set; }
        public List<ProductItemViewModel> MostSoldProducts { get; set; }
        public List<ProductItemViewModel> Variants { get; set; } = new List<ProductItemViewModel>();

        public IEnumerable<ProductFieldGroupViewModel> ProductFieldGroups {get;set;} = new List<ProductFieldGroupViewModel>();

        public void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ProductModel, ProductPageViewModel>();
        }

        public class ProductFilterItem
        {
            public ProductFilterItem(Variant variant, string selectedFilter1, string selectedFilter2, string value, string title, Guid websiteId, Guid channelId)
            {
                Value = value;
                IsActive = selectedFilter1 == value;
                SelectedFilter = selectedFilter2;
                Title = title;
                Url = variant.GetUrl(channelSystemId: channelId);
            }

            public bool Enabled { get; set; }
            public bool IsActive { get; set; }
            public string SelectedFilter { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public string Value { get; set; }
        }
    }
}
