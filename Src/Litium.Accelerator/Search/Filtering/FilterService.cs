using System.Collections.Generic;
using Litium.Accelerator.Constants;
using Litium.Accelerator.FieldTypes;
using Litium.Accelerator.Routing;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Search.Filtering
{
    [Service(ServiceType = typeof(FilterService), Lifetime = DependencyLifetime.Singleton)]
    public class FilterService
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly FilterFieldRepository _filterFieldRepository;

        internal const string _key = "Accelerator.ProductFiltering";

        public FilterService(
            RequestModelAccessor requestModelAccessor,
            FilterFieldRepository filterFieldRepository)
        {
            _requestModelAccessor = requestModelAccessor;
            _filterFieldRepository = filterFieldRepository;
        }

        public bool IndexFilter(string filterName)
        {
            var filters = _requestModelAccessor.RequestModel.WebsiteModel.GetValue<IList<string>>(AcceleratorWebsiteFieldNameConstants.FiltersIndexedBySearchEngines);
            return filters != null && filters.Contains(filterName);
        }

        public IList<string> GetProductFilteringFields()
        {
            return _filterFieldRepository.GetProductFilteringFields();
        }

        public void SaveProductFilteringFields(IList<string> items)
        {
            _filterFieldRepository.SaveProductFilteringFields(items);
        }
    }
}
