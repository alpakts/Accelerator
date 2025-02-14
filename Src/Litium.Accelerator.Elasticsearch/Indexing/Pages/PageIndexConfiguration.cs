using System.Globalization;
using System.Threading.Tasks;
using Litium.Data;
using Litium.Search;
using Litium.Search.Indexing;
using Litium.Websites.Queryable;
using Microsoft.Extensions.Localization;

namespace Litium.Accelerator.Search.Indexing.Pages
{
    public class PageIndexConfiguration : MultilingualIndexConfigurationBase<PageDocument>
    {
        private readonly DataService _dataService;
        private readonly IStringLocalizer _localizer;

        public PageIndexConfiguration(
            IndexConfigurationDependencies dependencies,
            DataService dataService,
            IStringLocalizer<IndexConfigurationActionResult> localizer)
            : base(dependencies)
        {
            _dataService = dataService;
            _localizer = localizer;
        }

        protected override void Configure(CultureInfo cultureInfo, IndexConfigurationBuilder<PageDocument> builder)
        {
            builder.Map(x => x.Properties(p => p
                .Text(t => t
                    .Name(x => x.Name)
                    .Similarity("BM25")
                    .Boost(10)
                    .Analyzer(cultureInfo.AsAnalyzer())
                )
                .Text(t => t
                    .Name(x => x.Content)
                )
            ));
        }

        protected override Task<IndexConfigurationActionResult> QueueIndexRebuildAsync(IndexQueueService indexQueueService)
        {
            using (var query = _dataService.CreateQuery<Websites.Page>())
            {
                query.Filter(f => f.Status(Common.ContentStatus.Published));

                foreach (var systemId in query.ToSystemIdList())
                {
                    indexQueueService.Enqueue(new IndexQueueItem<PageDocument>(systemId));
                }
            }

            return Task.FromResult(new IndexConfigurationActionResult
            {
                Message = _localizer.GetString("index.pages.queued")
            });
        }
    }
}
