using System.Collections.Generic;
using System.Linq;
using Litium.Security;
using Litium.Web;
using Litium.Web.Administration;
using Litium.Web.Administration.Common.ViewModels;
using Litium.Web.Administration.Settings;
using Microsoft.Extensions.Configuration;

namespace Litium.Accelerator.Administration.Extensions
{
    internal class SettingsMenuProvider : ISettingsMenuProvider
    {
        private readonly AuthorizationService _authorizationService;
        private readonly IConfiguration _configuration;

        public SettingsMenuProvider(
            AuthorizationService authorizationService,
            IConfiguration configuration)
        {
            _authorizationService = authorizationService;
            _configuration = configuration;
        }

        public IEnumerable<MenuModel> GetMenuList()
        {
            if (_authorizationService.HasOperation(Operations.Function.SystemSettings))
            {
                var elasticsearchConnectionString = _configuration["Litium:Elasticsearch:ConnectionString"];
                var elasticsearchConfigured = !string.IsNullOrWhiteSpace(elasticsearchConnectionString);

                var index = SortIndex + 1;
                var key = "accelerator.title";
                var menu = new MenuModel(key)
                {
                    Label = key.AsAngularResourceString(),
                    SortIndex = index++,
                    Children = new[]
                    {
                        ("/Litium/UI/settings/extensions/AcceleratorExtensions/grouping", "accelerator.setting.indexing", true),
                        ("/Litium/UI/settings/extensions/AcceleratorExtensions/filtering", "accelerator.setting.filtering", true),
                        ("/Litium/UI/settings/extensions/AcceleratorExtensions/searchindexing", "accelerator.setting.search", elasticsearchConfigured),
                    }
                    .Where(x => x.Item3)
                    .Select(x => new MenuModel($"{key}_{x.Item2}")
                    {
                        Url = x.Item1,
                        Label = x.Item2.AsAngularResourceString(),
                        ParentId = key
                    })
                    .ToList()
                };
                foreach (var item in menu.Children.OrderBy(x => x.Label))
                {
                    item.SortIndex = index++;
                }
                return new[] { menu };
            }

            return null;
        }

        public int SortIndex { get; } = 5000;
    }
}
