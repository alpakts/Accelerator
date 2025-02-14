using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Litium.Sales;
using Litium.Validations;

namespace Litium.Accelerator.ValidationRules
{
    public class PostActionValidationResult : ValidationResult
    {
        public List<Func<CartContext, Task>> Actions { get; } = new();
    }
}
