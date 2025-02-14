using Litium.Accelerator.Builders;

namespace Litium.Accelerator.ViewModels.MyPages
{
    public class ChangePasswordFormViewModel : IViewModel
    {
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}