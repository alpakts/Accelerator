using System.Threading;
using System.Threading.Tasks;
using Litium.Scheduler;

namespace Litium.Accelerator.Search.Indexing.PurchaseHistories
{
    [CronScheduler("Litium.Accelerator.PurchaseHistory", ExecutionRestriction = ScheduleCronJobExecutionRestriction.DisallowConcurrentDistributedExecution)]
    public class PurchaseHistoryScheduler : ICronScheduleJob
    {
        private readonly PurchaseHistoryService _purchaseHistoryService;

        public PurchaseHistoryScheduler(
            PurchaseHistoryService purchaseHistoryService)
        {
            _purchaseHistoryService = purchaseHistoryService;
        }

        public async ValueTask ExecuteAsync(object parameter, CancellationToken cancellationToken = default)
        {
            await _purchaseHistoryService.RebuildAsync();
        }
    }
}
