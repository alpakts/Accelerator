import React from 'react';
import { useSelector } from 'react-redux';
import { translate } from '../Services/translation';
import ReorderButton from './ReorderButton';
import ApproveOrderButton from './ApproveOrderButton';

const OrderHistoryList = ({
    onShowDetail,
    onApproveOrderCallback,
    isBusinessCustomer,
    hasApproverRole,
}) => {
    const orders = useSelector((state) => state.myPage.orders.list);

    return (
        <div className="order-history__list">
            {orders?.length > 0 && (
                <div className="simple-table">
                    <div className="row medium-unstack no-margin simple-table__header hide-for-small-only">
                        <div className="columns medium-2">
                            {translate('orderlist.column.orderdate')}
                        </div>
                        <div className="columns medium-6">
                            {translate('orderlist.column.content')}
                        </div>
                        <div className="columns medium-2">
                            {translate('orderlist.column.grandtotal')}
                        </div>
                        <div className="columns medium-2">
                            {translate('orderlist.column.status')}
                        </div>
                        {isBusinessCustomer && (
                            <div className="columns medium-2 hide-for-small-only"></div>
                        )}
                    </div>
                    {orders &&
                        orders.map((order) => (
                            <div
                                key={order.orderId}
                                className="row medium-unstack no-margin"
                            >
                                <div className="columns medium-2">
                                    {order.orderDate}
                                </div>
                                <div className="columns medium-6">
                                    <a
                                        onClick={() => onShowDetail(order)}
                                        className="order-detail__product-link"
                                    >
                                        {order.orderRows[0]?.brand}
                                        <b>{order.orderRows[0]?.name}</b>&nbsp;
                                        {order.orderRows.length > 1 &&
                                            translate(
                                                order.orderRows.length > 2
                                                    ? 'orderlist.items'
                                                    : 'orderlist.item'
                                            ).replace(
                                                '{0}',
                                                order.orderRows.length - 1
                                            )}
                                    </a>
                                </div>
                                <div className="columns medium-2">
                                    {order.orderGrandTotal}
                                </div>
                                <div className="columns medium-2">
                                    {order.isWaitingApprove &&
                                        translate(
                                            'order.state.waitingconfirmation'
                                        )}
                                    {!order.isWaitingApprove && order.status}
                                </div>
                                {isBusinessCustomer && (
                                    <div className="columns medium-2">
                                        {hasApproverRole &&
                                            order.isWaitingApprove && (
                                                <ApproveOrderButton
                                                    title={translate(
                                                        'approve.label'
                                                    )}
                                                    cssClass="table__icon table__icon--accept"
                                                    orderId={order.orderId}
                                                    callback={
                                                        onApproveOrderCallback
                                                    }
                                                />
                                            )}
                                        <ReorderButton
                                            title={translate('general.reorder')}
                                            cssClass="table__icon table__icon--reorder"
                                            orderId={order.orderId}
                                        />
                                    </div>
                                )}
                            </div>
                        ))}
                </div>
            )}
            {orders?.length <= 0 && (
                <div>{translate('orderlist.noorderfound')}</div>
            )}
        </div>
    );
};

export default OrderHistoryList;
