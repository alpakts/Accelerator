using System;
using System.Linq;
using Litium.Accelerator.ViewModels.Framework;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Litium.Accelerator.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Litium.Accelerator.Builders.Framework
{
    [Service(ServiceType = typeof(BodyViewModelBuilder), Lifetime = DependencyLifetime.Scoped)]
    public class BodyViewModelBuilder
    {
        private static readonly object _lock = new object();
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly TrackingScriptService _trackingScriptService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly IFileProvider _fileProvider;

        public BodyViewModelBuilder(
            TrackingScriptService trackingScriptService,
            IWebHostEnvironment hostingEnvironment,
            RequestModelAccessor requestModelAccessor,
            IMemoryCache memoryCache)
        {
            _trackingScriptService = trackingScriptService;
            _requestModelAccessor = requestModelAccessor;
            _hostingEnvironment = hostingEnvironment;
            _fileProvider = hostingEnvironment.WebRootFileProvider;
            _memoryCache = memoryCache;
        }

        public BodyEndViewModel BuildBodyEnd()
        {
            var pageModel = _requestModelAccessor.RequestModel.CurrentPageModel;
            string fileName = GetFileName("es6");
            string fileNameEs5 = GetFileName("es5");

            var viewModel = new BodyEndViewModel
            {
                FileName = fileName,
                FileNameEs5 = fileNameEs5,
                TrackingScripts = _trackingScriptService.GetBodyEndScript(pageModel.Page)
            };

            return viewModel;

            string GetFileName(string folder)
            {
                var cacheKey = "LitiumAppJs" + folder;
                string fileName = _memoryCache.Get(cacheKey) as string;
                if (fileName == null)
                {
                    lock (_lock)
                    {
                        fileName = _memoryCache.Get(cacheKey) as string;
                        if (fileName == null)
                        {
                            var fileInfos = _fileProvider.GetDirectoryContents(System.IO.Path.Combine("ui", folder)).OfType<IFileInfo>();
                            var file = fileInfos
                                .FirstOrDefault(x =>
                                !x.IsDirectory
                                && x.Name.StartsWith("app.", StringComparison.OrdinalIgnoreCase)
                                && x.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase));

                            if (file == null)
                            {
                                throw new Exception("You need to compile the client library.");
                            }

                            fileName = $"/ui/{folder}/{file.Name}";

                            if (_hostingEnvironment.IsDevelopment())
                            {
                                var changeToken = _fileProvider.Watch($"/ui/{folder}");
                                _memoryCache.Set(cacheKey, fileName, changeToken);
                            }
                            else
                            {
                                _memoryCache.Set(cacheKey, fileName);
                            }
                        }
                    }
                }
                return fileName;
            }
        }

        public BodyStartViewModel BuildBodyStart()
        {
            var pageModel = _requestModelAccessor.RequestModel.CurrentPageModel;
            var viewModel = new BodyStartViewModel
            {
                TrackingScripts = _trackingScriptService.GetBodyStartScripts(pageModel.Page)
            };
            return viewModel;
        }
    }
}
