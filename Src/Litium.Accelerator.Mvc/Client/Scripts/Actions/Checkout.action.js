import { post, put, remove } from '../Services/http';
import { load as loadCart } from './Cart.action';
import { catchError } from './Error.action';

import {
    CHECKOUT_SET_SELECTED_COMPANY_ADDRESS,
    CHECKOUT_SET_PRIVATE_CUSTOMER,
    CHECKOUT_SET_SIGN_UP,
    CHECKOUT_SET_DELIVERY,
    CHECKOUT_SET_COUNTRY,
    CHECKOUT_SET_PAYMENT,
    CHECKOUT_SET_DISCOUNT_CODE,
    CHECKOUT_SET_ORDER_NOTE,
    CHECKOUT_ACCEPT_TERMS_OF_CONDITION,
    CHECKOUT_SUBMIT,
    CHECKOUT_SUBMIT_ERROR,
    CHECKOUT_SET_PAYMENT_WIDGET,
    CHECKOUT_SET_SHIPMENT_WIDGET,
    CHECKOUT_SET_USED_DISCOUNT_CODE,
    CHECKOUT_UPDATE_CUSTOMER_INFO,
    CHECKOUT_SET_SHOW_ALTERNATIVE_ADDRESS,
    CLEAR_ERROR,
    CHECKOUT_SET_STATUS_SUBMIT_BUTTON,
    CHECKOUT_VALIDATE_ADDRESS,
    CHECKOUT_SET_SHIPPING_OPTION,
} from '../constants';

export const setBusinessCustomer = (isBusinessCustomer) => ({
    type: CHECKOUT_SET_PRIVATE_CUSTOMER,
    payload: {
        isBusinessCustomer,
    },
});

export const setSignUp = (signUp) => ({
    type: CHECKOUT_SET_SIGN_UP,
    payload: {
        signUp,
    },
});

export const updateCustomerDetails = (key, data) => ({
    type: CHECKOUT_UPDATE_CUSTOMER_INFO,
    payload: {
        key,
        data,
    },
});

export const setSelectedCompanyAddress = (selectedCompanyAddressId) => ({
    type: CHECKOUT_SET_SELECTED_COMPANY_ADDRESS,
    payload: {
        selectedCompanyAddressId,
    },
});

export const setDelivery = (method) => (dispatch, getState) => {
    dispatch({
        type: CHECKOUT_SET_DELIVERY,
        payload: {
            selectedDeliveryMethod: method,
        },
    });
    const { payload } = getState().checkout;
    return put('/api/checkout/setDeliveryProvider', payload)
        .then((response) => response.json())
        .then((result) => {
            dispatch(loadCart());
            if (result !== null && result.paymentWidget) {
                dispatch(setPaymentWidget(result.paymentWidget));
            }
            dispatch(setShipmentWidget(result.shipmentWidget));
            dispatch(setSelectedShippingOption(null));
        })
        .catch((ex) => dispatch(catchError(ex, (error) => submitError(error))));
};

export const setCountry = (systemId) => (dispatch, getState) => {
    dispatch({
        type: CHECKOUT_SET_COUNTRY,
        payload: {
            selectedCountry: systemId,
        },
    });
    const { payload } = getState().checkout;

    // reset paymentWidget to null to avoid sending unnecessary request to payment provider
    dispatch(setPaymentWidget(null));

    return put('/api/checkout/setCountry', payload)
        .then((response) => response.json())
        .then((result) => {
            dispatch(loadCart());
            dispatch(setPaymentWidget(result.paymentWidget));
            dispatch({
                type: CHECKOUT_SET_DELIVERY,
                payload: {
                    deliveryMethods: result.deliveryMethods,
                    selectedDeliveryMethod: result.selectedDeliveryMethod,
                },
            });
            dispatch({
                type: CHECKOUT_SET_PAYMENT,
                payload: {
                    paymentMethods: result.paymentMethods,
                    selectedPaymentMethod: result.selectedPaymentMethod,
                },
            });
        })
        .catch((ex) => dispatch(catchError(ex, (error) => submitError(error))));
};

export const setPayment = (method) => (dispatch, getState) => {
    dispatch({
        type: CHECKOUT_SET_PAYMENT,
        payload: {
            selectedPaymentMethod: method,
        },
    });

    const { payload } = getState().checkout;
    // reset paymentWidget to null to avoid sending unnecessary request to payment provider
    dispatch(setPaymentWidget(null));
    // reset payment error on the checkout
    dispatch(
        submitError({
            modelState: {
                payment: [],
            },
        })
    );

    return put('/api/checkout/setPaymentProvider', payload)
        .then((response) => response.json())
        .then((result) => {
            dispatch(loadCart());
            dispatch(setPaymentWidget(result.paymentWidget));
        })
        .catch((ex) => dispatch(catchError(ex, (error) => submitError(error))));
};

export const reloadPayment = () => (dispatch, getState) => {
    const { payload } = getState().checkout;
    return put('/api/checkout/reloadPaymentWidget', payload)
        .then((response) => response.json())
        .then((result) => {
            if (result && result.paymentWidget) {
                dispatch(setPaymentWidget(result.paymentWidget));
            }
        })
        .catch((ex) => dispatch(catchError(ex, (error) => submitError(error))));
};

export const reloadShipment = () => (dispatch, getState) => {
    const { payload } = getState().checkout;
    return put('/api/checkout/reloadShipmentWidget', payload)
        .then((response) => response.json())
        .then((result) => {
            if (result && result.shipmentWidget) {
                dispatch(setShipmentWidget(result.shipmentWidget));
            }
        })
        .catch((ex) => dispatch(catchError(ex, (error) => submitError(error))));
};

