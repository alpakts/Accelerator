using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Administration.Extensions.ViewModel;
using Litium.Accelerator.Services;
using Litium.Blocks;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Runtime;
using Litium.Web;
using Litium.Web.Administration.FieldFramework;
using Litium.Web.Administration.WebApi;
using Litium.Websites;
using Microsoft.AspNetCore.Mvc;
using IHttpActionResult = Microsoft.AspNetCore.Mvc.IActionResult;
using RoutePrefix = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Litium.Accelerator.Administration.Extensions.Controllers
{
    [RoutePrefix("site/administration/extensions/api/searchindexing")]
    public class SearchIndexingController : BackofficeApiController
    {
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly TemplateSettingService _templateSettingService;
        private readonly FieldDefinitionService _fieldDefinitionService;

        public SearchIndexingController(FieldTemplateService fieldTemplateService, TemplateSettingService templateSettingService, FieldDefinitionService fieldDefinitionService)
        {
            _fieldTemplateService = fieldTemplateService;
            _templateSettingService = templateSettingService;
            _fieldDefinitionService = fieldDefinitionService;
        }

        [Route("")]
        [HttpGet]
        public SearchIndexingModel Get()
        {
            var templates = _fieldTemplateService.GetAll().ToList();
            return new SearchIndexingModel
            {
                GroupedTemplates = new List<SearchIndexingModel.TemplateGroup>()
                {
                    new SearchIndexingModel.TemplateGroup
                    {
                        Title = "accelerator.settings.searchindexing.template.product.templates.headline".AsAngularResourceString(),
                        Templates = GetTemplates<ProductFieldTemplate, ProductArea>(templates)
                    },
                    new SearchIndexingModel.TemplateGroup
                    {
                        Title = "accelerator.settings.searchindexing.template.category.templates.headline".AsAngularResourceString(),
                        Templates = GetTemplates<CategoryFieldTemplate, ProductArea>(templates)
                    },
                    new SearchIndexingModel.TemplateGroup()
                    {
                        Title = "accelerator.settings.searchindexing.template.page.templates.headline".AsAngularResourceString(),
                        Templates = GetTemplates<PageFieldTemplate, WebsiteArea>(templates)
                    },
                    new SearchIndexingModel.TemplateGroup()
                    {
                        Title = "accelerator.settings.searchindexing.template.block.templates.headline".AsAngularResourceString(),
                        Templates = GetTemplates<BlockFieldTemplate, BlockArea>(templates)
                    },
                }
            };
        }

        private List<SearchIndexingModel.Template> GetTemplates<TTemplate, TArea>(IEnumerable<FieldTemplate> templates)
            where TTemplate : FieldTemplate where TArea : IArea
        {
            var templateList = new List<SearchIndexingModel.Template>();
            foreach (var item in templates.OfType<TTemplate>())
            {
                //Take all fields for the TArea
                var templateFields = _fieldDefinitionService.GetAll<TArea>().Where(z => IsValidType(z.FieldType)).Select(y => y.MakeWritableClone()).ToList()
                    .ChangeDuplicateFilterItemTitles()
                    .Select(z => new SearchIndexingModel.FieldGroup
                    { Title = z.Localizations.CurrentUICulture.Name ?? z.Id, FieldId = z.Id.ToLowerInvariant() })
                    .OrderBy(z => z.Title)
                    .ToList();

                var selectedFields = _templateSettingService.GetTemplateIndexingFields<TArea>(item.Id) ?? new List<string>(); 

                var template = new SearchIndexingModel.Template
                {
                    Title = item.Localizations.CurrentUICulture.Name ?? item.Id,
                    TemplateId = item.Id,
                    AreaType = typeof(TArea),
                    SelectedFields = templateFields.Where(x => selectedFields.Contains(x.FieldId)).Select(z => z.FieldId).ToList(),
                    Fields = templateFields
                };
                templateList.Add(template);
            }

            return templateList.OrderBy(x => x.Title).ToList();
        }

        private bool IsValidType(string fieldType)
        {
            switch (fieldType)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                case SystemFieldTypeConstants.CustomerPointerOrganization:
#pragma warning restore CS0618 // Type or member is obsolete
                case SystemFieldTypeConstants.Date:
                case SystemFieldTypeConstants.DateTime:
                case SystemFieldTypeConstants.MediaPointerFile:
                case SystemFieldTypeConstants.MediaPointerImage:
                case SystemFieldTypeConstants.Object:
                case SystemFieldTypeConstants.Pointer:
                case SystemFieldTypeConstants.Boolean:
                case SystemFieldTypeConstants.Link:
                case "FilterFields":
                case "MediaPointerImageArray":
                    return false;
                default:
                    return true;
            }
        }

        [Route("")]
        [HttpPut]
        public IHttpActionResult Put([FromBody] SearchIndexingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var groupedTemplates in model.GroupedTemplates)
            {
                foreach (var item in groupedTemplates.Templates)
                {
                    _templateSettingService.SetTemplateIndexingFields(item.AreaType, item.TemplateId, item.SelectedFields.ToList());
                }
            }

            model = Get();

            model.Message = GetString(GetString("General.Item.Updated") + " " + GetString("pim.RebuildSearchIndex"));
            return Ok(model);
        }
    }
}
