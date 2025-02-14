import { post } from '../Services/http';
import { catchError } from './Error.action';
import {
    QUICK_SEARCH_QUERY,
    QUICK_SEARCH_RECEIVE,
    QUICK_SEARCH_ERROR,
    QUICK_SEARCH_SHOW_FULL_FORM,
    QUICK_SEARCH_SELECT_ITEM,
} from '../constants';

let abortController;

export const query = (q) => (dispatch, getState) => {
    // abort any existing, pending request.
    // It's ok to call .abort() after the fetch has already completed, fetch simply ignores it.
    abortController && abortController.abort();
    abortController = new AbortController();
    return post('/api/quickSearch', q, abortController)
        .then((response) => response.json())
        .then((result) => dispatch(receive(result)))
        .catch((ex) => dispatch(catchError(ex, (error) => searchError(error))));
};

export const setSearchQuery = (query) => ({
    type: QUICK_SEARCH_QUERY,
    payload: {
        query,
    },
});

export const searchError = (error) => ({
    type: QUICK_SEARCH_ERROR,
    payload: {
        error,
    },
});

export const receive = (result) => ({
    type: QUICK_SEARCH_RECEIVE,
    payload: {
        result,
        showResult: result && result.length > 0,
    },
});

export const toggleShowFullForm = () => (dispatch, getState) => {
    dispatch(show(!getState().quickSearch.showFullForm));
};

const show = (visible) => ({
    type: QUICK_SEARCH_SHOW_FULL_FORM,
    payload: {
        showFullForm: visible,
    },
});

export const handleKeyDown = (event, opt) => (dispatch, getState) => {
    const { result, selectedItem } = getState().quickSearch;

    if (!result || !result.length) {
        return;
    }
    const max = result.length - 1,
        clip = (index) => (index < 0 ? max : index > max ? 0 : index);
    switch (event.keyCode) {
        case 38:
            dispatch(selectItem(clip(selectedItem - 1)));
            break;
        case 40:
            dispatch(selectItem(clip(selectedItem + 1)));
            break;
        case 13:
            const selectedObject = result[selectedItem];
            if (selectedObject && selectedObject.url) {
                location.href = selectedObject.url;
            } else {
                location.href = opt.searchUrl;
            }
            break;
        default:
            break;
    }
};

export const handleClickSearch = (opt) => (dispatch, getState) => {
    const { result } = getState().quickSearch;

    if (!result || !result.length) {
        return;
    }
    location.href = opt.searchUrl;
};

const selectItem = (selectedItem) => ({
    type: QUICK_SEARCH_SELECT_ITEM,
    payload: {
        selectedItem,
    },
});
