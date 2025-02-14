using System.Collections.Generic;
using AutoMapper;
using Litium.Accelerator.Builders;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class SubNavigationLinkModel : IViewModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsSelected { get; set; }
        public IList<SubNavigationLinkModel> Links { get; set; } = new List<SubNavigationLinkModel>();

        internal class Mapper : IAutoMapperConfiguration
        {
            void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
            {
                cfg.CreateMap<FilterItem, SubNavigationLinkModel>();
            }
        }
    }
}