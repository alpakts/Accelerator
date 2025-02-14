using System.Collections.Generic;
using AutoMapper;
using Litium.Accelerator.Builders;
using Litium.Accelerator.ViewModels.Search;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class ContentLinkModel : IViewModel
    {
        public Dictionary<string, string> Attributes { get; set; }
        public ImageModel Image { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool Disabled { get; set; }
        public bool IsSelected { get; set; }
        public IList<ContentLinkModel> Links { get; set; } = new List<ContentLinkModel>();
        public string ExtraInfo { get; set; }

        internal class Mapper : IAutoMapperConfiguration
        {
            void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
            {
                cfg.CreateMap<FilterItem, ContentLinkModel>();
                cfg.CreateMap<GroupFilter, ContentLinkModel>();
            }
        }
    }
}