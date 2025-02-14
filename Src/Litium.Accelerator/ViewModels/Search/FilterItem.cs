using AutoMapper;
using Litium.Runtime.AutoMapper;
using System.Collections.Generic;

namespace Litium.Accelerator.ViewModels.Search
{
    public class FilterItem : IAutoMapperConfiguration
    {
        public Dictionary<string, string> Attributes { get; set; }
        public int? Count { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<FilterItem, FacetFilter>()
               .ForMember(x => x.Label, m => m.MapFrom(c => c.Name))
               .ForMember(x => x.Quantity, m => m.MapFrom(c => c.Count))
               .ForMember(x => x.Id, m => m.MapFrom((c, _) => c.Attributes.TryGetValue("value", out string value) ? value : ""));
        }
    }
}
