using Litium.Sales;
using Litium.StateTransitions;
using Litium.Validations;

namespace Litium.Accelerator.StateTransitions.SalesOrders
{
    public class ProcessingToCompletedCondition : StateTransitionValidationRule<SalesOrder>
    {
        public override string FromState => OrderState.Processing;

        public override string ToState => OrderState.Completed;

        public override ValidationResult Validate(SalesOrder entity)
        {
            // Add your own buissnisslogic validation for processing to completed transition.
            return new ValidationResult();
        }
    }
}
