import {
    QUICK_SEARCH_QUERY,
    QUICK_SEARCH_RECEIVE,
    QUICK_SEARCH_SHOW_FULL_FORM,
    QUICK_SEARCH_SELECT_ITEM,
} from '../constants';

const DEFAULT_STATE = {
    query: '',
    result: [],
    showResult: false,
    showFullForm: false,
    selectedItem: -1,
};

export const quickSearch = (state = DEFAULT_STATE, action) => {
    switch (action.type) {
        case QUICK_SEARCH_QUERY:
        case QUICK_SEARCH_RECEIVE:
        case QUICK_SEARCH_SHOW_FULL_FORM:
        case QUICK_SEARCH_SELECT_ITEM:
            return {
                ...state,
                ...action.payload,
            };
        default:
            return state;
    }
};
