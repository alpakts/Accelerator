using Litium.Sales;
using Litium.StateTransitions;
using Litium.Validations;

namespace Litium.Accelerator.StateTransitions.Shipments
{
    public class ReadyToShipToShippedCondition : StateTransitionValidationRule<Shipment>
    {
        public override string FromState => ShipmentState.ReadyToShip;

        public override string ToState => ShipmentState.Shipped;

        public override ValidationResult Validate(Shipment entity)
        {
            // Add your own buissnisslogic validation for ready to ship to shipped transition.
            return new ValidationResult();
        }
    }
}
