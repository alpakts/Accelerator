import {
    ADDRESS_RECEIVE,
    ADDRESS_ERROR,
    ADDRESS_CHANGE_MODE,
} from '../constants';
import { ViewMode } from '../constants';
import { error as errorReducer } from './Error.reducer';

const defaultState = {
    list: [],
    mode: ViewMode.List,
    errors: {},
};

export const address = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case ADDRESS_RECEIVE:
        case ADDRESS_CHANGE_MODE:
            return {
                ...state,
                errors: {},
                ...payload,
            };
        case ADDRESS_ERROR:
            return {
                ...state,
                errors: errorReducer(state.errors, action),
            };
        default:
            return state;
    }
};
