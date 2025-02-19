using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;
using Litium.FieldFramework.FieldTypes;
using Litium.Accelerator.Search;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class BlockFieldDefinitionSetup : FieldDefinitionSetup
    {
        public override IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var fields = new List<FieldDefinition>();

            fields.AddRange(GeneralFields());
            fields.AddRange(BrandFields());
            fields.AddRange(BannerFields());
            fields.AddRange(ProductFields());
            fields.AddRange(ProductsAndBannerFields());
            fields.AddRange(SliderFields());
            fields.AddRange(AuthorFields());

            foreach (var field in fields)
            {
                field.UseInStorefront = true;
                field.UseInSearchEngine = true;
            }
            return fields;
        }

        private IEnumerable<FieldDefinition> GeneralFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockTitle, SystemFieldTypeConstants.Text)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockText, SystemFieldTypeConstants.Text)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.Link, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.LinkText, SystemFieldTypeConstants.Text)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockVideo, SystemFieldTypeConstants.Pointer)
                {
                    MultiCulture = true,
                    Option = new PointerOption { EntityType = PointerTypeConstants.MediaVideo },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.ActionText, SystemFieldTypeConstants.Text)
                {
                    MultiCulture = true,
                    Editable = true,
                },
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> BrandFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BrandsLinkList, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage, MultiSelect = true },
                    Editable = true,
                }
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> BannerFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.Banners, SystemFieldTypeConstants.MultiField)
                {
                    Option = new MultiFieldOption { IsArray = true, Fields = new List<string>(){ BlockFieldNameConstants.LinkText, BlockFieldNameConstants.BlockImagePointer, BlockFieldNameConstants.BannerLinkToCategory, BlockFieldNameConstants.BannerLinkToPage, BlockFieldNameConstants.BannerLinkToProduct, BlockFieldNameConstants.ActionText } },
                    Editable = true,
                }
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> ProductFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.CategoryLink, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.ProductListLink, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsProductList },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.ProductsLinkList, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsProduct, MultiSelect = true},
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.NumberOfProducts, SystemFieldTypeConstants.Int){
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.ProductSorting, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = SearchQueryConstants.Popular,
                                Name = new Dictionary<string, string> { { "en-US", "Popular" }, { "sv-SE", "Populära" } }
                            },
                            new TextOption.Item
                            {
                                Value = SearchQueryConstants.Recommended,
                                Name = new Dictionary<string, string> { { "en-US", "Recommended" }, { "sv-SE", "Rekommenderade" } }
                            },
                            new TextOption.Item
                            {
                                Value = SearchQueryConstants.News,
                                Name = new Dictionary<string, string> { { "en-US", "News" }, { "sv-SE", "Nyheter" } }
                            }
                        }
                    }
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.LinkToCategory, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.LinkToPage, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BannerLinkToCategory, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsCategory },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BannerLinkToPage, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage },
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BannerLinkToProduct, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.ProductsProduct },
                    Editable = true,
                },
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> ProductsAndBannerFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.ShowProductToTheRight, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.ProductLinkText, SystemFieldTypeConstants.Text) {
                    Editable = true,
                    MultiCulture = true
                }
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> SliderFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.Slider, SystemFieldTypeConstants.MultiField)
                {
                    Editable = true,
                    Option = new MultiFieldOption { IsArray = true, Fields = new List<string>(){ BlockFieldNameConstants.LinkText, BlockFieldNameConstants.BlockImagePointer, BlockFieldNameConstants.BannerLinkToCategory, BlockFieldNameConstants.BannerLinkToPage, BlockFieldNameConstants.BannerLinkToProduct, BlockFieldNameConstants.ActionText } }
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockImagePointer, SystemFieldTypeConstants.MediaPointerImage)
                {
                    Editable = true,
                    MultiCulture = true,
                }
            };
            return fields;
        }

        private IEnumerable<FieldDefinition> AuthorFields()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockImagePointer, SystemFieldTypeConstants.MediaPointerImage)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockTitle, SystemFieldTypeConstants.Text)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.BlockText, SystemFieldTypeConstants.Editor)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.LinkText, SystemFieldTypeConstants.Text)
                {
                    MultiCulture = true,
                    Editable = true,
                },
                new FieldDefinition<BlockArea>(BlockFieldNameConstants.Link, SystemFieldTypeConstants.Pointer)
                {
                    Option = new PointerOption { EntityType = PointerTypeConstants.WebsitesPage },
                    Editable = true,
                }
            };
            return fields;
        }
    }
}
