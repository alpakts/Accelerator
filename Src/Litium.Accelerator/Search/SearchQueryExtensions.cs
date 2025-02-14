using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Litium.Accelerator.Constants;
using Litium.Data.Queryable;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using Litium.Websites;

namespace Litium.Accelerator.Search
{
    public static class SearchQueryExtensions
    {
        private static readonly LazyService<UrlService> _urlService = new LazyService<UrlService>();

        public static bool ContainsFilter(this SearchQuery searchQuery, string exceptTag = null, bool includePriceFilter = true, bool includeCategoryFilter = true)
        {
            return searchQuery.Tags.Any(x => exceptTag is null || exceptTag != x.Key)
                || searchQuery.ContainsNewsFilter()
                || (includePriceFilter && searchQuery.ContainsPriceFilter())
                || (includeCategoryFilter && searchQuery.ContainsCategoryFilter());
        }

        public static bool ContainsMultipleFilters(this SearchQuery searchQuery)
        {
            return searchQuery.Tags.Count + searchQuery.Category.Count + (searchQuery.ContainsNewsFilter() ? 1 : 0) + searchQuery.PriceRanges.Count > 1;
        }

        public static bool ContainsCategoryFilter(this SearchQuery searchQuery)
        {
            return searchQuery.Category.Count > 0;
        }

        public static bool ContainsNewsFilter(this SearchQuery searchQuery)
        {
            return searchQuery.NewsDate != null;
        }

        public static bool ContainsPriceFilter(this SearchQuery searchQuery)
        {
            return searchQuery.PriceRanges.Count > 0;
        }

        public static string GetUrl(this SearchQuery searchQuery, Action<SearchQuery> action = null)
        {
            var query = action == null ? searchQuery : searchQuery.Clone();
            if (action != null)
            {
                query.PageNumber = 1;
                //if (CurrentState.Current.PageType.Name.IsBrandPageType())
                //{
                //    query.NamedData = null;
                //}
                action(query);
            }

            var sb = new StringBuilder();
            if (query.Type != default)
            {
                sb.Append('&');
                sb.Append(SearchQueryConstants.Type);
                sb.Append('=');
                sb.Append(query.Type);
            }

            if (query.Category != null)
            {
                foreach (var item in query.Category)
                {
                    sb.Append('&');
                    sb.Append(SearchQueryConstants.Category);
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(item.ToString("N")));
                }
            }

            if (query.Tags != null)
            {
                foreach (var tag in query.Tags)
                {
                    if (tag.Value.Count > 0)
                    {
                        foreach (var value in tag.Value)
                        {
                            sb.Append('&');
                            sb.Append(HttpUtility.UrlEncode(tag.Key));
                            sb.Append('=');
                            sb.Append(HttpUtility.UrlEncode(value));
                        }
                    }
                }
            }

            if (query.PriceRanges != null)
            {
                foreach (var value in query.PriceRanges)
                {
                    sb.Append('&');
                    sb.Append(HttpUtility.UrlEncode(SearchQueryConstants.PriceRange));
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(string.Format("{0}-{1}", value.Item1, value.Item2)));
                }
            }

