import React, { useState, useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { translate } from '../Services/translation';
import ReorderButton from './ReorderButton';
import ApproveOrderButton from './ApproveOrderButton';

const OrderHistoryB2BDetail = ({
    onDismiss,
    onApproveOrderCallback,
    hasApproverRole,
}) => {
    const { order } = useSelector((state) => state.myPage.orders);
    const [showArroveOrderButton, setShowArroveOrderButton] = useState(false);

    useEffect(() => {
        setShowArroveOrderButton(hasApproverRole && order?.isWaitingApprove);
    }, [hasApproverRole, order?.isWaitingApprove]);

    const approveOrderCallback = useCallback(() => {
        onApproveOrderCallback && onApproveOrderCallback(order?.orderId, true);
    }, [onApproveOrderCallback, order?.orderId]);

    return (
        <div className="row order-detail__container">
            <div className="columns">
                <div className="row-inner order-detail__button-container">
                    <div className="small-6">
                        <a
                            className="order-detail__button"
                            target="_blank"
                            rel="noreferrer"
                            href={`my-pages/order?id=${order?.orderId}&print=true`}
                        >
                            {translate('general.print')}
                        </a>
                        {showArroveOrderButton && (
                            <ApproveOrderButton
                                label={translate('approve.label')}
                                cssClass="order-detail__button"
                                orderId={order?.orderId}
                                callback={approveOrderCallback}
                            />
                        )}
                        <ReorderButton
                            label={translate('general.reorder')}
                            cssClass="order-detail__button"
                            orderId={order?.orderId}
                        />
                    </div>
                    <div className="small-6 text--right">
                        <a className="order-detail__button" onClick={onDismiss}>
                            {translate('orderdetail.backtoorderlist')}
                        </a>
                    </div>
                </div>
                <div className="row-inner">
                    <div className="medium-12 large-6">
                        <h3>
                            {translate('orderdetail.ordernumber')}:{' '}
                            {order?.externalOrderID}
                        </h3>
                    </div>
                    <div className="medium-12 large-6 text--right text__mobile--left">
                        <p>
                            <span>
                                {translate('orderdetail.orderdate')}:{' '}
                                {order?.orderDate}
                            </span>
                            <br />
                            <span>
                                {translate('orderdetail.orderstatus')}:{' '}
                                <strong>
                                    {order?.isWaitingApprove &&
                                        translate(
                                            'order.state.waitingconfirmation'
                                        )}
                                    {!order?.isWaitingApprove && order?.status}
                                </strong>
                            </span>
                            <br />
                            {order?.formattedActualDeliveryDate && (
                                <span>
                                    {translate('orderdetail.deliverydate')}:{' '}
                                    <strong>
                                        {order?.formattedActualDeliveryDate}
                                    </strong>
                                </span>
                            )}
                        </p>
                    </div>
                </div>
                <div className="row-inner">
                    <div className="small-12">
                        <div className="row medium-unstack no-margin">
                            <div className="medium-12">
                                {translate('orderdetail.information')}
                            </div>
                        </div>
                        <div className="row no-margin">
                            <div className="medium-12">
                                <p>
                                    {order?.customerInfo?.address1}
                                    <br />
                                    {order?.customerInfo?.zip}{' '}
                                    {order?.customerInfo?.city} <br />
                                    {order?.customerInfo?.country}
                                </p>
                                <p>
                                    {translate(
                                        'orderdetail.organizationnumber'
                                    )}
                                    : {order?.merchantOrganizationNumber}
                                    <br />
                                    {translate(
                                        'orderdetail.orderreference'
                                    )}: {order?.customerInfo?.firstName}{' '}
                                    {order?.customerInfo?.lastName}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="row-inner order-table">
                    <div className="row medium-unstack no-margin order-table__header hide-for-small-only">
                        <div className="columns medium-5">
                            {translate('orderdetail.column.products')}
                        </div>
                        <div className="columns medium-1">
                            {translate('orderdetail.column.quantity')}
                        </div>
                        <div className="columns medium-4">
                            {translate('orderdetail.column.price')}
                        </div>
                        <div className="columns medium-2 text--right">
                            {translate('orderdetail.column.total')}
                        </div>
                    </div>
                    <div className="order-table__body">
                        {order?.orderRows?.map((row, index) => (
                            <div
                                key={index}
                                className="row medium-unstack no-margin order-detail__summary-items"
                            >
                                <div className="columns medium-5">
                                    <a
                                        href={row.link?.href}
                                        target="_parent"
                                        className="order-detail__product-link"
                                    >
                                        {row.brand} <strong>{row.name}</strong>
                                    </a>
                                </div>
                                <div className="columns medium-1">
                                    {row.quantityString}
                                </div>
                                <div className="columns medium-4">
                                    {row.isFreeGift &&
                                    row.priceInfo?.formattedCampaignPrice ? (
                                        <span>
                                            {
                                                row.priceInfo
                                                    ?.formattedCampaignPrice
                                            }
                                        </span>
                                    ) : (
                                        <span>
                                            {row.priceInfo?.formattedPrice}
                                        </span>
                                    )}
                                </div>
                                <div className="columns medium-2 text--right">
                                    {row.totalPrice}
                                </div>
                            </div>
                        ))}
                        {order?.discountRows?.map((row, index) => (
                            <div
                                key={index}
                                className="row medium-unstack no-margin order-detail__summary-items"
                            >
                                <div className="columns medium-5">
                                    {row.name}
                                </div>
                                <div className="columns medium-7 text--right">
                                    {row.totalPrice}
                                </div>
                            </div>
                        ))}
                        <div className="row medium-unstack no-margin order-detail__summary-method">
                            <div className="columns medium-9">
                                {translate('orderdetail.paymentmethod')} -
                                {order?.paymentMethod}
                            </div>
                            <div className="columns medium-3 text--right">
                                {order?.orderTotalFee}
                            </div>
                        </div>
                        <div className="row medium-unstack no-margin order-detail__summary-method">
                            <div className="columns medium-9">
                                {translate('orderdetail.deliverymethod')} -{' '}
                                {order?.deliveryMethod}
                            </div>
                            <div className="columns medium-3 text--right">
                                {order?.orderTotalDeliveryCost}
                            </div>
                        </div>
                        {order?.orderTotalDiscountAmount && (
                            <div className="row medium-unstack no-margin order-detail__summary-method">
                                <div className="columns medium-9">
                                    {translate('orderdetail.discount')}
                                </div>
                                <div className="columns medium-3 text--right">
                                    {order?.orderTotalDiscountAmount}
                                </div>
                            </div>
                        )}
                        <div className="row medium-unstack no-margin order-table__space-delimiter"></div>
                        <div className="row medium-unstack no-margin">
                            <div className="columns small-12 text--right">
                                {translate('orderdetail.grandtotal')}:{' '}
                                <strong>{order?.orderGrandTotal}</strong>
                            </div>
                        </div>
                        <div className="row medium-unstack no-margin">
                            <div className="columns small-12 text--right">
                                {translate('orderdetail.ordertotalvat')}:{' '}
                                {order?.orderTotalVat}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default OrderHistoryB2BDetail;
