using System.Globalization;
using System.Threading.Tasks;
using Litium.Data;
using Litium.Products;
using Litium.Search;
using Litium.Search.Indexing;
using Microsoft.Extensions.Localization;
using Nest;

namespace Litium.Accelerator.Search.Indexing.Categories
{
    public class CategoryIndexConfiguration : MultilingualIndexConfigurationBase<CategoryDocument>
    {
        private readonly DataService _dataService;
        private readonly IStringLocalizer _localizer;

        public CategoryIndexConfiguration(
            IndexConfigurationDependencies dependencies,
            DataService dataService,
            IStringLocalizer<IndexConfigurationActionResult> localizer)
            : base(dependencies)
        {
            _dataService = dataService;
            _localizer = localizer;
        }

        protected override Task<IndexConfigurationActionResult> QueueIndexRebuildAsync(IndexQueueService indexQueueService)
        {
            using (var query = _dataService.CreateQuery<Category>())
            {
                foreach (var systemId in query.ToSystemIdList())
                {
                    indexQueueService.Enqueue(new IndexQueueItem<CategoryDocument>(systemId));
                }
            }

            return Task.FromResult(new IndexConfigurationActionResult
            {
                Message = _localizer.GetString("index.categories.queued")
            });
        }
    }
}
