using System;
using System.Threading.Tasks;
using Litium.Data;
using Litium.Sales;
using Litium.Sales.Queryable;
using Litium.Search;
using Litium.Search.Indexing;
using Microsoft.Extensions.Localization;

namespace Litium.Accelerator.Search.Indexing.PurchaseHistories
{
    public class PurchaseHistoryIndexConfiguration : IndexConfigurationBase<PurchaseHistoryDocument>
    {
        public const string Rebuild = nameof(Rebuild);
        private readonly DataService _dataService;
        private readonly IStringLocalizer _localizer;

        public PurchaseHistoryIndexConfiguration(
            IndexConfigurationDependencies dependencies,
            DataService dataService,
            IStringLocalizer<IndexConfigurationActionResult> localizer)
            : base(dependencies)
        {
            _dataService = dataService;
            _localizer = localizer;
        }

        protected override Task<IndexConfigurationActionResult> QueueIndexRebuildAsync(IndexQueueService indexQueueService)
        {
            using (var query = _dataService.CreateQuery<Order>())
            {
                foreach (var systemId in query
                    .Filter(x => x.InDateRange(fromDate: DateTimeOffset.UtcNow.AddMonths(-6), toDate: default))
                    .ToSystemIdList())
                {
                    indexQueueService.Enqueue(new IndexQueueItem<PurchaseHistoryDocument>(systemId));
                }
            }

            indexQueueService.Enqueue(new IndexQueueItem<PurchaseHistoryDocument>(Guid.Empty)
            {
                Action = IndexAction.Delete,
                AdditionalInfo =
                {
                    [Rebuild] = true
                }
            });

            return Task.FromResult(new IndexConfigurationActionResult
            {
                Message = _localizer.GetString("index.purchasehistories.queued")
            });
        }
    }
}
