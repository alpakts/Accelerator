using Litium.Accelerator.Routing;
using Litium.Accelerator.Search;

namespace Litium.Accelerator.Searchers
{
    public class BrandSearcher : PageSearcher
    {
        public override int SortOrder => 300;
        public override string ModelKey => "Brands";
        public override bool? OnlyBrands => true;
        public override int PageSize => 5;

        public BrandSearcher(PageSearchService pageSearchService, RequestModelAccessor requestModelAccessor)
            : base(pageSearchService, requestModelAccessor) { }
    }
}
