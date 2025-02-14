import { get } from '../Services/http';
import { catchError } from './Error.action';
import { NAVIGATION_LOAD_ERROR, NAVIGATION_RECEIVE } from '../constants';

export const load = () => (dispatch, getState) => {
    return get('/api/navigation')
        .then((response) => response.json())
        .then((data) => dispatch(receive(data)))
        .catch((ex) => dispatch(catchError(ex, (error) => loadError(error))));
};

export const loadError = (error) => ({
    type: NAVIGATION_LOAD_ERROR,
    payload: {
        error,
    },
});

export const receive = (data) => ({
    type: NAVIGATION_RECEIVE,
    payload: {
        contentLinks: data.contentLinks,
    },
});
