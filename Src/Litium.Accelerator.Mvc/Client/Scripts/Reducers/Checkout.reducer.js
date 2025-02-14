import {
    CHECKOUT_SUBMIT,
    CHECKOUT_SUBMIT_ERROR,
    CHECKOUT_SET_DISCOUNT_CODE,
    CHECKOUT_SET_DELIVERY,
    CHECKOUT_SET_PAYMENT,
    CHECKOUT_SET_ORDER_NOTE,
    CHECKOUT_ACCEPT_TERMS_OF_CONDITION,
    CHECKOUT_SET_PRIVATE_CUSTOMER,
    CHECKOUT_SET_PAYMENT_WIDGET,
    CHECKOUT_SET_SHIPMENT_WIDGET,
    CHECKOUT_SET_SELECTED_COMPANY_ADDRESS,
    CHECKOUT_SET_SIGN_UP,
    CHECKOUT_SET_COUNTRY,
    CHECKOUT_SET_USED_DISCOUNT_CODE,
    CHECKOUT_UPDATE_CUSTOMER_INFO,
    CHECKOUT_SET_SHOW_ALTERNATIVE_ADDRESS,
    CLEAR_ERROR,
    CHECKOUT_SET_STATUS_SUBMIT_BUTTON,
    CHECKOUT_VALIDATE_ADDRESS,
    CHECKOUT_SET_SHIPPING_OPTION,
} from '../constants';
import { error as errorReducer } from './Error.reducer';

const defaultState = {
    payload: {
        alternativeAddress: {},
        customerDetails: {},
        selectedCompanyAddressId: null,
        selectedDeliveryMethod: {},
        selectedPaymentMethod: {},
        selectedCountry: {},
        discountCode: '',
        orderNote: {},
        paymentWidget: null,
        shipmentWidget: null,
        shippingInfo: '',
        isBusinessCustomer: false,
        signUp: false,
        acceptTermsOfCondition: false,

        authenticated: false,
        deliveryMethods: [],
        paymentMethods: [],
        companyAddresses: [],
        responseUrl: '',
        cancelUrl: '',
        redirectUrl: '',
        showAlternativeAddress: false,
    },
    errors: {},
    result: {},
    enableConfirmButton: false,
};
export const checkout = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case CHECKOUT_SUBMIT_ERROR:
            return {
                ...state,
                errors: errorReducer(state.errors, action),
            };
        case CHECKOUT_SUBMIT:
        case CHECKOUT_SET_STATUS_SUBMIT_BUTTON:
        case CLEAR_ERROR:
            return {
                ...state,
                ...payload,
            };
        case CHECKOUT_UPDATE_CUSTOMER_INFO:
            return {
                ...state,
                payload: {
                    ...state.payload,
                    [payload.key]: {
                        ...state.payload[payload.key],
                        ...payload.data,
                    },
                },
            };
        case CHECKOUT_SET_DELIVERY:
        case CHECKOUT_SET_PAYMENT:
        case CHECKOUT_SET_ORDER_NOTE:
        case CHECKOUT_SET_PAYMENT_WIDGET:
        case CHECKOUT_SET_SHIPMENT_WIDGET:
        case CHECKOUT_SET_SHIPPING_OPTION:
        case CHECKOUT_SET_PRIVATE_CUSTOMER:
        case CHECKOUT_SET_SIGN_UP:
        case CHECKOUT_SET_SELECTED_COMPANY_ADDRESS:
        case CHECKOUT_ACCEPT_TERMS_OF_CONDITION:
        case CHECKOUT_SET_DISCOUNT_CODE:
        case CHECKOUT_SET_COUNTRY:
        case CHECKOUT_SET_USED_DISCOUNT_CODE:
        case CHECKOUT_SET_SHOW_ALTERNATIVE_ADDRESS:
        case CHECKOUT_VALIDATE_ADDRESS:
            return {
                ...state,
                payload: {
                    ...state.payload,
                    ...payload,
                },
            };
        default:
            return state;
    }
};
