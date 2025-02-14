using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Sales;
using Litium.Scheduler;
using Litium.Search;
using Litium.Search.Indexing;
using static Litium.Accelerator.Search.PurchaseHistoryDocument;

namespace Litium.Accelerator.Search.Indexing.PurchaseHistories
{
    public class PurchaseHistoryIndexDocumentBuilder : IndexDocumentBuilderBase<PurchaseHistoryDocument>
    {
        private readonly OrderService _orderService;
        private readonly SchedulerService _schedulerService;

        public PurchaseHistoryIndexDocumentBuilder(
            IndexDocumentBuilderDependencies dependencies,
            OrderService orderService,
            SchedulerService schedulerService)
            : base(dependencies)
        {
            _orderService = orderService;
            _schedulerService = schedulerService;
        }

        public override IEnumerable<IDocument> BuildIndexDocuments(IndexQueueItem item)
        {
            var order = _orderService.Get<SalesOrder>(item.SystemId);
            if (order is null)
            {
                yield return RemoveByFieldDocument.Create<PurchaseHistoryDocument, Guid>(x => x.SystemId, item.SystemId);
                yield break;
            }

            var document = new PurchaseHistoryDocument
            {
                SystemId = item.SystemId,
                OrderDate = order.OrderDate,
                ChannelSystemId = order.ChannelSystemId.GetValueOrDefault(),
            };

            document.Rows.AddRange(order.Rows.Where(x => x.OrderRowType == OrderRowType.Product).Select(x => new RowItem
            {
                ArticleNumber = x.ArticleNumber,
                Quantity = x.Quantity,
            }));

            document.ArticleNumbers.AddRange(document.Rows.Select(x => x.ArticleNumber));
            if (document.ArticleNumbers.Count == 0)
            {
                yield return new RemoveDocument(document);
                yield break;
            }

            yield return document;
        }

        public override IEnumerable<IDocument> BuildRemoveIndexDocuments(IndexQueueItem item)
        {
            if (item.AdditionalInfo?.TryGetValue(PurchaseHistoryIndexConfiguration.Rebuild, out var o) == true
                && o is bool b
                && b)
            {
                _schedulerService.ScheduleJob<PurchaseHistoryService>(x => x.RebuildAsync(), new()
                {
                    ExecuteAt = DateTimeOffset.UtcNow.AddSeconds(5),
                });
                yield break;
            }
            yield return RemoveByFieldDocument.Create<PurchaseHistoryDocument, Guid>(x => x.SystemId, item.SystemId);
        }
    }
}
