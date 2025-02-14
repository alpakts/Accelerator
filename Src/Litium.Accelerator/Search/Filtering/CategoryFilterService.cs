using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Litium.Accelerator.Constants;
using Litium.Common.Events;
using Litium.Events;
using Litium.FieldFramework.Events;
using Litium.Products;
using Litium.Products.Events;
using Litium.Runtime;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Search.Filtering
{
    [Autostart]
    [Service(ServiceType = typeof(CategoryFilterService), Lifetime = DependencyLifetime.Singleton)]
    public class CategoryFilterService : IAsyncAutostart
    {
        private readonly ConcurrentDictionary<Guid, (string[], bool)> _cache = new ConcurrentDictionary<Guid, (string[], bool)>();
        private readonly EventBroker _eventBroker;
        private readonly FilterService _filterService;

        public CategoryFilterService(EventBroker eventBroker, FilterService filterService)
        {
            _eventBroker = eventBroker;
            _filterService = filterService;
        }

        public (string[] filters, bool isOrdered) GetFilters(Guid categorySystemId)
        {
            return _cache.GetOrAdd(categorySystemId, key =>
            {
                var filterFields = _filterService.GetProductFilteringFields();
                var customFilters = GetCustomFilterValues(key);

                if (customFilters != null)
                {
                    return (customFilters.Intersect(filterFields, StringComparer.OrdinalIgnoreCase).ToArray(), true);
                }

                return (filterFields?.ToArray() ?? Array.Empty<string>(), false);
            });
        }

        private static IList<string> GetCustomFilterValues(Guid categorySystemId)
        {
            var category = categorySystemId.MapTo<Category>();
            while (category != null)
            {
                var customFilterOptions = category.Fields.GetValue<IList<string>>(NavigationConstants.AcceleratorFilterFieldDefinitionName);
                if (customFilterOptions != null)
                {
                    return customFilterOptions;
                }

                if (category.ParentCategorySystemId == Guid.Empty)
                {
                    break;
                }

                category = category.ParentCategorySystemId.MapTo<Category>();
            }

            return null;
        }

        ValueTask IAsyncAutostart.StartAsync(CancellationToken cancellationToken)
        {
            _eventBroker.Subscribe<PurgeFilterCache>(_ => _cache.Clear());
            _eventBroker.Subscribe<CategoryCreated>(_ => ClearCache());
            _eventBroker.Subscribe<CategoryDeleted>(_ => ClearCache());
            _eventBroker.Subscribe<CategoryUpdated>(_ => ClearCache());
            _eventBroker.Subscribe<FieldDefinitionCreated>(_ => ClearCache());
            _eventBroker.Subscribe<FieldDefinitionDeleted>(_ => ClearCache());
            _eventBroker.Subscribe<FieldDefinitionUpdated>(_ => ClearCache());
            _eventBroker.Subscribe<SettingChanged>(async x =>
            {
                if (string.Equals(FilterService._key, x.Key, StringComparison.OrdinalIgnoreCase) && x.PersonSystemId == Guid.Empty)
                {
                    await ClearCache();
                }
            });

            Task ClearCache()
            {
                _cache.Clear();
                return _eventBroker.PublishAsync(EventScope.Remote, new PurgeFilterCache());
            }
            return ValueTask.CompletedTask;
        }

        private class PurgeFilterCache : IMessage { }
    }
}
