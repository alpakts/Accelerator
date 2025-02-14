using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Product
{
    public class FilterProductViewModel : IViewModel
    {
        public IViewModel ViewData { get; set; }
        public int TotalCount { get; set; }
    }
}
