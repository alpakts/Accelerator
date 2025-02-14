using Litium.Sales;
using Litium.StateTransitions;
using Litium.Validations;

namespace Litium.Accelerator.StateTransitions.SalesOrders
{
    public class ConfirmedToProcessingCondition : StateTransitionValidationRule<SalesOrder>
    {
        public override string FromState => OrderState.Confirmed;

        public override string ToState => OrderState.Processing;

        public override ValidationResult Validate(SalesOrder entity)
        {
            //Empty condition and always returns no error.
            return new ValidationResult();
        }
    }
}
