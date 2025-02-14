import React, { Fragment } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { setDelivery } from '../Actions/Checkout.action';
import { translate } from '../Services/translation';

const CheckoutDeliveryMethods = ({ errors }) => {
    const dispatch = useDispatch();
    const { deliveryMethods, selectedDeliveryMethod } = useSelector(
        (state) => state.checkout.payload
    );

    return (
        <Fragment>
            <div className="row">
                <h3 className="checkout__section-title">
                    {translate('checkout.delivery.title')}
                </h3>
            </div>
            {deliveryMethods && deliveryMethods.length > 0 && (
                <section className="row checkout-info__container">
                    <div className="columns small-12">
                        {deliveryMethods.map((method) => (
                            <label className="row no-margin" key={method.id}>
                                <input
                                    type="radio"
                                    name="deliveryMethods"
                                    className="checkout-info__checkbox-radio"
                                    value={method.id}
                                    checked={
                                        method.id === selectedDeliveryMethod?.id
                                    }
                                    onChange={() =>
                                        dispatch(setDelivery(method))
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
                {errors['selectedDeliveryMethod'] && (
                    <span className="form__validator--error form__validator--top-narrow">
                        {errors['selectedDeliveryMethod'][0]}
                    </span>
                )}
            </div>
        </Fragment>
    );
};

export default CheckoutDeliveryMethods;
