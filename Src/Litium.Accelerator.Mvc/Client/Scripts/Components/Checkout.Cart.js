import React, { Fragment } from 'react';
import Cart from './Cart';
import { translate } from '../Services/translation';

const CheckoutCart = ({ errors }) => {
    return (
        <Fragment>
            <div className="row">
                <div className="small-12">
                    <h2 className="checkout__title">
                        {translate('checkout.title')}
                    </h2>
                </div>
            </div>
            <div className="row">
                <h3 className="checkout__section-title">
                    {translate('checkout.cart.title')}
                </h3>
            </div>
            <div className="row">
                {errors && errors['cart'] && (
                    <p className="checkout__validator--error">
                        {errors['cart'][0]}
                    </p>
                )}
            </div>
            <Cart />
        </Fragment>
    );
};

export default CheckoutCart;
