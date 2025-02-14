using System.Globalization;
using System.Threading.Tasks;
using Litium.Data;
using Litium.Products;
using Litium.Search;
using Litium.Search.Indexing;
using Microsoft.Extensions.Localization;
using Nest;

namespace Litium.Accelerator.Search.Indexing.Products
{
    public class ProductIndexConfiguration : MultilingualIndexConfigurationBase<ProductDocument>
    {
        private readonly DataService _dataService;
        private readonly IStringLocalizer _localizer;

        public ProductIndexConfiguration(
            IndexConfigurationDependencies dependencies,
            DataService dataService,
            IStringLocalizer<IndexConfigurationActionResult> localizer)
            : base(dependencies)
        {
            _dataService = dataService;
            _localizer = localizer;
        }

        protected override void Configure(CultureInfo cultureInfo, IndexConfigurationBuilder<ProductDocument> builder)
        {
            builder
            .Setting(UpdatableIndexSettings.MaxNGramDiff, 3) // default is 1
            .Analysis(a => a
                .Analyzers(az => az
                    .Custom("custom_ngram_analyzer", c => c
                        .Tokenizer("custom_ngram_tokenizer")
                        .Filters(new string[] { "lowercase" })))
                .Tokenizers(t => t
                    .EdgeNGram("custom_ngram_tokenizer", ng => ng
                        .MinGram(2) // will throw an error if the difference
                        .MaxGram(5) // is bigger than MaxNGramDiff above
                        .TokenChars(new TokenChar[] { TokenChar.Letter, TokenChar.Digit })
                    )
                )
            )
            .Map(m => m
                .Properties(p => p
                    .Text(k => k
                        .Name(n => n.Name)
                        .Fields(ff => ff
                            .Keyword(tk => tk
                                .Name("keyword")
                                .IgnoreAbove(256))
                            .Text(tt => tt
                                .Name("ngram")
                                .Analyzer("custom_ngram_analyzer")
                            )
                        )
                    )
                )
            );

            base.Configure(cultureInfo, builder);
        }

        protected override Task<IndexConfigurationActionResult> QueueIndexRebuildAsync(IndexQueueService indexQueueService)
        {
            using (var query = _dataService.CreateQuery<BaseProduct>())
            {
                foreach (var systemId in query.ToSystemIdList())
                {
                    indexQueueService.Enqueue(new IndexQueueItem<ProductDocument>(systemId));
                }
            }

            return Task.FromResult(new IndexConfigurationActionResult
            {
                Message = _localizer.GetString("index.products.queued")
            });
        }
    }
}
