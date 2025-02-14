using System;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels
{
    public abstract class PageViewModel : IViewModel
    {
        public Guid SystemId { get; set; }
    }
}
