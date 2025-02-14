import {
    PERSON_RECEIVE,
    PERSON_ERROR,
    PERSON_CHANGE_MODE,
    ADDRESS_RECEIVE,
    ADDRESS_ERROR,
    ADDRESS_CHANGE_MODE,
    ORDER_RECEIVE,
    ORDER_ERROR,
    ORDER_CHANGE_MODE,
    ORDER_CHANGE_CURRENTPAGE,
    ORDER_SET_ORDER,
} from '../constants';
import { person as personReducer } from './Person.reducer';
import { address as addressReducer } from './Address.reducer';
import { order as orderReducer } from './Order.reducer';

const defaultState = {
    persons: {},
    addresses: {},
    orders: {
        currentPage: 1,
    },
};

export const myPage = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case PERSON_RECEIVE:
        case PERSON_ERROR:
        case PERSON_CHANGE_MODE:
            return {
                ...state,
                persons: personReducer(state.persons, action),
            };
        case ADDRESS_RECEIVE:
        case ADDRESS_ERROR:
        case ADDRESS_CHANGE_MODE:
            return {
                ...state,
                addresses: addressReducer(state.addresses, action),
            };
        case ORDER_RECEIVE:
        case ORDER_ERROR:
        case ORDER_CHANGE_MODE:
        case ORDER_CHANGE_CURRENTPAGE:
        case ORDER_SET_ORDER:
            return {
                ...state,
                orders: orderReducer(state.orders, action),
            };
        default:
            return state;
    }
};
