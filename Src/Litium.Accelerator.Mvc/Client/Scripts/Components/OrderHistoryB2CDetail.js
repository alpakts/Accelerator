import React from 'react';
import { useSelector } from 'react-redux';
import { translate } from '../Services/translation';

const OrderHistoryB2CDetail = ({ onDismiss }) => {
    const { order } = useSelector((state) => state.myPage.orders);

    return (
        <div className="row order-detail__container">
            <div className="columns">
                <div className="row-inner order-detail__button-container">
                    <div className="small-4">
                        <a
                            className="order-detail__button"
                            target="_blank"
                            rel="noreferrer"
                            href={`my-pages/order?id=${order?.orderId}&print=true`}
                        >
                            {translate('general.print')}
                        </a>
                    </div>
                    <div className="small-8 text--right">
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
                            {translate('orderdetail.orderdate')}:{' '}
                            {order?.orderDate} <br />
                            {translate('orderdetail.orderstatus')}:{' '}
                            <strong> {order?.status}</strong>
                        </p>
                    </div>
                </div>

                {order?.formattedActualDeliveryDate && (
                    <div className="row-inner">
                        <div className="medium-12 large-6 large-offset-6 text--right text__mobile--left">
                            {translate('orderdetail.deliverydate')}:{' '}
                            <strong>
                                {order?.formattedActualDeliveryDate}
                            </strong>
                        </div>
                    </div>
                )}

                <div className="row-inner order-table">
                    <div className="small-12">
                        <div className="row medium-unstack no-margin order-table__header">
                            <div className="medium-12 columns">
                                {translate('orderdetail.information')}
                            </div>
                        </div>
                        {order?.deliveries?.map((delivery, index) => (
                            <div
                                key={index}
                                className="row no-margin order-table__body"
                            >
                                <div className="medium-12 columns">
                                    <p>
                                        {delivery.address.firstName}{' '}
                                        {delivery.address.lastName} <br />
                                        {delivery.address.address1} <br />
                                        {delivery.address.zip}{' '}
                                        {delivery.address.city} <br />
                                        {delivery.address.country}
                                    </p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="row-inner order-table">
                    <div className="row medium-unstack no-margin order-table__header hide-for-small-only">
                        <div className="columns medium-5">
                            {translate('orderdetail.column.products')}
                        </div>
                        <div className="columns medium-2">
                            {translate('orderdetail.column.quantity')}
                        </div>
                        <div className="columns medium-2">
                            {translate('orderdetail.column.price')}
                        </div>
                        <div className="columns medium-3 text--right">
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
                                <div className="columns medium-2">
                                    {row.quantityString}
                                </div>
                                <div className="columns medium-2">
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
                                <div className="columns medium-3 text--right">
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
                                {translate('orderdetail.paymentmethod')} -{' '}
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

export default OrderHistoryB2CDetail;
