import {
    FACETED_SEARCH_RECEIVE,
    FACETED_SEARCH_TOGGLE_VALUE,
    FACETED_SEARCH_TOGGLE_COMPACT,
} from '../constants';
const defaultState = {
    subNavigation: null,
    sortCriteria: null,
    facetFilters: [],
    visibleDropdownMenu: [],
    navigationTheme: 'filter',
    productsViewCachedId: null,
};

export const facetedSearch = (state = defaultState, action) => {
    const { type, payload } = action;
    switch (type) {
        case FACETED_SEARCH_RECEIVE:
            return {
                ...state,
                ...payload,
            };
        case FACETED_SEARCH_TOGGLE_VALUE:
            return {
                ...state,
                facetFilters: payload.facetFilters,
            };
        case FACETED_SEARCH_TOGGLE_COMPACT: {
            const { visibleDropdownMenu } = payload;
            return {
                ...state,
                visibleDropdownMenu: state.visibleDropdownMenu.includes(
                    visibleDropdownMenu
                )
                    ? []
                    : [visibleDropdownMenu],
            };
        }
        default:
            return state;
    }
};
