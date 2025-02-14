import {
    ORDER_RECEIVE,
    ORDER_ERROR,
    ORDER_CHANGE_MODE,
    ORDER_CHANGE_CURRENTPAGE,
    ORDER_SET_ORDER,
    ViewMode,
} from '../constants';
import { error as errorReducer } from './Error.reducer';

const defaultState = {
    mode: ViewMode.List,
    list: [],
    order: {},
    totalCount: 0,
    showOnlyMyOrders: false,
    errors: {},
};

export const order = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case ORDER_RECEIVE:
        case ORDER_CHANGE_MODE:
        case ORDER_CHANGE_CURRENTPAGE:
        case ORDER_SET_ORDER:
            return {
                ...state,
                errors: {},
                ...payload,
            };
        case ORDER_ERROR:
            return {
                ...state,
                errors: errorReducer(state.errors, action),
            };
        default:
            return state;
    }
};
