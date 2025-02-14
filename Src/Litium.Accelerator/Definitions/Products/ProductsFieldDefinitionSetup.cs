using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;

namespace Litium.Accelerator.Definitions.Products
{
    internal class ProductsFieldDefinitionSetup : FieldDefinitionSetup
    {
        public override IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            var fields = new List<FieldDefinition>
            {
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Brand, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    UseInStorefront = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "Adrian Hammond",
                                Name = new Dictionary<string, string> { { "en-US", "Adrian Hammond" }, { "sv-SE", "Adrian Hammond" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Dolce & Gabbana",
                                Name = new Dictionary<string, string> { { "en-US", "Dolce & Gabbana" }, { "sv-SE", "Dolce & Gabbana" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Le specs",
                                Name = new Dictionary<string, string> { { "en-US", "Le specs" }, { "sv-SE", "Le specs" } }
                            },
                            new TextOption.Item
                            {
                                Value = "MaQ",
                                Name = new Dictionary<string, string> { { "en-US", "MaQ" }, { "sv-SE", "MaQ" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Nolita",
                                Name = new Dictionary<string, string> { { "en-US", "Nolita" }, { "sv-SE", "Nolita" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Odd Molly",
                                Name = new Dictionary<string, string> { { "en-US", "Odd Molly" }, { "sv-SE", "Odd Molly" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Saint Tropez",
                                Name = new Dictionary<string, string> { { "en-US", "Saint Tropez" }, { "sv-SE", "Saint Tropez" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Vila",
                                Name = new Dictionary<string, string> { { "en-US", "Vila" }, { "sv-SE", "Vila" } }
                            },
                            new TextOption.Item
                            {
                                Value = "HÅG",
                                Name = new Dictionary<string, string> { { "en-US", "HÅG" }, { "sv-SE", "HÅG" } }
                            },
                            new TextOption.Item
                            {
                                Value = "RBM",
                                Name = new Dictionary<string, string> { { "en-US", "RBM" }, { "sv-SE", "RBM" } }
                            },
                            new TextOption.Item
                            {
                                Value = "RH",
                                Name = new Dictionary<string, string> { { "en-US", "RH" }, { "sv-SE", "RH" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Casall",
                                Name = new Dictionary<string, string> { { "en-US", "Casall" }, { "sv-SE", "Casall" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Color, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    UseInStorefront = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "White",
                                Name = new Dictionary<string, string> { { "en-US", "White" }, { "sv-SE", "Vit" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Beige",
                                Name = new Dictionary<string, string> { { "en-US", "Beige" }, { "sv-SE", "Beige" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Black",
                                Name = new Dictionary<string, string> { { "en-US", "Black" }, { "sv-SE", "Svart" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Grey",
                                Name = new Dictionary<string, string> { { "en-US", "Grey" }, { "sv-SE", "Grå" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Rose",
                                Name = new Dictionary<string, string> { { "en-US", "Rose" }, { "sv-SE", "Rosa" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Brown",
                                Name = new Dictionary<string, string> { { "en-US", "Brown" }, { "sv-SE", "Brun" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Red",
                                Name = new Dictionary<string, string> { { "en-US", "Red" }, { "sv-SE", "Röd" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Blue",
                                Name = new Dictionary<string, string> { { "en-US", "Blue" }, { "sv-SE", "Blå" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Green",
                                Name = new Dictionary<string, string> { { "en-US", "Green" }, { "sv-SE", "Grön" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.News, SystemFieldTypeConstants.Date)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = false,
                    MultiCulture = true,
                    UseInStorefront = true
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.ProductSheet, SystemFieldTypeConstants.MediaPointerFile)
                {
                    Editable = true,
                    CanBeGridColumn = false,
                    CanBeGridFilter = true,
                    MultiCulture = true,
                    UseInStorefront = true
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Specification, SystemFieldTypeConstants.MultirowText)
                {
                    Editable = true,
                    CanBeGridColumn = false,
                    CanBeGridFilter = true,
                    MultiCulture = true,
                    UseInStorefront = true
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.AcceleratorFilterFields, "FilterFields")
                {
                    Editable = true,
                    CanBeGridColumn = false,
                    CanBeGridFilter = false,
                    MultiCulture = false,
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Size, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    UseInStorefront = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "S",
                                Name = new Dictionary<string, string> { { "en-US", "S" }, { "sv-SE", "S" } }
                            },
                            new TextOption.Item
                            {
                                Value = "One Size",
                                Name = new Dictionary<string, string> { { "en-US", "One Size" }, { "sv-SE", "One Size" } }
                            },
                            new TextOption.Item
                            {
                                Value = "M",
                                Name = new Dictionary<string, string> { { "en-US", "M" }, { "sv-SE", "M" } }
                            },
                            new TextOption.Item
                            {
                                Value = "L",
                                Name = new Dictionary<string, string> { { "en-US", "L" }, { "sv-SE", "L" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Type, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "Outdoor",
                                Name = new Dictionary<string, string> { { "en-US", "Outdoor" }, { "sv-SE", "Outdoor" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Yoga",
                                Name = new Dictionary<string, string> { { "en-US", "Yoga" }, { "sv-SE", "Yoga" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Padel",
                                Name = new Dictionary<string, string> { { "en-US", "Padel" }, { "sv-SE", "Padel" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Running",
                                Name = new Dictionary<string, string> { { "en-US", "Running" }, { "sv-SE", "Löpning" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Loungewear",
                                Name = new Dictionary<string, string> { { "en-US", "Loungewear" }, { "sv-SE", "Loungewear" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Gym",
                                Name = new Dictionary<string, string> { { "en-US", "Gym" }, { "sv-SE", "Gym" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Weight, SystemFieldTypeConstants.Decimal)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                    UseInStorefront = true
                },

                // G O O G L E  S H O P P I N G  F I E L D S
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.AgeGroup, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "Newborn",
                                Name = new Dictionary<string, string> { { "en-US", "Newborn" }, { "sv-SE", "Nyfödd" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Infant",
                                Name = new Dictionary<string, string> { { "en-US", "Infant" }, { "sv-SE", "Spädbarn" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Toddler",
                                Name = new Dictionary<string, string> { { "en-US", "Toddler" }, { "sv-SE", "Småbarn" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Kids",
                                Name = new Dictionary<string, string> { { "en-US", "Kids" }, { "sv-SE", "Ungar" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Adult",
                                Name = new Dictionary<string, string> { { "en-US", "Adult" }, { "sv-SE", "Vuxen" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Adult, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Condition, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "New",
                                Name = new Dictionary<string, string> { { "en-US", "New" }, { "sv-SE", "Ny" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Refurbished",
                                Name = new Dictionary<string, string> { { "en-US", "Refurbished" }, { "sv-SE", "Restaurerad" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Used",
                                Name = new Dictionary<string, string> { { "en-US", "Used" }, { "sv-SE", "Begagnad" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.Gender, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    Option = new TextOption
                    {
                        MultiSelect = false,
                        Items = new List<TextOption.Item>
                        {
                            new TextOption.Item
                            {
                                Value = "Male",
                                Name = new Dictionary<string, string> { { "en-US", "Male" }, { "sv-SE", "Herr" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Female",
                                Name = new Dictionary<string, string> { { "en-US", "Female" }, { "sv-SE", "Dam" } }
                            },
                            new TextOption.Item
                            {
                                Value = "Unisex",
                                Name = new Dictionary<string, string> { { "en-US", "Unisex" }, { "sv-SE", "Unisex" } }
                            }
                        }
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.GTIN, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.IsBundle, SystemFieldTypeConstants.Boolean)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.MPN, SystemFieldTypeConstants.Text)
                {
                    Editable = true,
                    CanBeGridColumn = true,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.GoogleProductCategory, SystemFieldTypeConstants.TextOption)
                {
                    Editable = true,
                    CanBeGridColumn = false,
                    CanBeGridFilter = true,
                    MultiCulture = false,
                    Option = new TextOption
                    {
                        MultiSelect = false
                    }
                },
                new FieldDefinition<ProductArea>(ProductFieldNameConstants.OrganizationsPointer, SystemFieldTypeConstants.Pointer)
                {
                    Editable = true,
                    CanBeGridColumn = false,
                    CanBeGridFilter = false,
                    MultiCulture = false,
                    Option = new PointerOption
                    {
                        EntityType = PointerTypeConstants.CustomersOrganization,
                        MultiSelect = true
                    }
                },
            };

            fields.ForEach(fields =>
            {
                fields.UseInSearchEngine = true;
            });
            return fields;
        }
    }
}
