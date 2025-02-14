using AutoMapper;
using Litium.Runtime.AutoMapper;
using System;

namespace Litium.Accelerator.ViewModels.Search
{
    public class SearchResultItem: IAutoMapperConfiguration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public float Score { get; set; }

        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SearchResultItem, SearchItem>();
        }
    }
}
