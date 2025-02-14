using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Litium.Accelerator.ViewModels.Search;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Globalization;
using Litium.Products;
using Litium.Runtime.DependencyInjection;
using Litium.Security;
using Litium.Web;

namespace Litium.Accelerator.Search
{
    [Service(ServiceType = typeof(FilterAggregator), Lifetime = DependencyLifetime.Scoped)]
    public abstract class FilterAggregator
    {
        private readonly CategoryService _categoryService;
        private readonly CurrencyService _currencyService;
        private readonly UrlService _urlService;
        private readonly AuthorizationService _authorizationService;

        public FilterAggregator(
            CategoryService categoryService,
            CurrencyService currencyService,
            UrlService urlService,
            AuthorizationService authorizationService)
        {
            _categoryService = categoryService;
            _currencyService = currencyService;
            _urlService = urlService;
            _authorizationService = authorizationService;
        }

        public abstract Task<IEnumerable<GroupFilter>> GetFilterAsync(SearchQuery searchQuery, IEnumerable<string> fieldNames);

        protected static int Round(int value, bool roundUp)
        {
            var roundLevels = new[]
            {
                // param 1: price below, param 2: round to nerest
                new[] { 100, 10 },
                new[] { 1000, 100 },
                new[] { 5000, 500 },
                new[] { 10000, 1000 },
                new[] { 15000, 1500 },
                new[] { 20000, 2000 },
                new[] { 25000, 2500 },
                new[] { 50000, 5000 },
                new[] { 100000, 10000 },
                new[] { 150000, 15000 },
                new[] { 500000, 50000 },
                new[] { 1000000, 100000 },
                new[] { 10000000, 1000000 },
                new[] { int.MaxValue, 10000000 }
            };
            foreach (var item in roundLevels)
            {
                if (value <= item[0])
                {
                    if (roundUp)
                    {
                        return value + item[1] - (value % item[1]);
                    }

                    return value - (value % item[1]);
                }
            }

            throw new ArgumentException("No matching roundings for actual value.", nameof(value));
        }

        private static string FindValue(FieldDefinition fieldDefinition, string term, CultureInfo cultureInfo)
        {
            if (fieldDefinition.FieldType == SystemFieldTypeConstants.TextOption)
            {
                var option = fieldDefinition.Option as TextOption;

                return option?.Items.FirstOrDefault(x => x.Name.TryGetValue(cultureInfo.Name, out string value)
                    && term.Equals(value, StringComparison.OrdinalIgnoreCase))?.Value;
            }

            return term;
        }

        private static string FormatPrice(Currency currency, int value)
        {
            return currency.Format(value, false, CultureInfo.CurrentUICulture);
        }

