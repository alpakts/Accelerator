import {
    PERSON_RECEIVE,
    PERSON_ERROR,
    PERSON_CHANGE_MODE,
    ViewMode,
} from '../constants';
import { error as errorReducer } from './Error.reducer';

const defaultState = {
    list: [],
    mode: ViewMode.List,
    errors: {},
};

export const person = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case PERSON_RECEIVE:
        case PERSON_CHANGE_MODE:
            return {
                ...state,
                errors: {},
                ...payload,
            };
        case PERSON_ERROR:
            return {
                ...state,
                errors: errorReducer(state.errors, action),
            };
        default:
            return state;
    }
};
