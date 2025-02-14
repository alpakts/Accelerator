using AutoMapper;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Blocks;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.FieldFramework.FieldTypes;
using Litium.Web.Models;
using System.Globalization;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.Block
{
    public class VideoBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public FileModel Video { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, VideoBlockViewModel>()
               .ForMember(x => x.Video, m => m.MapFrom(c => c.GetValue<PointerItem>(BlockFieldNameConstants.BlockVideo).EntitySystemId.MapTo<FileModel>()));
        }
    }
}
