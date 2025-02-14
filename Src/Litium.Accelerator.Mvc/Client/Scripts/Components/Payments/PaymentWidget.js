import React from 'react';
import DynamicComponent from '../DynamicComponent';

const PaymentWidget = React.memo(function PaymentWidget({ responseString }) {
    const id = 'checkout-widget';

    const renderWidget = (responseString) => {
        const WidgetCheckout = DynamicComponent({
            loader: () => import('./CheckoutWidget'),
        });
        const args = {
            responseString,
            id,
        };
        return <WidgetCheckout {...args} />;
    };

    return renderWidget(responseString);
});

export default PaymentWidget;
