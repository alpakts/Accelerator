using System.Collections.Generic;
using System.Linq;
using Litium.Common;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Administration.FieldFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Litium.Accelerator.FieldTypes
{
    [Service(Name = "FilterFields")]
    internal class FilterEditFieldTypeConverter : IEditFieldTypeConverter
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly FilterFieldRepository _filterFieldtypeService;
        private readonly FieldDefinitionService _fieldDefinitionService;

        public FilterEditFieldTypeConverter(
            JsonSerializer jsonSerializer,
            FilterFieldRepository filterFieldtypeService,
            FieldDefinitionService fieldDefinitionService)
        {
            _jsonSerializer = jsonSerializer;
            _filterFieldtypeService = filterFieldtypeService;
            _fieldDefinitionService = fieldDefinitionService;
        }

        public object ConvertFromEditValue(EditFieldTypeConverterArgs args, JToken item)
        {
            var array = item as JArray;
            List<string> items = null;

            if (array != null)
            {
                items = array.ToObject<List<string>>(_jsonSerializer);
            }

            var value = item as JValue;
            if (value != null)
            {
                items = new List<string>(new[] { value.ToObject<string>(_jsonSerializer) });
            }
            return items;
        }

        public JToken ConvertToEditValue(EditFieldTypeConverterArgs args, object item)
        {
            var items = item as IList<string> ?? new List<string>();

            var removedIds = new List<string>();
            var ignoredIds = new List<string> { "#Price", "#News" };
            var productFilteringFields = _filterFieldtypeService.GetProductFilteringFields();
            foreach (var fieldId in items.Where(x => !ignoredIds.Contains(x)))
            {
                if (!productFilteringFields.Contains(fieldId))
                {
                    removedIds.Add(fieldId);
                }
                else
                {
                    var field = _fieldDefinitionService.Get<ProductArea>(fieldId);
                    if (field == null || field.Hidden || !field.CanBeGridFilter)
                    {
                        removedIds.Add(fieldId);
                    }
                }
            }

            return new JArray(items.Where(x => !removedIds.Contains(x)));
        }

        public object CreateOptionsModel() => null;

        /// <summary>
        /// The editor component name.
        /// </summary>
        /// <remark>
        /// Extension module should have module name prefix in order to be able to find the correct component at client side.
        /// </remark>
        public string EditComponentName => "Accelerator#FieldEditorFilterFieldsComponent";

        /// <summary>
        /// The settings component name.
        /// </summary>
        /// <remark>
        /// Extension module should have module name prefix in order to be able to find the correct component at client side.
        /// </remark>
        public string SettingsComponentName { get; }
    }
}
