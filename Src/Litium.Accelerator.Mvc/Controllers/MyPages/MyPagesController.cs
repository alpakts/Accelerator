using Litium.Accelerator.Services;
using Litium.Accelerator.ViewModels.MyPages;
using Litium.Accelerator.Builders.MyPages;
using Litium.Accelerator.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Litium.Accelerator.Mvc.Controllers.MyPages
{
    public class MyPagesController : ControllerBase
    {
        private readonly MyPagesViewModelBuilder _myPagesViewModelBuilder;
        private readonly MyPagesViewModelService _myPagesViewModelService;


        public MyPagesController(MyPagesViewModelBuilder myPagesViewModelBuilder, MyPagesViewModelService myPagesViewModelService)
        {
            _myPagesViewModelBuilder = myPagesViewModelBuilder;
            _myPagesViewModelService = myPagesViewModelService;
        }

        [HttpGet]
        public ActionResult Index(string currentTab = MyPagesTabConstants.MyDetails)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new RedirectResult("~/");
            }

            var model = _myPagesViewModelBuilder.Build();
            model.CurrentTab = currentTab;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveMyDetails(MyDetailsViewModel myDetailsPanel)
        {
            if (_myPagesViewModelService.IsValidMyDetailsForm(ModelState, myDetailsPanel))
            {
                await _myPagesViewModelService.SaveMyDetails(myDetailsPanel);
                return RedirectToAction(nameof(Index));
            }

            var model = _myPagesViewModelBuilder.Build(myDetailsPanel);
            model.CurrentTab = MyPagesTabConstants.MyDetails;
            return View(nameof(Index), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveBusinessCustomerDetails(BusinessCustomerDetailsViewModel businessCustomerDetailsPanel)
        {
            if (_myPagesViewModelService.IsValidBusinessCustomerDetailsForm(ModelState, businessCustomerDetailsPanel))
            {
                await _myPagesViewModelService.SaveMyDetails(businessCustomerDetailsPanel, false);
                return RedirectToAction(nameof(Index));
            }

            var model = _myPagesViewModelBuilder.Build(businessCustomerDetailsPanel);
            model.CurrentTab = MyPagesTabConstants.MyDetails;
            return View(nameof(Index), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveUserName(ChangeUserNameFormViewModel userNameForm)
        {
            if (_myPagesViewModelService.IsValidUserNameForm(ModelState, userNameForm))
            {
                _myPagesViewModelService.SaveUserName(userNameForm.UserName);
                return RedirectToAction(nameof(Index), new { CurrentTab = MyPagesTabConstants.LoginInfo });
            }

            var model = _myPagesViewModelBuilder.Build();
            model.CurrentTab = MyPagesTabConstants.LoginInfo;
            return View(nameof(Index), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePassword(ChangePasswordFormViewModel passwordForm)
        {
            if (_myPagesViewModelService.IsValidPasswordForm(ModelState, passwordForm))
            {
                _myPagesViewModelService.SavePassword(passwordForm.Password);
                return RedirectToAction(nameof(Index), new { CurrentTab = MyPagesTabConstants.LoginInfo });
            }

            var model = _myPagesViewModelBuilder.Build();
            model.CurrentTab = MyPagesTabConstants.LoginInfo;
            return View(nameof(Index), model);
        }
    }
}
