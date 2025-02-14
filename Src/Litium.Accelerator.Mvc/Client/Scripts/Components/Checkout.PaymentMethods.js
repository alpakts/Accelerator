import React, { Fragment } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { setPayment } from '../Actions/Checkout.action';
import { translate } from '../Services/translation';

const CheckoutPaymentMethods = ({ errors }) => {
    const dispatch = useDispatch();
    const { payload } = useSelector((state) => state.checkout);
    const { paymentMethods, selectedPaymentMethod } = payload;

    return (
        <Fragment>
            <div className="row">
                <h3 className="checkout__section-title">
                    {translate('checkout.payment.title')}
                </h3>
            </div>
            {paymentMethods && paymentMethods.length > 0 && (
                <section className="row checkout-info__container">
                    <div className="columns small-12">
                        {paymentMethods.map((method) => (
                            <label className="row no-margin" key={method.id}>
                                <input
                                    type="radio"
                                    name="paymentMethods"
                                    className="checkout-info__checkbox-radio"
                                    value={method.id}
                                    checked={
                                        method.id === selectedPaymentMethod.id
                                    }
                                    onChange={() =>
                                        dispatch(setPayment(method))
                                    }
                                />
                                <span className="columns">
                                    <b> {method.name} </b> -{' '}
                                    {method.formattedPrice}
                                </span>
                            </label>
                        ))}
                    </div>
                </section>
            )}
            <div className="row">
                {errors['selectedPaymentMethod'] && (
                    <span className="form__validator--error form__validator--top-narrow">
                        {errors['selectedPaymentMethod'][0]}
                    </span>
                )}
            </div>
        </Fragment>
    );
};

export default CheckoutPaymentMethods;
