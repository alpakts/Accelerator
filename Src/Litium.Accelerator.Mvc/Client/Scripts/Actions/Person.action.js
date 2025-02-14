import { ViewMode } from '../constants';
import { get, post, put, remove as httpRemove } from '../Services/http';
import { catchError } from './Error.action';
import { PERSON_RECEIVE, PERSON_ERROR, PERSON_CHANGE_MODE } from '../constants';

const rootRoute = '/api/mypageperson';

export const changeMode = (mode) => ({
    type: PERSON_CHANGE_MODE,
    payload: {
        mode,
    },
});

export const query = (mode = ViewMode.List) => (dispatch) => {
    return get(rootRoute)
        .then((response) => response.json())
        .then((result) => {
            dispatch(receive(result, mode));
        })
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));
};

export const remove = (personSystemId) => (dispatch) =>
    httpRemove(rootRoute, personSystemId)
        .then(() => dispatch(query()))
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));

export const add = (person) => (dispatch) =>
    post(rootRoute, person)
        .then(() => dispatch(query()))
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));

export const edit = (person) => (dispatch) =>
    put(rootRoute, person)
        .then(() => dispatch(query()))
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));

const receive = (list, mode) => ({
    type: PERSON_RECEIVE,
    payload: {
        list,
        mode,
    },
});

export const setError = (error) => ({
    type: PERSON_ERROR,
    payload: {
        error,
    },
});