export const setShippingOption = ({ shippingInfo, onSuccess, onError }) => (
    dispatch,
    getState
) => {
    const { payload } = getState().checkout;
    return put('/api/checkout/setShippingOption', {
        ...payload,
        shippingInfo,
    })
        .then((response) => response.json())
        .then((result) => {
            if (result?.paymentWidget) {
                dispatch(setPaymentWidget(result.paymentWidget));
            }
            dispatch(setSelectedShippingOption(shippingInfo));
            dispatch(loadCart());
            onSuccess && onSuccess();
        })
        .catch((ex) =>
            dispatch(
                catchError(ex, (error) => {
                    submitError(error);
                    onError && onError();
                })
            )
        );
};

const setSelectedShippingOption = (shippingInfo) => ({
    type: CHECKOUT_SET_SHIPPING_OPTION,
    payload: {
        shippingInfo,
    },
});

const setPaymentWidget = (paymentWidget) => ({
    type: CHECKOUT_SET_PAYMENT_WIDGET,
    payload: {
        paymentWidget,
    },
});

const setShipmentWidget = (shipmentWidget) => ({
    type: CHECKOUT_SET_SHIPMENT_WIDGET,
    payload: {
        shipmentWidget,
    },
});

export const setOrderNote = (orderNote) => ({
    type: CHECKOUT_SET_ORDER_NOTE,
    payload: {
        orderNote,
    },
});

export const acceptTermsOfCondition = (acceptTermsOfCondition) => ({
    type: CHECKOUT_ACCEPT_TERMS_OF_CONDITION,
    payload: {
        acceptTermsOfCondition,
    },
});

export const setDiscountCode = (discountCode) => ({
    type: CHECKOUT_SET_DISCOUNT_CODE,
    payload: {
        discountCode,
    },
});

export const submitDiscountCode = () => (dispatch, getState) => {
    const { payload } = getState().checkout;
    return put('/api/checkout/setDiscountCode', payload)
        .then((response) => response.json())
        .then((result) => {
            dispatch(setUsedDiscountCodes(result.usedDiscountCodes));
            dispatch(loadCart());
            if (result?.shipmentWidget) {
                dispatch(setShipmentWidget(result.shipmentWidget));
            }
            dispatch(setPaymentWidget(result.paymentWidget));
            // reset error of campaign code
            dispatch(
                submitError({
                    modelState: {
                        discountCode: [],
                    },
                })
            );
        })
        .catch((ex) => {
            dispatch(catchError(ex, (error) => submitError(error)));
            // restore the initial cart
            dispatch(loadCart());
        });
};

export const deleteDiscountCode = (discountCode) => (dispatch, getState) => {
    const { payload } = getState().checkout;
    payload.discountCode = discountCode;
    return remove('/api/checkout/deleteDiscountCode', payload)
        .then((response) => response.json())
        .then((result) => {
            dispatch(setUsedDiscountCodes(result.usedDiscountCodes));
            dispatch(loadCart());
            if (result?.shipmentWidget) {
                dispatch(setShipmentWidget(result.shipmentWidget));
            }
            dispatch(setPaymentWidget(result.paymentWidget));
            dispatch(setDiscountCode(''));
            // reset error of campaign code
            dispatch(
                submitError({
                    modelState: {
                        discountCode: [],
                    },
                })
            );
        })
        .catch((ex) => {
            dispatch(catchError(ex, (error) => submitError(error)));
            // restore the initial cart
            dispatch(loadCart());
        });
};

const setUsedDiscountCodes = (usedDiscountCodes) => ({
    type: CHECKOUT_SET_USED_DISCOUNT_CODE,
    payload: {
        usedDiscountCodes,
    },
});

export const submit = () => (dispatch, getState) => {
    const { payload } = getState().checkout;
    return _submit('/api/checkout', payload, dispatch);
};

const _submit = (url, model, dispatch) => {
    return post(url, model)
        .then((response) => response.json())
        .then((result) => {
            dispatch(submitDone(result));
        })
        .catch((ex) => {
            if (ex.response) {
                ex.response.json().then((error) => {
                    dispatch(submitError(error));
                    dispatch(submitDone(null));
                    // reload the cart, it might be changed after validation
                    dispatch(loadCart());
                });
            } else {
                dispatch(submitError(ex));
            }
            dispatch(setStatusSubmitButton(true));
        });
};

export const saveCustomerDetail = (data) => (dispatch, getState) => {
    const { payload } = getState().checkout;
    const updatedData = { ...payload, ...data };
    return put('/api/checkout/setCustomerDetail', updatedData)
        .then((response) => response.json())
        .then(() => dispatch(setStatusSubmitButton(true)))
        .catch((ex) => {
            if (ex.response) {
                ex.response.json().then((error) => {
                    dispatch(submitError(error));
                    dispatch(setStatusSubmitButton(false));
                });
            } else {
                dispatch(submitError(ex));
            }
            throw ex;
        });
};

export const clearError = () => ({
    type: CLEAR_ERROR,
    payload: {
        errors: [],
    },
});

export const submitDone = (result) => ({
    type: CHECKOUT_SUBMIT,
    payload: {
        result,
    },
});

export const submitError = (error) => ({
    type: CHECKOUT_SUBMIT_ERROR,
    payload: {
        error,
    },
});

export const setStatusSubmitButton = (value) => ({
    type: CHECKOUT_SET_STATUS_SUBMIT_BUTTON,
    payload: {
        enableConfirmButton: value,
    },
});

export const setShowAlternativeAddress = (showAlternativeAddress) => ({
    type: CHECKOUT_SET_SHOW_ALTERNATIVE_ADDRESS,
    payload: {
        showAlternativeAddress,
    },
});
export const setValidateStatus = (isValidating) => ({
    type: CHECKOUT_VALIDATE_ADDRESS,
    payload: {
        isValidating,
    },
});
