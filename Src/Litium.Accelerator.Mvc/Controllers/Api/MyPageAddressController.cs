using System;
using Litium.Accelerator.Builders.MyPages;
using Litium.Accelerator.Mvc.Attributes;
using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.Persons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Accelerator.Mvc.Controllers.Api
{
    [Authorize]
    [OrganizationRole(true, true)]
    [Route("api/mypageaddress")]
    public class MyPageAddressController : ApiControllerBase
    {
        private readonly BusinessAddressViewModelBuilder _businessAddressViewModelBuilder;
        private readonly AddressViewModelService _addressViewModelService;

        public MyPageAddressController(BusinessAddressViewModelBuilder businessAddressViewModelBuilder, AddressViewModelService addressViewModelService)
        {
            _businessAddressViewModelBuilder = businessAddressViewModelBuilder;
            _addressViewModelService = addressViewModelService;
        }

        /// <summary>
        /// Gets all addresses of the organization that the current user belongs to.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            var model = _businessAddressViewModelBuilder.Build();
            return Ok(model);
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <param name="id">The address system identifier.</param>
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(Guid id)
        {
            var model = _businessAddressViewModelBuilder.Build(id);
            return Ok(model);
        }

        /// <summary>
        /// Creates the address.
        /// </summary>
        /// <param name="viewModel">Object containing the address information.</param>
        [HttpPost]
        [Route("")]
        [ValidateAntiForgeryToken]
        public IActionResult Add(AddressViewModel viewModel)
        {
            if (_addressViewModelService.Validate(viewModel, ModelState) &&
                _addressViewModelService.Create(viewModel, ModelState))
            {
                return Ok();
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Updates the address.
        /// </summary>
        /// <param name="viewModel">Object containing the address information.</param>
        [HttpPut]
        [Route("")]
        [ValidateAntiForgeryToken]
        public IActionResult Update(AddressViewModel viewModel)
        {
            if (_addressViewModelService.Validate(viewModel, ModelState) && _addressViewModelService.Update(viewModel, ModelState))
            {
                return Ok();
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Deletes the address.
        /// </summary>
        /// <param name="id">The address system identifier.</param>
        [HttpDelete]
        [Route("")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete([FromBody] Guid id)
        {
            _addressViewModelService.Delete(id);
            return Ok();
        }
    }
}
