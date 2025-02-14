import { get, put } from '../Services/http';
import { catchError } from './Error.action';
import { CART_LOAD_ERROR, CART_RECEIVE, CART_SHOW_INFO } from '../constants';

export const load = () => (dispatch, getState) => {
    return get('/api/cart')
        .then((response) => response.json())
        .then((cart) => {
            dispatch(receive(cart));
        })
        .catch((ex) => dispatch(catchError(ex, (error) => loadError(error))));
};

export const loadError = (error) => ({
    type: CART_LOAD_ERROR,
    payload: {
        error,
    },
});

export const receive = (cart) => ({
    type: CART_RECEIVE,
    payload: cart,
});

export const toggle = () => (dispatch, getState) => {
    dispatch(show(!getState().cart.showInfo));
};

const show = (visible) => ({
    type: CART_SHOW_INFO,
    payload: {
        showInfo: visible,
    },
});

export const update = (articleNumber, quantity, abortController = null) => (
    dispatch
) => {
    return put(`/api/cart/update`, { articleNumber, quantity }, abortController)
        .then((response) => response.json())
        .then((cart) => dispatch(receive(cart)))
        .catch((ex) => {
            dispatch(catchError(ex, (error) => loadError(error)));
            throw ex;
        });
};
