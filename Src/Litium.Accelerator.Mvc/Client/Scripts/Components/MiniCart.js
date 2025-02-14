import React from 'react';
import { translate } from '../Services/translation';
import { useSelector, useDispatch } from 'react-redux';
import { toggle } from '../Actions/Cart.action';

const MiniCart = () => {
    const dispatch = useDispatch();
    const { quantity, orderTotal, checkoutUrl, showInfo } = useSelector(
        (state) => state.cart
    );
    const onToggle = () => dispatch(toggle());

    return (
        <div className="cart cart--mini">
            <a
                className="cart__link--block"
                rel="nofollow"
                href="#"
                onClick={(e) => {
                    e.preventDefault();
                    onToggle();
                }}
            >
                <i className="cart__icon">
                    <span className="cart__quantity">{quantity}</span>
                </i>
                <span className="cart__title hide">
                    {translate('minicart.checkout')}
                </span>
            </a>
            <div
                className={`cart__info ${
                    !showInfo ? 'cart__info--hidden' : ''
                }`}
            >
                <span
                    className="cart__close-button"
                    onClick={() => onToggle()}
                ></span>
                <p className="cart__info-row">
                    {quantity} {translate('minicart.numberofproduct')}
                </p>
                <p className="cart__info-row">
                    <b>{translate('minicart.total')}</b> {orderTotal}
                </p>
                <a href={checkoutUrl} className="cart__checkout-button">
                    {translate('minicart.checkout')}
                </a>
            </div>
        </div>
    );
};

export default MiniCart;
