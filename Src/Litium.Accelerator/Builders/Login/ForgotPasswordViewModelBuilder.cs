using Litium.Accelerator.Caching;
using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Login;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using Litium.Web.Models.Websites;

namespace Litium.Accelerator.Builders.Login
{
    public class ForgotPasswordViewModelBuilder : IViewModelBuilder<ForgotPasswordViewModel>
    {
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly PageByFieldTemplateCache<LoginPageByFieldTemplateCache> _pageByFieldType;

        public ForgotPasswordViewModelBuilder(RequestModelAccessor requestModelAccessor, PageByFieldTemplateCache<LoginPageByFieldTemplateCache> pageByFieldType)
        {
            _requestModelAccessor = requestModelAccessor;
            _pageByFieldType = pageByFieldType;
        }

        public virtual ForgotPasswordViewModel Build()
            => Build(_requestModelAccessor.RequestModel.CurrentPageModel);

        public virtual ForgotPasswordViewModel Build(PageModel pageModel)
        {
            var model = pageModel.MapTo<ForgotPasswordViewModel>();
            model.LoginLink = GetLoginPageLink();
            model.ChannelSystemId = _requestModelAccessor.RequestModel.ChannelModel.SystemId;
            return model;
        }

        public virtual ForgotPasswordViewModel Build(ForgotPasswordFormViewModel forgotPasswordForm)
        {
            var model = Build();
            model.ForgotPasswordForm = forgotPasswordForm;
            return model;
        }

        private LinkModel GetLoginPageLink()
        {
            LinkModel loginPage = null;
            _pageByFieldType.TryFindPage(page =>
            {
                if (page == null)
                {
                    return false;
                }

                loginPage = page.MapTo<LinkModel>();
                loginPage = loginPage !=null && loginPage.AccessibleByUser ? loginPage : null;
                return true;
            }, true);

            return loginPage;
        }
    }
}
