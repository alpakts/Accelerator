using System.Linq;
using Litium.Accelerator.Administration.Extensions.ViewModel;
using Litium.Accelerator.Services;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Web.Administration.WebApi;
using Microsoft.AspNetCore.Mvc;
using IHttpActionResult = Microsoft.AspNetCore.Mvc.IActionResult;
using RoutePrefix = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Litium.Accelerator.Administration.Extensions.Controllers
{
    [RoutePrefix("site/administration/extensions/api/indexing")]
    public class IndexingController : BackofficeApiController
    {
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly TemplateSettingService _templateSettingService;
        private readonly FieldDefinitionService _fieldDefinitionService;

        public IndexingController(FieldTemplateService fieldTemplateService, TemplateSettingService templateSettingService, FieldDefinitionService fieldDefinitionService)
        {
            _fieldTemplateService = fieldTemplateService;
            _templateSettingService = templateSettingService;
            _fieldDefinitionService = fieldDefinitionService;
        }

        [HttpGet]
        public IndexingModel Get()
        {
            return new IndexingModel
            {
                Templates = _fieldTemplateService.GetAll().OfType<ProductFieldTemplate>().Select(x => new IndexingModel.Template
                {
                    Title = x.Localizations.CurrentUICulture.Name ?? x.Id,
                    TemplateId = x.Id,
                    GroupingFieldId = _templateSettingService.GetTemplateGroupingField(x.Id)?.ToLowerInvariant(),
                    Fields = x.VariantFieldGroups.SelectMany(z => z.Fields)
                        .Distinct()
                        .Select(z => _fieldDefinitionService.Get<ProductArea>(z))
                        .Where(z => z != null)
                        .Select(z => new IndexingModel.FieldGroup {Title = z.Localizations.CurrentUICulture.Name ?? z.Id, FieldId = z.Id.ToLowerInvariant() })
                        .OrderBy(z => z.Title)
                        .ToList()
                })
                .OrderBy(x => x.Title)
                .ToList()
            };
        }

        [HttpPut]
        public IHttpActionResult Put([FromBody] IndexingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var item in model.Templates)
            {
                _templateSettingService.SetTemplateGroupings(item.TemplateId, item.GroupingFieldId);
            }

            var result = Get();
            result.Message = GetString("General.Item.Updated") + " " + GetString("pim.RebuildSearchIndex");
            return Ok(result);
        }
    }
}
