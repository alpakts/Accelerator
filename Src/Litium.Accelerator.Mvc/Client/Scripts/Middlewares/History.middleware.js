import { FACETED_SEARCH_QUERY } from '../constants';

export const historyMiddleware = (store) => (next) => (action) => {
    const { type, payload } = action;
    switch (type) {
        case FACETED_SEARCH_QUERY:
            let { query } = payload;
            const url =
                window.location.href.replace(window.location.search, '') +
                `${query ? '?' : ''}${query}`;
            window.history.pushState('search', 'Search Page', url);
            break;
    }
    next(action);
};
