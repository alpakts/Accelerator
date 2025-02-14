using Litium.Sales;
using Litium.StateTransitions;
using Litium.Validations;

namespace Litium.Accelerator.StateTransitions.SalesOrders
{
    public class InitToConfirmedCondition : StateTransitionValidationRule<SalesOrder>
    {
        public override string FromState => OrderState.Init;

        public override string ToState => OrderState.Confirmed;

        public override ValidationResult Validate(SalesOrder entity)
        {
            // Add your own buissnisslogic validation for init to confirmed transition.
            return new ValidationResult();
        }
    }
}
