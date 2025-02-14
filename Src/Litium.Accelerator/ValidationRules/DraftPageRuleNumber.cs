using Litium.Websites;
using Litium.Validations;
using Litium.FieldFramework;
using Litium.Accelerator.Constants;
using Litium.Web;

namespace Litium.Accelerator.ValidationRules
{
    internal class DraftPageRuleNumber : ValidationRuleBase<DraftPage>
    {
        private readonly FieldTemplateService _fieldTemplateService;

        public DraftPageRuleNumber(FieldTemplateService fieldTemplateService)
        {
            _fieldTemplateService = fieldTemplateService;
        }

        public override ValidationResult Validate(DraftPage entity, ValidationMode validationMode)
        {
            var result = new ValidationResult();

            if (validationMode == ValidationMode.Modify)
            {
                var fieldId = string.Empty;
                var fieldValue = 0;
                var fieldTemplate = _fieldTemplateService.Get<PageFieldTemplate>(entity.FieldTemplateSystemId);

                switch (fieldTemplate.Id)
                {
                    case PageTemplateNameConstants.NewsList:
                        fieldId = PageFieldNameConstants.NumberOfNewsPerPage;
                        fieldValue = entity.Fields.GetValue<int>(fieldId);
                        break;
                    case PageTemplateNameConstants.OrderHistory:
                        fieldId = PageFieldNameConstants.NumberOfOrdersPerPage;
                        fieldValue = entity.Fields.GetValue<int>(fieldId);
                        break;
                    case PageTemplateNameConstants.BrandList:
                        fieldId = PageFieldNameConstants.PageSize;
                        fieldValue = entity.Fields.GetValue<int>(fieldId);
                        break;
                    default:
                        break;
                }

                if (fieldValue < 0)
                {
                    result.AddError(fieldId, "accelerator.validation.number".AsAngularResourceString());
                }
            }

            return result;
        }
    }
}


