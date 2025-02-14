import { ViewMode, PaginationOptions } from '../constants';
import { get, post } from '../Services/http';
import { catchError } from './Error.action';

import {
    ORDER_RECEIVE,
    ORDER_ERROR,
    ORDER_CHANGE_MODE,
    ORDER_CHANGE_CURRENTPAGE,
    ORDER_SET_ORDER,
} from '../constants';

const rootRoute = '/api/order';

export const changeMode = (mode) => ({
    type: ORDER_CHANGE_MODE,
    payload: {
        mode,
    },
});

export const query = (
    pageIndex = 1,
    showOnlyMyOrders = false,
    pageSize = PaginationOptions.PageSize,
    orderId = null,
    showOrderDetail = false
) => (dispatch) => {
    return get(
        `${rootRoute}?pageIndex=${pageIndex}&showMyOrders=${showOnlyMyOrders}&pageSize=${pageSize}`
    )
        .then((response) => response.json())
        .then((result) => {
            dispatch(
                receive(
                    result.orders,
                    result.totalCount,
                    pageIndex,
                    showOnlyMyOrders,
                    showOrderDetail ? ViewMode.Detail : ViewMode.List
                )
            );
            if (orderId && showOrderDetail) {
                const order = result.orders.find(
                    (order) => order.orderId === orderId
                );
                dispatch(setOrder(order || {}));
            }
        })
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));
};

export const approveOrder = (orderId, callback) => (dispatch) => {
    return post(`${rootRoute}/approveOrder`, {
        id: orderId,
    })
        .then((response) => response.json())
        .then((result) => {
            callback && callback(result);
        })
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));
};

export const getOrder = (orderId) => (dispatch) => {
    return get(`${rootRoute}/${orderId}`)
        .then((response) => response.json())
        .then((result) => {
            dispatch(setOrder(result));
        })
        .catch((ex) => dispatch(catchError(ex, (error) => setError(error))));
};

const receive = (
    list,
    totalCount,
    currentPage,
    showOnlyMyOrders,
    mode = ViewMode.List
) => ({
    type: ORDER_RECEIVE,
    payload: {
        list,
        mode,
        totalCount,
        currentPage,
        showOnlyMyOrders,
    },
});

export const changeCurrentPage = (currentPage) => ({
    type: ORDER_CHANGE_CURRENTPAGE,
    payload: {
        currentPage,
    },
});

export const setError = (error) => ({
    type: ORDER_ERROR,
    payload: {
        error,
    },
});

export const setOrder = (order) => ({
    type: ORDER_SET_ORDER,
    payload: {
        order,
    },
});
