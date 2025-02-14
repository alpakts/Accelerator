using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using System.Collections.Generic;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;
using Litium.FieldFramework.FieldTypes;

namespace Litium.Accelerator.ViewModels.Order
{
    public class OrderHistoryViewModel : IAutoMapperConfiguration, IViewModel
    {
        public string Title { get; set; }
        public string Introduction { get; set; }
        public EditorString Text { get; set; }
        public int NumberOfOrdersPerPage { get; set; }
        public LinkModel OrderLink { get; set; }

        public IList<OrderDetailsViewModel> Orders { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public bool IsBusinessCustomer { get; set; }
        public bool HasApproverRole { get; set; }
        public bool ShowOnlyMyOrders { get; set; }
        public string MyOrdersLink { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, OrderHistoryViewModel>()
               .ForMember(x => x.Title, m => m.MapFromField(PageFieldNameConstants.Title))
               .ForMember(x => x.Introduction, m => m.MapFromField(PageFieldNameConstants.Introduction))
               .ForMember(x => x.Text, m => m.MapFrom(orderHistoryPage => orderHistoryPage.GetValue<string>(PageFieldNameConstants.Text)))
               .ForMember(x => x.NumberOfOrdersPerPage, m => m.MapFromField(PageFieldNameConstants.NumberOfOrdersPerPage))
               .ForMember(x => x.OrderLink, m => m.MapFrom(orderHistoryPage => orderHistoryPage.GetValue<PointerPageItem>(PageFieldNameConstants.OrderLink).MapTo<LinkModel>()));
        }
    }
}

