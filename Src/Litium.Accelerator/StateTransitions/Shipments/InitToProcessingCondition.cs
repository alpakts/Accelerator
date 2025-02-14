using Litium.Sales;
using Litium.StateTransitions;
using Litium.Validations;

namespace Litium.Accelerator.StateTransitions.Shipments
{
    public class InitToProcessingCondition : StateTransitionValidationRule<Shipment>
    {
        public override string FromState => ShipmentState.Init;

        public override string ToState => ShipmentState.Processing;

        public override ValidationResult Validate(Shipment entity)
        {
            // Add your own buissnisslogic validation for init to processing transition.
            return new ValidationResult();
        }
    }
}
