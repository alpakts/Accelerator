using AutoMapper;
using Litium.Runtime.AutoMapper;
using System.Collections.Generic;
using System.Linq;

namespace Litium.Accelerator.ViewModels.Search
{
    public class GroupFilter : IAutoMapperConfiguration
    {
        public Dictionary<string, string> Attributes { get; set; }
        public bool IsSelected { get; set; }
        public bool SingleSelect { get; set; }
        public IList<FilterItem> Links { get; set; }
        public string Name { get; set; }

        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<GroupFilter, FacetGroupFilter>()
               .ForMember(x => x.Label, m => m.MapFrom(c => c.Name))
               .ForMember(x => x.Id, m => m.MapFrom((c, _) => c.Attributes.TryGetValue("value", out string value) ? value : ""))
               .ForMember(x => x.SelectedOptions, m => m.MapFrom((c, _) =>
               {
                   if (c.Links == null)
                       return new string[] { };

                   var childsSelected = c.Links.Where(l => l.IsSelected).ToList();
                   if (childsSelected?.Count > 0)
                   {
                        return childsSelected.Select(l => l.Attributes.TryGetValue("value", out string value) ? value : "");
                   }
                   return new string[] { };
               }))
               .ForMember(x => x.Options, m => m.MapFrom(c => c.Links));
        }
    }
}
