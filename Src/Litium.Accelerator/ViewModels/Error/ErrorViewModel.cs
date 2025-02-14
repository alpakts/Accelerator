using Litium.Web.Models;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;
using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;

namespace Litium.Accelerator.ViewModels.Error
{
    public class ErrorViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string Title { get; set; }
        public EditorString Text { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, ErrorViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.Text, m => m.MapFrom(page => page.GetValue<string>(PageFieldNameConstants.ErrorMessage)));
        }
    }
}