            if (query.NewsDate != null)
            {
                sb.Append('&');
                sb.Append(HttpUtility.UrlEncode(SearchQueryConstants.News));
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(string.Format("{0:yyyyMMdd}-{1:yyyyMMdd}", query.NewsDate.Item1, query.NewsDate.Item2)));
            }

            if (query.PageNumber > 1)
            {
                sb.Append('&');
                sb.Append(SearchQueryConstants.Page);
                sb.Append('=');
                sb.Append(query.PageNumber);
            }

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                sb.Append('&');
                sb.Append(SearchQueryConstants.SortBy);
                sb.Append('=');
                sb.Append(query.SortBy);
            }
            if (query.SortDirection == SortDirection.Descending)
            {
                sb.Append('&');
                sb.Append(SearchQueryConstants.SortDirection);
                sb.Append('=');
                sb.Append(SortDirection.Descending.ToString());
            }

            if (!string.IsNullOrEmpty(query.Text))
            {
                sb.Append('&');
                sb.Append(HttpUtility.UrlEncode(SearchQueryConstants.Text));
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(query.Text));
            }

            var url = searchQuery.BaseUrl;
            if (url == null)
            {
                if (query.PageType == PageTemplateNameConstants.SearchResult || query.PageType == PageTemplateNameConstants.Brand || query.PageType == PageTemplateNameConstants.ProductList)
                {
                    url = _urlService.Value.GetUrl(query.PageSystemId.MapTo<Page>());
                }
                else
                {
                    url = (query.CategorySystemId.GetValueOrDefault() != Guid.Empty)
                        ? _urlService.Value.GetUrl(query.CategorySystemId.Value.MapTo<Category>())
                        : query.WebsiteSystemId.MapTo<WebsiteModel>().Fields.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.SearchResultPage)?.MapTo<LinkModel>()?.Href;
                }
                searchQuery.BaseUrl = url;
            }

            if (sb.Length > 1)
            {
                sb.Remove(0, 1);
                return url + "?" + sb;
            }

            return url;
        }

        public static string GetUrlCategory(this SearchQuery searchQuery, Guid? id, bool selected)
        {
            return searchQuery.GetUrl(query =>
            {
                if (id == null)
                {
                    query.Category.Clear();
                }
                else
                {
                    if (selected && query.Category.Contains(id.Value))
                    {
                        query.Category.Remove(id.Value);
                    }
                    else if (!query.Category.Contains(id.Value))
                    {
                        query.Category.Add(id.Value);
                    }
                }
                query.Type = SearchType.Products;
            });
        }

        public static string GetUrlNews(this SearchQuery searchQuery, Tuple<DateTime, DateTime> value, bool selected)
        {
            return searchQuery.GetUrl(query =>
            {
                query.NewsDate = selected ? null : value;
                query.Type = SearchType.Products;
            });
        }

        public static string GetUrlPrice(this SearchQuery searchQuery)
        {
            return searchQuery.GetUrl(query =>
            {
                query.Type = SearchType.Products;
                query.PriceRanges.Clear();
            });
        }

        public static string GetUrlPrice(this SearchQuery searchQuery, (int, int) priceRange, bool selected)
        {
            return searchQuery.GetUrl(query =>
            {
                if (selected && query.PriceRanges.Contains(priceRange))
                {
                    query.PriceRanges.Remove(priceRange);
                }
                else if (!selected && !query.PriceRanges.Contains(priceRange))
                {
                    query.PriceRanges.Add(priceRange);
                }
                query.Type = SearchType.Products;
            });
        }

        public static string GetUrlSort(this SearchQuery searchQuery, string sortBy, SortDirection sortDirection)
        {
            return searchQuery.GetUrl(query =>
            {
                query.SortBy = sortBy;
                query.SortDirection = sortDirection;
            });
        }

        public static string GetUrlTag(this SearchQuery searchQuery, string tagName, string tagValue, bool selected)
        {
            return searchQuery.GetUrl(query =>
            {
                var tags = query.Tags;
                if (selected && tags.ContainsKey(tagName))
                {
                    if (string.IsNullOrEmpty(tagValue))
                    {
                        tags.Remove(tagName);
                    }
                    else
                    {
                        tags[tagName].Remove(tagValue);
                    }
                }
                else if (!string.IsNullOrEmpty(tagValue))
                {
                    if (!tags.ContainsKey(tagName))
                    {
                        tags.Add(tagName, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
                    }

                    tags[tagName].Add(tagValue);
                }
                else if (tags.ContainsKey(tagName))
                {
                    tags.Remove(tagName);
                }
                query.Type = SearchType.Products;
            });
        }

        public static bool IsSortedBy(this SearchQuery searchQuery, string field, SortDirection direction)
        {
            if (direction == SortDirection.Descending)
            {
                return SortDirection.Descending == searchQuery.SortDirection && field.Equals(searchQuery.SortBy, StringComparison.OrdinalIgnoreCase);
            }

            return SortDirection.Ascending == searchQuery.SortDirection && field.Equals(searchQuery.SortBy, StringComparison.OrdinalIgnoreCase);
        }
    }
}
