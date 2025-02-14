using System;
using Litium.Accelerator.Builders;

namespace Litium.Accelerator.Utilities
{
    public class ValidationRuleItem<TForm> where TForm : IViewModel
    {
        public string Field { get; set; }

        public Func<TForm, bool> Rule { get; set; }

        public Func<string> ErrorMessage { get; set; }
    }
}
