using System.Collections.Generic;

namespace Litium.Accelerator.Administration.Extensions.ViewModel
{
    public class FilteringModel
    {
        public List<string> Items { get; set; }
        public List<Item> Filters { get; set; }

        public string Message { get; set; }

        public class Item
        {
            public string Title { get; set; }
            public string FieldId { get; set; }
            public string GroupName { get; set; }
        }
    }
}
