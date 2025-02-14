using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using Litium.FieldFramework;
using Litium.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;

namespace Litium.Accelerator.ViewModels.Block
{
    public class SliderBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public Guid SystemId { get; set; }

        public List<BannerBlockItemViewModel> Sliders { get; set; } = new List<BannerBlockItemViewModel>();

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, SliderBlockViewModel>()
               .ForMember(x => x.Sliders, m => m.MapFrom<SlideshowResolver>());
        }

        private class SlideshowResolver : IValueResolver<BlockModel, SliderBlockViewModel, List<BannerBlockItemViewModel>>
        {
            private readonly FieldTemplateService _fieldTemplateService;

            public SlideshowResolver(FieldTemplateService fieldTemplateService)
            {
                _fieldTemplateService = fieldTemplateService;
            }

            public List<BannerBlockItemViewModel> Resolve(BlockModel block, SliderBlockViewModel viewModel, List<BannerBlockItemViewModel> destMember, ResolutionContext context)
            {
                var result = new List<BannerBlockItemViewModel>();
                var blockTemplate = _fieldTemplateService.Get<FieldTemplateBase>(block.FieldTemplateSystemId);
                if (blockTemplate.FieldGroups.Any(x => x.Id == "Slides"))
                {
                    var blockItems = block.GetValue<IList<MultiFieldItem>>(BlockFieldNameConstants.Slider);
                    if (blockItems != null)
                    {
                        result.AddRange(blockItems.Select(c => c.MapTo<BannerBlockItemViewModel>()));
                    }
                }

                return result;
            }
        }
    }
}
