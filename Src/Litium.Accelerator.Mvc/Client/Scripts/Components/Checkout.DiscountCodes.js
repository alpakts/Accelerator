import React, { Fragment, useEffect, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { translate } from '../Services/translation';
import {
    setDiscountCode,
    submitDiscountCode,
    deleteDiscountCode,
} from '../Actions/Checkout.action';

const CheckoutPaymentMethods = () => {
    const dispatch = useDispatch();
    const { payload, errors = {} } = useSelector((state) => state.checkout);
    const { usedDiscountCodes } = payload;
    const discountCodeEl = useRef(null);

    useEffect(() => {
        if (errors && !errors['discountCode']?.length) {
            discountCodeEl.current.value = '';
        }
    }, [errors]);

    return (
        <Fragment>
            <div className="row">
                <h3 className="checkout__section-title">
                    {translate('checkout.discountcode')}
                </h3>
            </div>
            <section className="row checkout-info__container">
                <div className="columns small-12">
                    <div className="row no-margin">
                        <div className="small-6 medium-4">
                            <input
                                ref={discountCodeEl}
                                className="form__input"
                                id="campaign-code"
                                placeholder={translate('checkout.discountcode')}
                                onChange={(event) =>
                                    dispatch(
                                        setDiscountCode(event.target.value)
                                    )
                                }
                            />
                            {errors && errors['discountCode'] && (
                                <span
                                    className="form__validator--error form__validator--top-narrow"
                                    data-error-for="campaign-code"
                                >
                                    {errors['discountCode'][0]}
                                </span>
                            )}
                        </div>
                        <div className="small-5 medium-4 columns">
                            <button
                                className="checkout-info__campaign-button"
                                onClick={() => dispatch(submitDiscountCode())}
                            >
                                {translate('checkout.usediscountcode')}
                            </button>
                        </div>
                    </div>
                    <div className="row no-margin">
                        <div className="chip__container">
                            {usedDiscountCodes &&
                                usedDiscountCodes.map((discountCode) => (
                                    <div className="chip" key={discountCode}>
                                        <span className="chip__label checkout-discount-codes">
                                            {discountCode}
                                        </span>
                                        <i className="chip__icon chip__icon--check"></i>
                                        <i
                                            className="chip__icon chip__icon--delete"
                                            onClick={() =>
                                                dispatch(
                                                    deleteDiscountCode(
                                                        discountCode
                                                    )
                                                )
                                            }
                                            title={translate('general.remove')}
                                        ></i>
                                    </div>
                                ))}
                        </div>
                    </div>
                </div>
            </section>
        </Fragment>
    );
};

export default CheckoutPaymentMethods;
