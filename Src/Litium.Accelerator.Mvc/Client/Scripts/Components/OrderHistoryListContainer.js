import React, { Fragment, useEffect, useState, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    query,
    changeMode,
    changeCurrentPage,
    setOrder,
} from '../Actions/Order.action';
import OrderHistoryList from './OrderHistoryList';
import OrderHistoryB2BDetail from './OrderHistoryB2BDetail';
import OrderHistoryB2CDetail from './OrderHistoryB2CDetail';
import Pagination from './Pagination';
import { calculatePager } from '../Services/Pagination.service';
import { translate } from '../Services/translation';
import { PaginationOptions, ViewMode } from '../constants';
import { isEmpty } from 'lodash-es';

const OrderListContainer = () => {
    const {
        mode,
        totalCount,
        currentPage,
        showOnlyMyOrders,
        errors,
    } = useSelector((state) => state.myPage.orders);
    const { isBusinessCustomer, hasApproverRole } = useSelector(
        (state) => state.myPage
    );
    const [currentShowMyOrder, setCurrentShowMyOrder] = useState(
        showOnlyMyOrders || false
    );
    const [pager, setPager] = useState({});
    const dispatch = useDispatch();

    useEffect(() => {
        dispatch(query(currentPage, currentShowMyOrder));
    }, [dispatch, currentPage, currentShowMyOrder]);

    useEffect(() => {
        setPager(calculatePager(totalCount, currentPage));
    }, [totalCount, currentPage]);

    const showDetail = useCallback(
        (order) => {
            dispatch(setOrder(order));
            dispatch(changeMode(ViewMode.Detail));
        },
        [dispatch]
    );

    const showList = useCallback(() => {
        dispatch(setOrder({}));
        dispatch(changeMode(ViewMode.List));
    }, [dispatch]);

    const changeShowMyOrder = useCallback((checked) => {
        setCurrentShowMyOrder(checked);
    }, []);

    const changePageIndex = useCallback(
        (index) => {
            if (index !== currentPage) {
                dispatch(changeCurrentPage(index));
            }
        },
        [currentPage, dispatch]
    );

    const onApproveOrderCallback = useCallback(
        (orderId, showOrderDetail = false) => {
            dispatch(
                query(
                    currentPage,
                    currentShowMyOrder,
                    PaginationOptions.PageSize,
                    orderId,
                    showOrderDetail
                )
            );
        },
        [currentPage, currentShowMyOrder, dispatch]
    );

    return (
        <Fragment>
            {errors && !isEmpty(errors) && (
                <Fragment>
                    {Object.keys(errors).map((key, i) => (
                        <div key={i} className="form__validator--error">
                            {translate(errors[key])}
                        </div>
                    ))}
                </Fragment>
            )}
            {mode !== ViewMode.List && (
                <Fragment>
                    {isBusinessCustomer ? (
                        <OrderHistoryB2BDetail
                            hasApproverRole={hasApproverRole}
                            onApproveOrderCallback={onApproveOrderCallback}
                            onDismiss={showList}
                        />
                    ) : (
                        <OrderHistoryB2CDetail onDismiss={showList} />
                    )}
                </Fragment>
            )}
            {mode === ViewMode.List && (
                <Fragment>
                    {isBusinessCustomer && hasApproverRole && (
                        <div className="order__checkbox-container">
                            <a className="order__checkbox-input">
                                <input
                                    className="form__radio"
                                    id="showOnlyMyOrders"
                                    name="showOnlyMyOrders"
                                    type="checkbox"
                                    defaultChecked={currentShowMyOrder}
                                    onChange={(e) =>
                                        changeShowMyOrder(e.target.checked)
                                    }
                                />
                                <label htmlFor="showOnlyMyOrders">
                                    {translate('orderlist.showonlymyorders')}
                                </label>
                            </a>
                        </div>
                    )}
                    <OrderHistoryList
                        onShowDetail={showDetail}
                        onApproveOrderCallback={onApproveOrderCallback}
                        isBusinessCustomer={isBusinessCustomer}
                        hasApproverRole={hasApproverRole}
                    />
                    <Pagination model={pager} onChange={changePageIndex} />
                </Fragment>
            )}
        </Fragment>
    );
};

export default OrderListContainer;
