using AutoMapper;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.ViewModels.Order
{
    public class OrderConfirmationViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string Title { get; set; }

        public EditorString Text { get; set; }

        public OrderDetailsViewModel Order { get; set; } = new OrderDetailsViewModel();

        public void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, OrderConfirmationViewModel>()
                .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
                .ForMember(x => x.Text, m => m.MapFrom(page => page.GetValue<string>(PageFieldNameConstants.Text)));
        }
    }
}