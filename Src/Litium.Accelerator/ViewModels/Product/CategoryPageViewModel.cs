using System.Collections.Generic;
using Litium.Web.Models.Blocks;
using Newtonsoft.Json;

namespace Litium.Accelerator.ViewModels.Product
{
    public class CategoryPageViewModel : PageViewModel
    {
        [JsonProperty(Order = 1)]
        public string Name { get; set; }
        [JsonProperty(Order = 2)]
        public string Description { get; set; }
        [JsonProperty(Order = 3)]
        public List<string> Images { get; set; }
        [JsonProperty(Order = 4)]
        public List<ProductItemViewModel> Products { get; set; }

        public Dictionary<string, List<BlockModel>> Blocks { get; set; }
        public PaginationViewModel Pagination { get; set; }
        public bool ShowRegularHeader { get; set; }
        public bool ShowFilterHeader { get; set; }
        public bool ShowSections { get; set; }
    }
}
