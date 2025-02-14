using System;
using System.Threading.Tasks;
using Litium.Accelerator.ValidationRules;
using Litium.Sales;
using Litium.Validations;

namespace Litium.Accelerator.Extensions
{
    public static class ValidationExtensions
    {
        public static async Task ProcessPostActionsAsync(this Exception exception, CartContext cartContext)
        {
            if ((exception as ValidationException)?.ValidationResult is ValidationSummary validationSummary)
            {
                foreach (var item in validationSummary.Results)
                {
                    if (item.Result is PostActionValidationResult postActionResult)
                    {
                        foreach (var action in postActionResult.Actions)
                        {
                            await action(cartContext);
                        }
                    }
                }
            }
        }
    }
}
