using Litium.Sales;
using Litium.StateTransitions;
using Litium.Validations;

namespace Litium.Accelerator.StateTransitions.Shipments
{
    public class ProcessingToReadyToShipCondition : StateTransitionValidationRule<Shipment>
    {
        public override string FromState => ShipmentState.Processing;

        public override string ToState => ShipmentState.ReadyToShip;

        public override ValidationResult Validate(Shipment entity)
        {
            // Add your own buissnisslogic validation for processing to ready to ship transition.
            return new ValidationResult();
        }
    }
}
