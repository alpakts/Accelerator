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
    [OrganizationRole(true, false)]
    [Route("api/mypageperson")]
    public class MyPagePersonController : ApiControllerBase
    {
        private readonly BusinessPersonViewModelBuilder _b2BPersonViewModelBuilder;
        private readonly PersonViewModelService _personViewModelService;

        public MyPagePersonController(BusinessPersonViewModelBuilder b2BPersonViewModelBuilder,
            PersonViewModelService personViewModelService)
        {
            _b2BPersonViewModelBuilder = b2BPersonViewModelBuilder;
            _personViewModelService = personViewModelService;
        }

        /// <summary>
        /// Gets all persons of the organization that the current user belongs to.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            var model = _b2BPersonViewModelBuilder.Build();
            return Ok(model);
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="id">The person system identifier.</param>
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(Guid id)
        {
            var model = _b2BPersonViewModelBuilder.Build(id);
            return Ok(model);
        }

        /// <summary>
        /// Updates the person.
        /// </summary>
        /// <param name="viewModel">Object containing the person information.</param>
        [HttpPut]
        [Route("")]
        [ValidateAntiForgeryToken]
        public IActionResult Update(BusinessPersonViewModel viewModel)
        {
            if (_personViewModelService.Validate(viewModel, ModelState)
                && _personViewModelService.Update(viewModel, ModelState))
            {
                return Ok();
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Deletes the link between the person and the organization.
        /// </summary>
        /// <param name="personId">The person system identifier.</param>
        [HttpDelete]
        [Route("")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete([FromBody]Guid personId)
        {
            _personViewModelService.Delete(personId);
            return Ok();
        }

        /// <summary>
        /// Creates the person.
        /// </summary>
        /// <param name="viewModel">Object containing the person information.</param>
        [HttpPost]
        [Route("")]
        [ValidateAntiForgeryToken]
        public IActionResult Add(BusinessPersonViewModel viewModel)
        {
            if (_personViewModelService.Validate(viewModel, ModelState)
                && _personViewModelService.Create(viewModel, ModelState))
            {
                return Ok();
            }

            return BadRequest(ModelState);
        }
    }
}
