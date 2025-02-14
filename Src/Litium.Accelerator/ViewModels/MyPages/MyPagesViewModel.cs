using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Caching;
using Litium.Accelerator.Constants;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;
using System;
using Litium.Accelerator.Builders;
using Litium.Accelerator.Extensions;

namespace Litium.Accelerator.ViewModels.MyPages
{
    public class MyPagesViewModel : IAutoMapperConfiguration, IViewModel
    {
        public bool MayUserEditLogin { get; set; }

        public MyDetailsViewModel MyDetailsPanel { get; set; }
        public BusinessCustomerDetailsViewModel BusinessCustomerDetailsPanel { get; set; }
        public LoginInfoViewModel LoginInfoPanel { get; set; }

        public bool IsBusinessCustomer { get; set; } = false;
        public LinkModel LogoutLink { get; set; }
        public string CurrentTab { get; set; }

        public bool HasApproverRole { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PageModel, MyPagesViewModel>()
               .ForMember(x => x.MayUserEditLogin, m => m.MapFromField(PageFieldNameConstants.MayUserEditLogin))
               .ForMember(x => x.LogoutLink, m => m.MapFrom<LogOutLinkResolver>());
        }

        [UsedImplicitly]
        protected class LogOutLinkResolver : IValueResolver<PageModel, MyPagesViewModel, LinkModel>
        {
            private readonly PageByFieldTemplateCache<LoginPageByFieldTemplateCache> _pageByFieldType;

            public LogOutLinkResolver(PageByFieldTemplateCache<LoginPageByFieldTemplateCache> pageByFieldType)
            {
                _pageByFieldType = pageByFieldType;
            }

            public LinkModel Resolve(PageModel source, MyPagesViewModel destination, LinkModel destMember, ResolutionContext context)
            {
                LinkModel loginPage = null;
                var found = _pageByFieldType.TryFindPage(login =>
                {
                    loginPage = login?.MapTo<LinkModel>();
                    if (loginPage == null)
                    {
                        return false;
                    }
                    return true;
                });
                if (!found)
                {
                    return null;
                }
                if (loginPage.Href != null)
                {
                    loginPage.Href += ".Logout";
                }
                return loginPage;
            }
        }
    }
}
