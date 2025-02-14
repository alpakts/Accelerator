using System;
using Litium.Accelerator.ViewModels.Order;

namespace Litium.Accelerator.Services
{
    public class OrderHistoryViewModelService : ViewModelService<OrderHistoryViewModel>
    {
        public void SaveOrder(string orderState, Guid id)
        {
            // This will be implemented with return management
        }

        public void CancelOrder(Guid id)
        {
            // This will be implemented with return management
        }
    }
}
