import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { translate } from '../Services/translation';
import { setOrderNote } from '../Actions/Checkout.action';

const CheckoutOrderNote = () => {
    const dispatch = useDispatch();
    const orderNote = useSelector((state) => state.checkout.payload.orderNote);

    return (
        <div className="columns small-12 medium-6 checkout-info__summary--full-height">
            {translate('checkout.order.message')}
            <textarea
                className="form__input checkout-info__messages"
                value={orderNote}
                onChange={(event) => dispatch(setOrderNote(event.target.value))}
            ></textarea>
        </div>
    );
};

export default CheckoutOrderNote;
