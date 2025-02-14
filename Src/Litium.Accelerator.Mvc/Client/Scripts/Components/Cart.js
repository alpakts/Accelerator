import React, { useState, Fragment, useCallback, useEffect } from 'react';
import { translate } from '../Services/translation';
import { useDispatch, useSelector } from 'react-redux';
import { reloadPayment, reloadShipment } from '../Actions/Checkout.action';
import { update, loadError } from '../Actions/Cart.action';
import { debounce } from 'lodash-es';
import { catchError } from '../Actions/Error.action';

const mapCartToAbort = {};

const quantityDispatch = debounce((articleNumber, quantity, dispatch) => {
    mapCartToAbort[articleNumber] = new AbortController();
    dispatch(update(articleNumber, quantity, mapCartToAbort[articleNumber]))
        .then(() => {
            dispatch(reloadShipment());
            dispatch(reloadPayment());
        })
        .catch((ex) => dispatch(catchError(ex, (error) => loadError(error))));
}, 500);

const Cart = () => {
    const dispatch = useDispatch();
    const [removingRow, setRemovingRow] = useState({});
    const cart = useSelector((state) => state.cart);
    const [orderRows, setOrderRows] = useState(cart?.orderRows);
    const { discountRows } = cart;

    useEffect(() => {
        setOrderRows(cart?.orderRows);
    }, [cart]);

    const removeOrderRow = useCallback(
        (articleNumber) =>
            dispatch(update(articleNumber, 0)).then(() => {
                dispatch(reloadShipment());
                dispatch(reloadPayment());
            }),
        [dispatch]
    );

    const handleQuantityInput = useCallback(
        (articleNumber, quantity, rowSystemId, ensureCorrectness = true) => {
            // In case of onBlur event, we need ensureCorrectness as true in order to ensure if the value is a valid float number
            // In case of onChange event, we accept invalid float number, but we don't send any request to the server.
            let floatQuantity = parseFloat(quantity);
            let validValue = !isNaN(floatQuantity) && floatQuantity > 0;
            if (ensureCorrectness) {
                quantity = validValue ? floatQuantity : 1;
                floatQuantity = quantity;
                validValue = true;
            }
            const index = orderRows.findIndex(
                (item) => item.rowSystemId === rowSystemId
            );
            const oldQuantity = parseFloat(orderRows[index].quantity);
            if (floatQuantity !== oldQuantity) {
                mapCartToAbort[articleNumber] &&
                    mapCartToAbort[articleNumber].abort();
                quantityDispatch && quantityDispatch.cancel();
                validValue &&
                    quantityDispatch(articleNumber, quantity, dispatch);
            }
            const tempOrderRows = [...orderRows];
            tempOrderRows[index] = {
                ...tempOrderRows[index],
                quantity,
            };
            setOrderRows(tempOrderRows);
        },
        [dispatch, orderRows]
    );

    const ProductImage = useCallback(({ order }) => {
        return (
            <img
                className="checkout-cart__image"
                src={order.image}
                alt={order.name}
            />
        );
    }, []);

    const ProductName = useCallback(({ order }) => {
        return (
            <Fragment>
                <a href={order.url}>{order.name}</a>
                <span className="checkout-cart__brand-name">{order.brand}</span>
            </Fragment>
        );
    }, []);

    const ProductPrice = useCallback(({ order }) => {
        return (
            <Fragment>
                {order.isFreeGift ? order.campaignPrice : order.price}
            </Fragment>
        );
    }, []);

    const ProductQuantity = useCallback(
        (order) => {
            return (
                <Fragment>
                    {order.isFreeGift ? (
                        <div>{order.quantity}</div>
                    ) : (
                        <input
                            className="checkout-cart__input"
                            type="number"
                            min="1"
                            maxLength={3}
                            value={order.quantity.toString()}
                            onChange={(event) =>
                                handleQuantityInput(
                                    order.articleNumber,
                                    event.target.value,
                                    order.rowSystemId,
                                    false
                                )
                            }
                            onBlur={(event) =>
                                handleQuantityInput(
                                    order.articleNumber,
                                    event.target.value,
                                    order.rowSystemId
                                )
                            }
                        />
                    )}
                </Fragment>
            );
        },
        [handleQuantityInput]
    );

    const ProductTotalPrice = ({ order }) => {
        return (
            <Fragment>
                {order.isFreeGift ? order.totalCampaignPrice : order.totalPrice}
            </Fragment>
        );
    };

    const RemoveBtn = useCallback(
        ({ order }) => {
            return (
                <Fragment>
                    {!order.isFreeGift && !removingRow[order.rowSystemId] && (
                        <a
                            className="table__icon table__icon--delete"
                            onClick={() =>
                                setRemovingRow({
                                    ...removingRow,
                                    [order.rowSystemId]: true,
                                })
                            }
                            title={translate('general.remove')}
                        ></a>
                    )}
                    {!order.isFreeGift && removingRow[order.rowSystemId] && (
                        <Fragment>
                            <a
                                className="table__icon table__icon--accept"
                                onClick={() =>
                                    removeOrderRow(order.articleNumber)
                                }
                                title={translate('general.ok')}
                            ></a>
                            <a
                                className="table__icon table__icon--cancel"
                                onClick={() =>
                                    setRemovingRow({
                                        ...removingRow,
                                        [order.rowSystemId]: false,
                                    })
                                }
                                title={translate('general.cancel')}
                            ></a>
                        </Fragment>
                    )}
                </Fragment>
            );
        },
        [removeOrderRow, removingRow]
    );

    const CartTotal = useCallback(({ cart }) => {
        return (
            <h3 className="text--right">
                {translate('checkout.cart.total')}: {cart.orderTotal}
            </h3>
        );
    }, []);

    return (
        <div className="row checkout__container">
            <div className="small-12 simple-table hide-for-small-only">
                <div className="row small-unstack no-margin">
                    <div className="columns small-12 medium-4 large-5"></div>
                    <div className="columns small-3 medium-2 large-2">
                        {translate('checkout.cart.header.price')}
                    </div>
                    <div className="columns small-4 medium-2 large-2">
                        {translate('checkout.cart.header.quantity')}
                    </div>
                    <div className="columns small-5 medium-3 large-3">
                        {translate('checkout.cart.header.total')}
                    </div>
                </div>
                {orderRows.map((order) => (
                    <div
                        className="row small-unstack no-margin checkout-cart__row"
                        key={order.rowSystemId}
                    >
                        <div className="columns small-12 medium-4 large-5 checkout-cart__image-container">
                            <div className="checkout-cart__image-wrapper">
                                <ProductImage order={order} />
                            </div>
                            <div className="checkout-cart__image-info">
                                <ProductName order={order} />
                            </div>
                        </div>
                        <div className="columns small-3 medium-2 large-2 simple-table__cell--no-break-word">
                            <ProductPrice order={order} />
                        </div>
                        <div className="columns small-2 medium-2 large-2">
                            {ProductQuantity(order)}
                        </div>
                        <div className="checkout-cart__total-price columns small-2 medium-3 large-2 simple-table__cell--no-break-word">
                            <ProductTotalPrice order={order} />
                        </div>
                        <div className="columns small-3 medium-1 large-1">
                            <RemoveBtn order={order} />
                        </div>
                    </div>
                ))}
                {discountRows.map((order) => (
                    <div
                        className="row small-unstack no-margin checkout-cart__row"
                        key={order.rowSystemId}
                    >
                        <div className="columns small-12 medium-4 large-5 checkout-cart__image-container">
                            <div className="checkout-cart__discount-info checkout-cart__discount-row">
                                <ProductName order={order} />
                            </div>
                        </div>
                        <div className="columns small-3 medium-2 large-2 simple-table__cell--no-break-word"></div>
                        <div className="columns small-2 medium-2 large-2"></div>
                        <div className="checkout-cart__discount-price columns small-2 medium-3 large-2 simple-table__cell--no-break-word">
                            <ProductTotalPrice order={order} />
                        </div>
                        <div className="columns small-3 medium-1 large-1"></div>
                    </div>
                ))}
                <div className="row small-unstack no-margin checkout-cart__row">
                    <div className="columns">
                        <CartTotal cart={cart} />
                    </div>
                </div>
            </div>
            <div className="small-12 simple-table checkout-mobile show-for-small-only">
                {orderRows.map((order) => (
                    <div className="row no-margin" key={order.rowSystemId}>
                        <div className="columns small-3">
                            <ProductImage order={order} />
                        </div>
                        <div className="columns small-9">
                            <div className="row">
                                <div className="small-8 columns">
                                    <div className="flex-container flex-dir-column align-center">
                                        <ProductName order={order} />
                                    </div>
                                </div>
                                <div className="small-4 columns flex-container align-right">
                                    <RemoveBtn order={order} />
                                </div>
                            </div>
                            <div className="row">
                                <div className="small-12 columns flex-container align-justify">
                                    <div>
                                        <ProductPrice order={order} />
                                    </div>
                                    <div>{ProductQuantity(order)}</div>
                                </div>
                            </div>
                            <div className="row">
                                <div className="checkout-cart__total-price small-12 columns text--right">
                                    <ProductTotalPrice order={order} />
                                </div>
                            </div>
                        </div>
                    </div>
                ))}
                {discountRows.map((order) => (
                    <div className="row no-margin" key={order.rowSystemId}>
                        <div className="columns">
                            <div className="checkout-cart__discount-row">
                                <ProductName order={order} />
                            </div>
                            <div className="checkout-cart__discount-price text--right">
                                <ProductTotalPrice order={order} />
                            </div>
                        </div>
                    </div>
                ))}
                <div className="row no-margin">
                    <div className="columns">
                        <CartTotal cart={cart} />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Cart;
