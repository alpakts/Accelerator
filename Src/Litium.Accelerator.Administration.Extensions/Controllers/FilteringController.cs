using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Administration.Extensions.ViewModel;
using Litium.Accelerator.Search.Filtering;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Web;
using Litium.Web.Administration.FieldFramework;
using Litium.Web.Administration.WebApi;
using Microsoft.AspNetCore.Mvc;
using IHttpActionResult = Microsoft.AspNetCore.Mvc.IActionResult;
using RoutePrefix = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Litium.Accelerator.Administration.Extensions.Controllers
{
    [RoutePrefix("site/administration/extensions/api/filtering")]
    public class FilteringController : BackofficeApiController
    {
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly FilterService _filterService;

        public FilteringController(FieldDefinitionService fieldDefinitionService, FilterService filterService)
        {
            _fieldDefinitionService = fieldDefinitionService;
            _filterService = filterService;
        }

        [HttpGet]
        public IEnumerable<FilteringModel.Item> Get()
        {
            var selected = _filterService.GetProductFilteringFields();
            var all = GetAllFields();

            return all.Where(x => selected.Contains(x.FieldId)).OrderBy(x => selected.IndexOf(x.FieldId)).ToList();
        }

        [Route("setting")]
        public FilteringModel GetSetting()
        {
            var selected = _filterService.GetProductFilteringFields();
            var all = GetAllFields();

            return new FilteringModel
            {
                Filters = all,
                Items = all.Where(x => selected.Contains(x.FieldId)).OrderBy(x => selected.IndexOf(x.FieldId)).Select(x => x.FieldId).ToList(),
            };
        }

        [Route("setting")]
        [HttpPost]
        public IHttpActionResult SaveSetting([FromBody] FilteringModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var selected = model.Items.ToList();
            _filterService.SaveProductFilteringFields(selected);
            model = GetSetting();

            model.Message = GetString("General.Item.Updated");
            return Ok(model);
        }

        private List<FilteringModel.Item> GetAllFields()
        {
            return _fieldDefinitionService.GetAll<ProductArea>()
                            .Where(x => !x.Hidden && x.CanBeGridFilter)
                            .Select(y => y.MakeWritableClone())
                            .ToList()
                            .ChangeDuplicateFilterItemTitles()
                            .Select(x => new FilteringModel.Item
                            {
                                FieldId = x.Id,
                                Title = x.Localizations.CurrentCulture.Name ?? x.Id,
                                GroupName = (x.SystemDefined ? "pim.template.fieldgroup.systemdefined" : "pim.template.fieldgroup.userdefined").AsAngularResourceString()
                            })
                            .Concat(new[]
                            {
                                new FilteringModel.Item {
                                    FieldId = "#Price",
                                    Title = "accelerator.filterfield.filterprice".AsAngularResourceString(),
                                    GroupName = "accelerator.filterfield.predefined".AsAngularResourceString() },
                                new FilteringModel.Item {
                                    FieldId = "#News",
                                    Title = "accelerator.filterfield.filternews".AsAngularResourceString(),
                                    GroupName = "accelerator.filterfield.predefined".AsAngularResourceString() }
                            })
                            .ToList();
        }
    }
}
