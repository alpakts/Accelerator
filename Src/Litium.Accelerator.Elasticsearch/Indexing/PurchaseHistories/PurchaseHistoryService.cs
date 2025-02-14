using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litium.Caching;
using Litium.Common;
using Litium.Events;
using Litium.Products;
using Litium.Runtime.DependencyInjection;
using Litium.Search;
using Nest;
using static Litium.Accelerator.Search.Indexing.Products.ProductEventListener;

namespace Litium.Accelerator.Search.Indexing.PurchaseHistories
{
    [Service(ServiceType = typeof(PurchaseHistoryService), Lifetime = DependencyLifetime.Singleton)]
    public abstract class PurchaseHistoryService
    {
        /// <summary>
        /// Try to get purchase history information out for the <see cref="Variant"/>.
        /// </summary>
        /// <remarks>
        /// The result is an <see cref="IDictionary{TKey, TValue}"/> where key is the system id
        /// for the <see cref="Globalization.Channel"/> and the value is quantity.
        /// </remarks>
        /// <param name="id">The variant id.</param>
        /// <param name="item">The result item.</param>
        /// <returns><c>true</c> for success result; otherwise <c>false</c>.</returns>
        public abstract bool TryGet(string id, out IDictionary<Guid, decimal> item);

        /// <summary>
        /// Rebuild the in-memory information of purchase history.
        /// </summary>
        /// <returns>The awaitable task.</returns>
        public abstract Task RebuildAsync();
    }

    public class PurchaseHistoryServiceImpl : PurchaseHistoryService
    {
        private readonly string _cacheKey = nameof(PurchaseHistoryService);
        private readonly EventBroker _eventBroker;
        private readonly IElasticClient _elasticClient;
        private readonly IndexNamingService _indexNameConfigurationService;
        private readonly SearchClientService _searchClientService;
        private readonly KeyLookupService _keyLookupService;
        private readonly DistributedMemoryCacheService _distributedMemoryCacheService;
        private bool _supportPointInTime = true;

        public PurchaseHistoryServiceImpl(
            EventBroker eventBroker,
            IElasticClient elasticClient,
            IndexNamingService indexNameConfigurationService,
            SearchClientService searchClientService,
            KeyLookupService keyLookupService,
            DistributedMemoryCacheService distributedMemoryCacheService)
        {
            _eventBroker = eventBroker;
            _elasticClient = elasticClient;
            _indexNameConfigurationService = indexNameConfigurationService;
            _searchClientService = searchClientService;
            _keyLookupService = keyLookupService;
            _distributedMemoryCacheService = distributedMemoryCacheService;
        }

        public override async Task RebuildAsync()
        {
            if (!_searchClientService.IsConfigured)
            {
                return;
            }

            var newCache = new InternalCache();
            await foreach (var item in GetDocuments())
            {
                foreach (var row in item.Rows)
                {
                    var cacheItem = newCache.GetOrAdd(row.ArticleNumber, _ => new());
                    cacheItem.AddOrUpdate(item.ChannelSystemId, _ => row.Quantity, (_, current) => current + row.Quantity);
                }
            }
            var oldCache = GetCache();
            _distributedMemoryCacheService.Set(_cacheKey, newCache);

            foreach (var newItem in newCache)
            {
                if (!_keyLookupService.TryGetSystemId<Variant>(newItem.Key, out var systemId))
                {
                    continue;
                }

                if (oldCache.TryRemove(newItem.Key, out var oldItem))
                {
                    if (newItem.Value.Count != oldItem.Count)
                    {
                        await TriggerEvent();
                        continue;
                    }

                    var allEqual = true;
                    foreach (var channelData in newItem.Value)
                    {
                        if (oldItem.TryGetValue(channelData.Key, out var existingValue)
                            && existingValue == channelData.Value)
                        {
                            continue;
                        }

                        allEqual = false;
                        break;
                    }

                    if (!allEqual)
                    {
                        await TriggerEvent();
                    }
                }
                else
                {
                    await TriggerEvent();
                }

                Task TriggerEvent()
                {
                    return _eventBroker.PublishAsync(new ReindexVariant
                    {
                        VariantSystemId = systemId
                    });
                }
            }

            foreach (var item in oldCache)
            {
                if (_keyLookupService.TryGetSystemId<Variant>(item.Key, out var systemId))
                {
                    await _eventBroker.PublishAsync(new ReindexVariant
                    {
                        VariantSystemId = systemId
                    });
                }
            }
        }

