using AutoMapper;
using JetBrains.Annotations;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using Litium.Web.Models.Websites;
using System.Collections.Generic;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.LandingPage
{
    public class LandingPageViewModel : IAutoMapperConfiguration, IViewModel
    {
        public Dictionary<string, List<BlockModel>> Blocks { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, LandingPageViewModel>();
        }
    }
}
