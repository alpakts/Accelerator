using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Common;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.FieldTypes
{
    [Service(ServiceType = typeof(FilterFieldRepository), Lifetime = DependencyLifetime.Singleton)]
    public class FilterFieldRepository
    {
        private readonly SettingService _settingsService;

        internal const string _key = "Accelerator.ProductFiltering";

        public FilterFieldRepository(
            SettingService settingsService)
        {
            _settingsService = settingsService;
        }

        public IList<string> GetProductFilteringFields()
        {
            try
            {
                return _settingsService.Get<IList<string>>(_key) ?? new List<string>();
            }
            catch (InvalidCastException)
            {
                try
                {
                    return _settingsService.Get<ICollection<string>>(_key)?.ToList() ?? new List<string>();
                }
                catch
                {
                    // swallow all exceptions
                }
                throw;
            }
        }

        public void SaveProductFilteringFields(IList<string> items)
        {
            _settingsService.Set(_key, items);
        }
    }
}
