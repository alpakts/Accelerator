using System;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Product
{
    public class CategoryItemViewModel : IViewModel
    {
        public Guid SystemId { get; set; }
        public string Name { get; set; }
        public List<string> Images { get; set; }
        public string Description { get; set; }
    }
}
