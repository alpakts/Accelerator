using System;
using System.Collections.Generic;
using System.Linq;
using Litium.FieldFramework;

namespace Litium.Accelerator.FieldTypes
{
    public class FilterFieldTypeMetadata : FieldTypeMetadataBase
    {
        public override string Id => "FilterFields";
        public override bool IsArray => true;
        public override Type JsonType => typeof(List<string>);

        public override IFieldType CreateInstance(IFieldDefinition fieldDefinition)
        {
            var item = new FilterFieldType();
            item.Init(fieldDefinition);
            return item;
        }

        public class FilterFieldType : FieldTypeBase
        {
            public override object GetValue(ICollection<FieldData> fieldDatas)
            {
                if (fieldDatas.Count == 1 && fieldDatas.First().BooleanValue.HasValue)
                {
                    return null;
                }

                return fieldDatas.Where(x => !string.IsNullOrEmpty(x.TextValue)).Select(x => x.TextValue).ToList();
            }

            public override ICollection<FieldData> PersistFieldData(object item)
            {
                return PersistFieldDataInternal(item);
            }

            protected override ICollection<FieldData> PersistFieldDataInternal(object item)
            {
                if (!(item is IEnumerable<string>data))
                {
                    return new[] { new FieldData { BooleanValue = true } };
                }

                return data.Select(x => new FieldData { TextValue = x }).ToList();
            }

            public override object ConvertFromJsonValue(object item)
            {
                return item as IList<string>;
            }

            public override object ConvertToJsonValue(object item)
            {
                var items = item as IList<string>;

                if (items == null && item is string value)
                {
                    items = new List<string> { value };
                }

                return items;
            }
        }
    }
}
