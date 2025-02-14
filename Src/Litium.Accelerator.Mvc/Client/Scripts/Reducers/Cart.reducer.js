import { CART_RECEIVE, CART_SHOW_INFO } from '../constants';

export const cart = (
    state = { quantity: 0, orderTotal: 0, showInfo: false },
    action
) => {
    switch (action.type) {
        case CART_RECEIVE:
        case CART_SHOW_INFO:
            return {
                ...state,
                ...action.payload,
            };
        default:
            return state;
    }
};
