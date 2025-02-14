using Litium.Accelerator.Builders.News;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.News
{
    public class NewsController : ControllerBase
    {
        private readonly NewsViewModelBuilder _newsViewModelBuilder;
        private readonly NewsListViewModelBuilder _newsListViewModelBuilder;

        public NewsController(NewsViewModelBuilder newsViewModelBuilder, NewsListViewModelBuilder newsListViewModelBuilder)
        {
            _newsViewModelBuilder = newsViewModelBuilder;
            _newsListViewModelBuilder = newsListViewModelBuilder;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = _newsViewModelBuilder.Build();
            return View(model);
        }

        [HttpGet]
        public ActionResult List(int page = 1)
        {
            var model = _newsListViewModelBuilder.Build(page);
            return View(model);
        }
    }
}