using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Builders.Order;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Order;
using Litium.Data;
using Litium.Sales;
using Litium.Sales.Queryable;
using Litium.Security;
using Litium.Tagging;
using Litium.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Authorize]
    [Route("api/order")]
    public class OrderController : ApiControllerBase
    {
        private readonly SecurityContextService _securityContextService;
        private readonly OrderViewModelBuilder _orderViewModelBuilder;
        private readonly TaggingService _taggingService;
        private readonly OrderOverviewService _orderOverviewService;
        private readonly OrderService _orderService;
        private readonly PersonStorage _personStorage;
        private readonly DataService _dataService;
        private const int DefaultNumberOfOrderPerPage = 5;

        public OrderController(SecurityContextService securityContextService,
            OrderOverviewService orderOverviewService,
            OrderService orderService,
            PersonStorage personStorage,
            OrderViewModelBuilder orderViewModelBuilder,
            TaggingService taggingService,
            DataService dataService)
        {
            _securityContextService = securityContextService;
            _orderViewModelBuilder = orderViewModelBuilder;
            _taggingService = taggingService;
            _orderOverviewService = orderOverviewService;
            _orderService = orderService;
            _personStorage = personStorage;
            _dataService = dataService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult List(int pageIndex = 1, bool showMyOrders = false, int pageSize = DefaultNumberOfOrderPerPage)
        {
            var orders = new List<OrderDetailsViewModel>();
            var totalOrderCount = 0;
            var personId = _securityContextService.GetIdentityUserSystemId();

            using (var query = _dataService.CreateQuery<SalesOrder>())
            {
                var queryResult = query.Filter(filter =>
                {
                    if (_personStorage.CurrentSelectedOrganizationSystemId is null || !_personStorage.HasApproverRole || showMyOrders)
                    {
                        filter.PersonSystemId(personId.Value);
                    }

                    filter.Bool(b => b.Should(s =>
                    {
                        s.Bool(x => x.Must(m =>
                        {
                            m.OrganizationSystemId(null);
                            if (_personStorage.HasApproverRole && !showMyOrders)
                            {
                                m.PersonSystemId(personId.Value);
                            }
                        }));

                        if (_personStorage.CurrentSelectedOrganizationSystemId is Guid organizationSystemId)
                        {
                            s.OrganizationSystemId(organizationSystemId);
                        }
                    }));
                })
                .Sort(descriptor => descriptor.OrderNumber(Data.Queryable.SortDirection.Descending));

                totalOrderCount = queryResult.Count();
                var orderSystemIds = queryResult
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToSystemIdList();
                orders = _orderOverviewService.Get(orderSystemIds).Select(x => _orderViewModelBuilder.Build(x)).ToList();
            }
            return Ok(new { orders, totalCount = totalOrderCount });
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetOrder(Guid id)
        {
            var orderOverView = _orderOverviewService.Get(id);
            if (orderOverView is null)
            {
                ModelState.AddModelError("general", "mypage.order.notfound".AsWebsiteText());
                return BadRequest(ModelState);
            }

            var personId = _securityContextService.GetIdentityUserSystemId();
            //user still not login
            if (!personId.HasValue)
            {
                ModelState.AddModelError("general", "mypage.order.nothavepermission".AsWebsiteText());
                return BadRequest(ModelState);
            }

            if (_personStorage.CurrentSelectedOrganizationSystemId is null)
            {
                if (orderOverView.SalesOrder.CustomerInfo.OrganizationSystemId is not null || orderOverView.SalesOrder.CustomerInfo.PersonSystemId != personId)
                {
                    ModelState.AddModelError("general", "mypage.order.nothavepermission".AsWebsiteText());
                    return BadRequest(ModelState);
                }
            }
            else
            {
                if (orderOverView.SalesOrder.CustomerInfo.OrganizationSystemId != _personStorage.CurrentSelectedOrganizationSystemId
                    || (!_personStorage.HasApproverRole && orderOverView.SalesOrder.CustomerInfo.PersonSystemId != personId))
                {
                    ModelState.AddModelError("general", "mypage.order.nothavepermission".AsWebsiteText());
                    return BadRequest(ModelState);
                }
            }
            var result = _orderViewModelBuilder.Build(orderOverView);

            return Ok(result);
        }

        [HttpPost]
        [Route("approveOrder")]
        public IActionResult ApproveOrder(RequestApproveModel model)
        {
            if (model.Id == Guid.Empty)
            {
                ModelState.AddModelError("general", "mypage.order.invalid".AsWebsiteText());
            }

            var order = _orderService.Get<SalesOrder>(model.Id);
            if (order is null)
            {
                ModelState.AddModelError("general", "mypage.order.notfound".AsWebsiteText());
            }

            Guid organizationId = Guid.Empty;
            if (order.CustomerInfo is not null || order.CustomerInfo.OrganizationSystemId.HasValue)
            {
                organizationId = order.CustomerInfo.OrganizationSystemId.Value;
            }

            if (_personStorage.CurrentSelectedOrganizationSystemId is null || !_personStorage.HasApproverRole
                || organizationId == Guid.Empty || _personStorage.CurrentSelectedOrganizationSystemId != organizationId)
            {
                ModelState.AddModelError("general", "mypage.order.nothavepermission".AsWebsiteText());
            }

            var tags = _taggingService.GetAll<Sales.Order>(model.Id);
            if (tags.Count == 0
                || !tags.Contains(OrderTaggingConstants.AwaitOrderApproval)
                || tags.Contains(OrderTaggingConstants.ApprovalDenied))
            {
                ModelState.AddModelError("general", "mypage.order.cannotapprove".AsWebsiteText());
            }
            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(ModelState);
            }

            _taggingService.Remove<Sales.Order>(model.Id, OrderTaggingConstants.AwaitOrderApproval);
            return Ok(new { model.Id });
        }
    }
}
