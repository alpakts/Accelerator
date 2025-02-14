using System.Collections.Generic;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Framework;
using Litium.FieldFramework;
using Litium.Runtime.AutoMapper;

namespace Litium.Accelerator.Builders.Framework
{
    public class FooterViewModelBuilder : IViewModelBuilder<FooterViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;

        public FooterViewModelBuilder(RequestModelAccessor requestModelAccessor)
        {
            _requestModelAccessor = requestModelAccessor;
        }

        public FooterViewModel Build()
        {
            var model = new FooterViewModel();
            var website = _requestModelAccessor.RequestModel.WebsiteModel;
            var footer = website.GetValue<IList<MultiFieldItem>>(AcceleratorWebsiteFieldNameConstants.Footer);
            if (footer != null)
            {
                model.SectionList = footer.Select(c => c.MapTo<SectionModel>()).ToList();
            }

            return model;
        }
    }
}