using Litium.Web.Models;
using System.Collections.Generic;
using Litium.Runtime.AutoMapper;
using JetBrains.Annotations;
using Litium.FieldFramework;
using AutoMapper;
using Litium.Accelerator.Constants;
using Litium.FieldFramework.FieldTypes;
using System.Linq;
using System.Globalization;

namespace Litium.Accelerator.ViewModels.Framework
{
    public class FooterViewModel : PageViewModel
    {
        public virtual List<SectionModel> SectionList { get; set; } = new List<SectionModel>();
    }

    public class SectionModel : IAutoMapperConfiguration
    {
        public virtual IList<LinkModel> SectionLinkList { get; set; } = new List<LinkModel>();
        public virtual EditorString SectionText { get; set; }
        public virtual string SectionTitle { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<MultiFieldItem, SectionModel>()
               .ForMember(x => x.SectionTitle, m => m.MapFrom(c => c.Fields.GetValue<string>(AcceleratorWebsiteFieldNameConstants.FooterHeader, CultureInfo.CurrentUICulture)))
               .ForMember(x => x.SectionLinkList, m => m.MapFrom(c => c.Fields.GetValue<IList<PointerItem>>(AcceleratorWebsiteFieldNameConstants.FooterLinkList).OfType<PointerPageItem>().ToList().Select(x => x.MapTo<LinkModel>()).Where(c => c != null).ToList() ?? new List<LinkModel>()))
               .ForMember(x => x.SectionText, m => m.MapFrom(c => c.Fields.GetValue<string>(AcceleratorWebsiteFieldNameConstants.FooterText, CultureInfo.CurrentUICulture)));
        }
    }
}