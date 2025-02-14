using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.News;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.News
{
    [Service(ServiceType = typeof(NewsViewModelBuilder))]
    public class NewsViewModelBuilder
    {
        private readonly RequestModelAccessor _requestModelAccessor;

        public NewsViewModelBuilder(RequestModelAccessor requestModelAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
        }

        public virtual NewsViewModel Build()
            => Build(_requestModelAccessor.RequestModel.CurrentPageModel);

        public virtual NewsViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<NewsViewModel>();
            return model;
        }
    }
}
