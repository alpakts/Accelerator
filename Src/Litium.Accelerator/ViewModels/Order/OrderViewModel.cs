using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Builders;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.ViewModels.Order
{
    public class OrderViewModel : IAutoMapperConfiguration, IViewModel
    {
        public OrderDetailsViewModel Order { get; set; } = new OrderDetailsViewModel();

        public bool IsPrintPage { get; set; }
        public bool ShowButton { get => !IsPrintPage; }
        public string OrderHistoryUrl { get; set; }

        public bool IsBusinessCustomer { get; set; }
        public bool HasApproverRole { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, OrderViewModel>();
        }
    }
}
