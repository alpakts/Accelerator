using Litium.Events;
using Litium.Sales;
using Litium.Sales.Events;
using Litium.Search.Indexing;

namespace Litium.Accelerator.Search.Indexing.PurchaseHistories
{
    public class PurchaseHistoryEventListener : IIndexQueueHandlerRegistration
    {
        private readonly IndexQueueService _indexQueueService;

        public PurchaseHistoryEventListener(
            EventBroker eventBroker,
            IndexQueueService indexQueueService)
        {
            _indexQueueService = indexQueueService;
            eventBroker.Subscribe<OrderCreated>(x => {
                if (x.Item is SalesOrder salesOrder)
                {
                    OnUpdate(salesOrder);
                }
            });
        }

        private void OnUpdate(SalesOrder order)
        {
            _indexQueueService.Enqueue(new IndexQueueItem<PurchaseHistoryDocument>(order.SystemId));
        }
    }
}
