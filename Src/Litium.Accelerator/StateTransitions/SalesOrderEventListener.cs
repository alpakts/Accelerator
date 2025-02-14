using Litium.Events;
using Litium.Runtime;
using System.Threading.Tasks;
using System.Threading;
using Litium.Accelerator.Services;
using Litium.Sales.Events;
using Litium.Accelerator.Mailing;

namespace Litium.Accelerator.StateTransitions
{
    [Autostart]
    public class SalesOrderEventListener : IAsyncAutostart
    {
        private readonly EventBroker _eventBroker;
        private readonly MailService _mailService;
        private readonly StockService _stockService;

        public SalesOrderEventListener(
            EventBroker eventBroker,
            MailService mailService,
            StockService stockService)
        {
            _eventBroker = eventBroker;
            _mailService = mailService;
            _stockService = stockService;
        }

        ValueTask IAsyncAutostart.StartAsync(CancellationToken cancellationToken)
        {
            _eventBroker.Subscribe<SalesOrderConfirmed>(x => _stockService.ReduceStock(x.Item));
            _eventBroker.Subscribe<SalesOrderConfirmed>(x =>
                _mailService.SendEmail(new OrderConfirmationEmail(x.Item.ChannelSystemId.Value, x.SystemId, x.Item.CustomerInfo.Email), false));

            return ValueTask.CompletedTask;
        }
    }
}
