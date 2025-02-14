using System;
using System.Collections.Generic;
using Litium.Runtime;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Services
{
    [Service(ServiceType = typeof(TemplateSettingService))]
    public abstract class TemplateSettingService
    {
        public abstract string GetTemplateGroupingField(string templateId);
        public abstract void SetTemplateGroupings(string templateId, string groupingField);

        public abstract ICollection<string> GetTemplateIndexingFields<TArea>(string templateId) where TArea : IArea;
        public abstract void SetTemplateIndexingFields(Type areaType, string templateId, ICollection<string> fields);
    }
}