        protected GroupFilter GetFilterTag(SearchQuery searchQuery, FieldDefinition field, Dictionary<string, int> tagValues)
        {
            if (!searchQuery.Tags.TryGetValue(field.Id, out ISet<string> selectedValues))
            {
                selectedValues = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return new GroupFilter
            {
                Name = field.Localizations.CurrentCulture.Name ?? field.Id,
                Attributes = new Dictionary<string, string>
                {
                    { "value", NormalizeTag(field.Id) },
                },
                IsSelected = searchQuery.Tags.ContainsKey(field.Id),
                Links = tagValues
                    .Select(x =>
                    {
                        string key;
                        switch (field.FieldType)
                        {
                            case SystemFieldTypeConstants.Decimal:
                            case SystemFieldTypeConstants.DecimalOption:
                                {
                                    if (decimal.TryParse(x.Key, NumberStyles.Any, CultureInfo.CurrentCulture, out var decValue))
                                    {
                                        key = decValue.ToString("0.########", CultureInfo.CurrentCulture);
                                    }
                                    else
                                    {
                                        key = x.Key;
                                    }
                                    break;
                                }
                            case SystemFieldTypeConstants.Int:
                            case SystemFieldTypeConstants.IntOption:
                                {
                                    if (int.TryParse(x.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
                                    {
                                        key = intValue.ToString(CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        key = x.Key;
                                    }
                                    break;
                                }
                            case SystemFieldTypeConstants.Long:
                                {
                                    if (long.TryParse(x.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out var longValue))
                                    {
                                        key = longValue.ToString(CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        key = x.Key.TrimStart('0');
                                    }
                                    break;
                                }
                            case SystemFieldTypeConstants.Date:
                            case SystemFieldTypeConstants.DateTime:
                                {
                                    if (long.TryParse(x.Key, NumberStyles.Any, CultureInfo.InvariantCulture, out long l))
                                    {
                                        key = new DateTime(l).ToShortDateString();
                                    }
                                    else
                                    {
                                        goto default;
                                    }
                                    break;
                                }
                            case SystemFieldTypeConstants.Boolean:
                                {
                                    key = string.Format(@"accelerator.systembooleanfield.{0}",x.Key.ToLower()).AsWebsiteText();
                                    break;
                                }
                            default:
                                {
                                    key = x.Key;
                                    break;
                                }
                        }

                        var selected = selectedValues.Contains(x.Key);

                        return new FilterItem
                        {
                            Name = key,
                            IsSelected = selected,
                            Url = searchQuery.GetUrlTag(field.Id, x.Key, selected),
                            Count = x.Value,
                            Attributes = new Dictionary<string, string>
                            {
                                { "value", NormalizeTag(x.Key) },
                                { "cssValue", (FindValue(field, key, CultureInfo.CurrentCulture) ?? key)?.ToLowerInvariant() },
                            }
                        };
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Name)
                    .ToList()
            };
        }

        protected static GroupFilter GetNewsTag(SearchQuery searchQuery)
        {
            if (searchQuery.ProductListSystemId != null && searchQuery.ProductListSystemId == Guid.Empty)
            {
                return null;
            }

            var dteValue1 = new Tuple<DateTime, DateTime>(DateTime.Today.AddMonths(-1), DateTime.Today);
            var dteValue3 = new Tuple<DateTime, DateTime>(DateTime.Today.AddMonths(-3), DateTime.Today);

            return new GroupFilter
            {
                Attributes = new Dictionary<string, string>
                {
                    { "value", "news" },
                },
                Name = "filter.newsheadline".AsWebsiteText(),
                SingleSelect = true,
                Links = new List<FilterItem>(new[]
                {
                    new FilterItem
                    {
                        Name = "filter.newslastmonth".AsWebsiteText(),
                        IsSelected = Equals(dteValue1, searchQuery.NewsDate),
                        Url = searchQuery.GetUrlNews(dteValue1, Equals(dteValue1, searchQuery.NewsDate)),
                        Attributes = new Dictionary<string, string>
                        {
                            { "value", string.Format("{0:yyyyMMdd}-{1:yyyyMMdd}", dteValue1.Item1, dteValue1.Item2) }
                        }
                    },
                    new FilterItem
                    {
                        Name = "filter.newslast3month".AsWebsiteText(),
                        IsSelected = Equals(dteValue3, searchQuery.NewsDate),
                        Url = searchQuery.GetUrlNews(dteValue3, Equals(dteValue3, searchQuery.NewsDate)),
                        Attributes = new Dictionary<string, string>
                        {
                            { "value", string.Format("{0:yyyyMMdd}-{1:yyyyMMdd}", dteValue3.Item1, dteValue3.Item2) }
                        }
                    }
                })
            };
        }

        protected static IEnumerable<(int LastPrice, int MaxSlotPrice, int Count)> GetPriceGroups(Dictionary<decimal, int> priceHits, int minPrice, int maxPrice)
        {
            const decimal slots = 7;

            var roundedMaxPrice = Round(maxPrice, true);
            var priceInEachInterval = (int)Math.Floor(roundedMaxPrice / slots);

            var result = new List<(int, int, int)>();
            var lastPrice = Round(minPrice, false);
            for (var i = 1; lastPrice < maxPrice; i++)
            {
                var i1 = i * priceInEachInterval;
                var maxSlotPrice = Round(i1, true);
                var items = priceHits.Where(x => x.Key >= lastPrice && x.Key <= maxSlotPrice).Sum(x => x.Value);

                //Rounding off can make lastPrice and maxSlotPrice have the same value
                if (items > 0 && lastPrice < maxSlotPrice)
                {
                    result.Add((lastPrice, maxSlotPrice, items));
                }

                lastPrice = maxSlotPrice;
            }

            return result;
        }

        protected GroupFilter GetPriceTag(SearchQuery searchQuery, IList<(int LastPrice, int MaxSlotPrice, int Count)> priceHits, bool includeChild, Guid currencySystemId)
        {
            if (priceHits.Count > 0)
            {
                var currency = _currencyService.Get(currencySystemId);

                return new GroupFilter
                {
                    Name = "filter.price".AsWebsiteText(),
                    Attributes = new Dictionary<string, string>
                    {
                        { "value", "price_range" }
                    },
                    IsSelected = searchQuery.ContainsPriceFilter(),
                    Links = includeChild
                        ? priceHits
                            .Select(x =>
                            {
                                var extraInfo = x.Count.ToString(CultureInfo.CurrentCulture.NumberFormat);
                                var currentPriceRange = (x.LastPrice, x.MaxSlotPrice);
                                var selected = searchQuery.PriceRanges.Contains(currentPriceRange);
                                return new FilterItem
                                {
                                    Name = string.Format("{0}-{1}", FormatPrice(currency, x.LastPrice), FormatPrice(currency, x.MaxSlotPrice)),
                                    Count = int.Parse(x.Count.ToString(CultureInfo.CurrentCulture.NumberFormat)),
                                    IsSelected = selected,
                                    Url = searchQuery.GetUrlPrice(currentPriceRange, selected),
                                    Attributes = new Dictionary<string, string>
                                    {
                                        { "value", string.Format("{0}-{1}", x.LastPrice, x.MaxSlotPrice) },
                                    }
                                };
                            })
                            .ToList()
                        : null
                };
            }

            return null;
        }

        protected GroupFilter GetProductCategoryTag(SearchQuery searchQuery, Dictionary<Guid, int> tags)
        {
            return new GroupFilter
            {
                Attributes = new Dictionary<string, string>
                {
                    { "value", "category" }
                },
                Name = "filter.productcategories".AsWebsiteText(),
                Links = _categoryService
                    .Get(tags.Select(x => x.Key))
                    .Where(x => _authorizationService.HasOperation<Category>(Operations.Entity.Read, x.SystemId))
                    .Select(x => new { Category = x, Count = tags[x.SystemId] })
                    .Where(x => !string.IsNullOrEmpty(_urlService.GetUrl(x.Category)))
                    .Select(x =>
                    {
                        var isSelected = searchQuery.Category.Contains(x.Category.SystemId);

                        return new FilterItem
                        {
                            Name = x.Category.Fields.GetValue<string>(SystemFieldDefinitionConstants.Name, CultureInfo.CurrentCulture),
                            IsSelected = isSelected,
                            Url = searchQuery.GetUrlCategory(x.Category.SystemId, isSelected),
                            Count = x.Count,
                            Attributes = new Dictionary<string, string>
                            {
                                { "value", x.Category.SystemId.ToString("N") }
                            }
                        };
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Name)
                    .ToList()
            };
        }

        protected virtual string NormalizeTag(string text)
            => text.ToLowerInvariant();
    }

    internal class FilterAggregatorImpl : FilterAggregator
    {
        public FilterAggregatorImpl(
            CategoryService categoryService,
            CurrencyService currencyService,
            UrlService urlService,
            AuthorizationService authorizationService)
            : base(categoryService, currencyService, urlService, authorizationService)
        {
        }

        public override Task<IEnumerable<GroupFilter>> GetFilterAsync(SearchQuery searchQuery, IEnumerable<string> fieldNames)
        {
            return Task.FromResult<IEnumerable<GroupFilter>>(Array.Empty<GroupFilter>());
        }
    }
}
