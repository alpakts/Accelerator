import { NAVIGATION_RECEIVE } from '../constants';

export const navigation = (state = { menu: [] }, action) => {
    switch (action.type) {
        case NAVIGATION_RECEIVE:
            return {
                ...state,
                ...action.payload,
            };
        default:
            return state;
    }
};
