import React from 'react';
import DynamicComponent from '../DynamicComponent';

const ShipmentWidget = React.memo(function ShipmentWidget({ responseString }) {
    const id = 'shipment-checkout-widget';

    const renderWidget = (responseString) => {
        const WidgetCheckout = DynamicComponent({
            loader: () => import('../Payments/CheckoutWidget'),
        });
        const args = {
            responseString,
            id,
        };
        return <WidgetCheckout {...args} />;
    };

    return renderWidget(responseString);
});

export default ShipmentWidget;
