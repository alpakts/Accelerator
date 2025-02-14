using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Litium.Common;

namespace Litium.Accelerator.Services
{
    internal class TemplateSettingServiceImpl : TemplateSettingService
    {
        private readonly ConcurrentDictionary<string, string> _keyResolver = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase); 
        private readonly ConcurrentDictionary<(Type, string), string> _indexingKeyResolver = new ConcurrentDictionary<(Type, string), string>(new KeyComparer());

        private readonly SettingService _settingService;

        public TemplateSettingServiceImpl(SettingService settingService)
        {
            _settingService = settingService;
        }

        public override string GetTemplateGroupingField(string templateId)
        {
            return _settingService.Get<string>(_keyResolver.GetOrAdd(templateId, GetGroupingKey));
        }

        public override void SetTemplateGroupings(string templateId, string groupingField)
        {
            _settingService.Set(_keyResolver.GetOrAdd(templateId, GetGroupingKey), groupingField);
        }

        public override void SetTemplateIndexingFields(Type areaType, string templateId, ICollection<string> fields)
        {
            _settingService.Set(_indexingKeyResolver.GetOrAdd((areaType, templateId), GetIndexingKey), fields);
        }

        public override ICollection<string> GetTemplateIndexingFields<TArea>(string templateId)
        {
            return _settingService.Get<ICollection<string>>(_indexingKeyResolver.GetOrAdd((typeof(TArea), templateId), GetIndexingKey));
        }

        private string GetGroupingKey(string id)
        {
            return $"IndexingTemplateGrouping:{id}";
        }
        private string GetIndexingKey((Type area, string id) key)
        {
            return $"IndexingTemplateFields:{key.area.Name}:{key.id}";
        }

        private class KeyComparer : IEqualityComparer<(Type, string)>
        {
            public bool Equals((Type, string) x, (Type, string) y)
            {
                return x.Item1 == y.Item1
                    && string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode((Type, string) obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