        public override bool TryGet(string id, out IDictionary<Guid, decimal> item)
        {
            if (GetCache().TryGetValue(id, out var cacheItem))
            {
                item = cacheItem;
                return true;
            }

            item = default;
            return false;
        }

        private async IAsyncEnumerable<PurchaseHistoryDocument> GetDocuments()
        {
            var keepAlive = new Time(TimeSpan.FromMinutes(1));
            var index = _indexNameConfigurationService.GetQueryIndexName<PurchaseHistoryDocument>();
            await _elasticClient.DeleteByQueryAsync<PurchaseHistoryDocument>(x => x.Index(index)
                .Query(q =>
                    q.DateRange(t => t.Field(f => f.OrderDate).LessThan(DateTimeOffset.Now.UtcDateTime.AddMonths(-6)))
                )
            );
            if (_supportPointInTime)
            {
                var pit = await _elasticClient.OpenPointInTimeAsync(index, p => p.KeepAlive(keepAlive.ToString()));
                if (pit.IsValid)
                {
                    await foreach (var item in WithPointInTime(pit))
                    {
                        yield return item;
                    }
                    yield break;
                }
                else
                {
                    _supportPointInTime = false;
                }
            }

            await foreach (var item in WithScroll())
            {
                yield return item;
            }

            async IAsyncEnumerable<PurchaseHistoryDocument> WithScroll()
            {
                var response = await _searchClientService.SearchAsync<PurchaseHistoryDocument>(selector => selector
                    .Scroll(keepAlive)
                    .Size(5000)
                    .Query(q => !q.Term(t => t.Field(f => f.ChannelSystemId).Value(Guid.Empty))));

                while (response.Hits.Count > 0)
                {
                    foreach (var item in response.Hits)
                    {
                        yield return item.Source;
                    }
                    response = await _elasticClient.ScrollAsync<PurchaseHistoryDocument>(keepAlive, response.ScrollId);
                }
            }

            async IAsyncEnumerable<PurchaseHistoryDocument> WithPointInTime(OpenPointInTimeResponse pit)
            {
                var response = await _searchClientService.SearchAsync<PurchaseHistoryDocument>(selector => selector
                    .PointInTime(pit.Id, p => p.KeepAlive(keepAlive))
                    .TrackTotalHits(false)
                    .Size(5000)
                    .Query(q => !q.Term(t => t.Field(f => f.ChannelSystemId).Value(Guid.Empty)))
                    .Sort(s => s.Ascending(f => f.SystemId)));

                IHit<PurchaseHistoryDocument> lastHit = default;
                foreach (var item in response.Hits)
                {
                    lastHit = item;
                    yield return item.Source;
                }

                if (lastHit is null)
                {
                    yield break;
                }

                while (response.Hits.Count > 0)
                {
                    response = await _searchClientService.SearchAsync<PurchaseHistoryDocument>(selector => selector
                        .PointInTime(response.PointInTimeId, p => p.KeepAlive(keepAlive))
                        .SearchAfter(lastHit.Sorts)
                        .TrackTotalHits(false)
                        .Size(5000)
                        .Query(q => !q.Term(t => t.Field(f => f.ChannelSystemId).Value(Guid.Empty)))
                        .Sort(s => s.Ascending(f => f.SystemId)));

                    foreach (var item in response.Hits)
                    {
                        lastHit = item;
                        yield return item.Source;
                    }
                }

                var closePitResponse = await _elasticClient.ClosePointInTimeAsync(p => p.Id(response.PointInTimeId));
            }
        }

        private InternalCache GetCache()
        {
            if (_distributedMemoryCacheService.TryGet<InternalCache>(_cacheKey, out var cache))
            {
                return cache;
            }

            cache = new();
            _distributedMemoryCacheService.Set(_cacheKey, cache);
            return cache;
        }

        private class InternalCache : ConcurrentDictionary<string, ConcurrentDictionary<Guid, decimal>>
        {
            public InternalCache()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }
        }
    }
}
