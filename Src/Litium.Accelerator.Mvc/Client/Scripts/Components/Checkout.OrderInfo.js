import React from 'react';
import { useSelector } from 'react-redux';
import { translate } from '../Services/translation';

const CheckoutOrderInfo = () => {
    const cart = useSelector((state) => state.cart);

    return (
        <div className="columns small-12 medium-6 checkout-info__summary--full-row">
            <div>
                {translate('checkout.order.productstotal')}{' '}
                <span className="checkout-info__summary--expand"></span>{' '}
                {cart.orderTotal}
            </div>
            <div>
                {translate('checkout.order.discount')}{' '}
                <span className="checkout-info__summary--expand"></span>{' '}
                {cart.discount}
            </div>
            <div>
                {translate('checkout.order.deliverycost')}{' '}
                <span className="checkout-info__summary--expand"></span>{' '}
                {cart.deliveryCost}
            </div>
            <div>
                {translate('checkout.order.paymentcost')}{' '}
                <span className="checkout-info__summary--expand"></span>{' '}
                {cart.paymentCost}
            </div>
            <h3>
                {translate('checkout.order.grandTotal')}{' '}
                <span className="checkout-info__summary--expand"></span>{' '}
                {cart.grandTotal}
            </h3>
            <div>
                {translate('checkout.order.vat')}{' '}
                <span className="checkout-info__summary--expand"></span>{' '}
                {cart.vat}
            </div>
        </div>
    );
};

export default CheckoutOrderInfo;
