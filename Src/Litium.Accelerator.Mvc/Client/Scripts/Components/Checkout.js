import React, {
    Fragment,
    useCallback,
    useEffect,
    useRef,
    useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
    acceptTermsOfCondition,
    setDelivery,
    setPayment,
    setStatusSubmitButton,
    submit,
    submitDone,
    submitError,
    setShippingOption,
} from '../Actions/Checkout.action';
import constants, {
    PaymentIntegrationType,
    ShippingIntegrationType,
} from '../constants';
import { translate } from '../Services/translation';
import Button from './Button';
import CheckoutCart from './Checkout.Cart';
import CheckoutCustomerInfo, {
    validateAlternativeAddress,
    validateCustomerInfo,
} from './Checkout.CustomerInfo';
import CheckoutDeliveryMethods from './Checkout.DeliveryMethods';
import CheckoutDiscountCodes from './Checkout.DiscountCodes';
import CheckoutOrderInfo from './Checkout.OrderInfo';
import CheckoutOrderNote from './Checkout.OrderNote';
import CheckoutPaymentMethods from './Checkout.PaymentMethods';
import {
    businessCustomerAdditionalDetailsSchema,
    businessCustomerDetailsSchema,
    privateCustomerAdditionalDetailsSchema,
    privateCustomerAddressSchema,
    privateCustomerAlternativeAddressSchema,
} from './Checkout.ValidationSchema';
import PaymentWidget from './Payments/PaymentWidget';
import ShipmentWidget from './Shipments/ShipmentWidget';

const Checkout = () => {
    const dispatch = useDispatch();
    const cart = useSelector((state) => state.cart);
    const checkout = useSelector((state) => state.checkout);
    const prevQuantity = usePrevious(cart?.quanity);
    const prevDiscount = usePrevious(cart?.discount);

    const [shippingOptionVisibility, setShippingOptionVisibility] = useState(
        true
    );
    const [paymentOptionVisibility, setPaymentOptionVisibility] = useState(
        true
    );
    const [customerDetailsVisibility, setCustomerDetailsVisibility] = useState(
        true
    );
    const [deliveryWidgetVisibility, setDeliveryWidgetVisibility] = useState(
        true
    );
    const [paymentWidgetVisibility, setPaymentWidgetVisibility] = useState(
        true
    );
    const [orderSummaryVisibility, setOrderSummaryVisibility] = useState(true);
    const [signUpCheckboxVisibility, setSignUpCheckboxVisibility] = useState(
        true
    );

    const {
        payload: {
            customerDetails,
            alternativeAddress,
            selectedCompanyAddressId,
            selectedDeliveryMethod,
            selectedPaymentMethod,
            deliveryMethods,
            paymentMethods,
        },
    } = checkout;
    const [isEditingDeliveryWidget, setIsEditingDeliveryWidget] = useState(
        true
    );
    const [isEditingAddress, setIsEditingAddress] = useState(true);
    const [addressFormValue, setAddressFormValue] = useState({
        customerDetails,
        alternativeAddress,
        selectedCompanyAddressId,
    });
    const [selectedShippingInfo, setSelectedShippingInfo] = useState(null);
    const [
        isSubmittingShippingOption,
        setIsSubmittingShippingOption,
    ] = useState(false);
    const onSubmit = useCallback(() => dispatch(submit()), [dispatch]);
    const onSubmitError = useCallback(
        (error) => {
            dispatch(submitError(error));
            dispatch(submitDone(null));
        },
        [dispatch]
    );

    const placeOrder = useCallback(() => {
        const { payload } = checkout,
            {
                isBusinessCustomer,
                selectedCompanyAddressId,
                acceptTermsOfCondition,
                selectedPaymentMethod,
                selectedDeliveryMethod,
            } = payload;
        const notCustomerDetailFields = [
            'selectedCompanyAddressId',
            'selectedPaymentMethod',
            'selectedDeliveryMethod',
            'acceptTermsOfCondition',
        ];
        const onError = (error, addressPath = 'customerDetails') => {
            error.path =
                notCustomerDetailFields.indexOf(error.path) >= 0
                    ? error.path
                    : `${addressPath}-${error.path}`;
            onSubmitError(error);
            dispatch(setStatusSubmitButton(true));
        };
        dispatch(setStatusSubmitButton(false));
        if (isBusinessCustomer) {
            businessCustomerDetailsSchema
                .validate({
                    ...payload.customerDetails,
                    selectedCompanyAddressId,
                })
                .then(() => {
                    businessCustomerAdditionalDetailsSchema
                        .validate({
                            selectedPaymentMethod,
                            selectedDeliveryMethod,
                            acceptTermsOfCondition,
                        })
                        .then(() => {
                            onSubmit();
                        })
                        .catch(onError);
                })
                .catch(onError);
        } else {
            privateCustomerAddressSchema
                .validate({
                    ...payload.customerDetails,
                })
                .then(() => {
                    if (
                        validateAlternativeAddress(payload.alternativeAddress)
                    ) {
                        privateCustomerAlternativeAddressSchema
                            .validate({
                                ...payload.alternativeAddress,
                            })
                            .then(() => {
                                privateCustomerAdditionalDetailsSchema
                                    .validate({
                                        selectedPaymentMethod,
                                        selectedDeliveryMethod,
                                        acceptTermsOfCondition,
                                    })
                                    .then(() => {
                                        onSubmit();
                                    })
                                    .catch(onError);
                            })
                            .catch((error) => {
                                onError(error, 'alternativeAddress');
                            });
                    } else {
                        privateCustomerAdditionalDetailsSchema
                            .validate({
                                selectedPaymentMethod,
                                selectedDeliveryMethod,
                                acceptTermsOfCondition,
                            })
                            .then(() => {
                                onSubmit();
                            })
                            .catch(onError);
                    }
                })
                .catch(onError);
        }
    }, [checkout, dispatch, onSubmit, onSubmitError]);

    const resetVisibilityValues = useCallback(() => {
        setShippingOptionVisibility(true);
        setPaymentOptionVisibility(true);
        setCustomerDetailsVisibility(true);
        setDeliveryWidgetVisibility(true);
        setPaymentWidgetVisibility(true);
        setOrderSummaryVisibility(true);
        setSignUpCheckboxVisibility(true);
    }, []);

    const checkIntegrationTypeExist = useCallback(
        (integrationTypeCheck) => {
            return deliveryMethods.find(
                (method) => method.integrationType === integrationTypeCheck
            );
        },
        [deliveryMethods]
    );

    const firstRender = useRef(true);

    const listenMessageHandler = useCallback((event) => {
        if (
            event.data.type == 'litium-connect-shipping' &&
            event.data.event == 'optionChanging'
        ) {
            setSelectedShippingInfo(event.data.data);
        }
    }, []);
    useEffect(() => {
        window.addEventListener('message', listenMessageHandler);
        return () =>
            window.removeEventListener('message', listenMessageHandler);
    }, [listenMessageHandler]);

    // Sets default value on first load.
    useEffect(() => {
        if (!firstRender.current) {
            return;
        }
        firstRender.current = false;

        if (!checkout) {
            return;
        }

        const {
            selectedPaymentMethod,
            selectedDeliveryMethod,
            customerDetails,
            alternativeAddress,
        } = checkout.payload;

        // set selected value for payment method on load.
        selectedPaymentMethod && dispatch(setPayment(selectedPaymentMethod));
        // set selected value for delivery method on load.
        selectedDeliveryMethod && dispatch(setDelivery(selectedDeliveryMethod));
        // fill default select value to the state
        setAddressFormValue((previousState) => ({
            ...previousState,
            customerDetails: {
                ...(customerDetails ?? {}),
                country:
                    customerDetails?.country ?? constants.countries[0].value,
            },
            alternativeAddress: {
                ...(alternativeAddress ?? {}),
                country:
                    alternativeAddress?.country ?? constants.countries[0].value,
            },
        }));
    }, [checkout, dispatch]);

    // Show or hide different sections depending on payment and delivery methods.
    useEffect(() => {
        resetVisibilityValues();
        switch (selectedPaymentMethod?.integrationType) {
            case PaymentIntegrationType.IframeCheckout:
                setCustomerDetailsVisibility(false);
                setOrderSummaryVisibility(false);
                break;
            case PaymentIntegrationType.PaymentWidgets:
                if (isEditingAddress) {
                    setPaymentWidgetVisibility(false);
                }
                setOrderSummaryVisibility(false);
                setSignUpCheckboxVisibility(false);
                break;
            case PaymentIntegrationType.DirectPayment:
                if (isEditingAddress) {
                    setOrderSummaryVisibility(false);
                }
                setPaymentWidgetVisibility(false);
                break;
        }
        switch (selectedDeliveryMethod?.integrationType) {
            case ShippingIntegrationType.DeliveryCheckout:
                if (!selectedShippingInfo?.value) {
                    setIsEditingDeliveryWidget(true);
                }
                if (isEditingAddress) {
                    setDeliveryWidgetVisibility(false);
                }
                if (
                    selectedPaymentMethod?.integrationType ===
                    PaymentIntegrationType.IframeCheckout
                ) {
                    setCustomerDetailsVisibility(false);
                }
                break;
            default:
                setDeliveryWidgetVisibility(false);
                setSelectedShippingInfo(null);
                break;
        }
        if (
            checkIntegrationTypeExist(ShippingIntegrationType.DeliveryCheckout)
        ) {
            return setShippingOptionVisibility(true);
        }
        if (
            checkIntegrationTypeExist(ShippingIntegrationType.PaymentCheckout)
        ) {
            return (
                setShippingOptionVisibility(false) &&
                setDeliveryWidgetVisibility(false)
            );
        }
    }, [
        selectedDeliveryMethod,
        selectedPaymentMethod,
        deliveryMethods,
        paymentMethods,
        resetVisibilityValues,
        checkIntegrationTypeExist,
        isEditingAddress,
        isEditingDeliveryWidget,
        selectedShippingInfo,
    ]);

    // Update place order button's status when the state of Customer Information form is changed.
    useEffect(() => {
        dispatch(setStatusSubmitButton(false));
        if (isEditingAddress) {
            return;
        }
        setIsEditingDeliveryWidget(true);
        validateCustomerInfo(
            addressFormValue,
            checkout.payload.isBusinessCustomer
        )
            .then(() => {
                dispatch(setStatusSubmitButton(true));
            })
            .catch(() => {
                dispatch(setStatusSubmitButton(false));
            });
    }, [
        dispatch,
        isEditingAddress,
        addressFormValue,
        checkout.payload.isBusinessCustomer,
    ]);

    // Scroll the the first field that has validation error when saving the form.
    useEffect(() => {
        if (checkout.result && checkout.result.redirectUrl) {
            window.location = checkout.result.redirectUrl;
            return;
        }

        if (!checkout.errors) {
            return;
        }

        const errorKeys = Object.keys(checkout.errors);
        if (!errorKeys || errorKeys.length < 1) {
            return;
        }

        const errorNode = document.querySelector(
            `[data-error-for="${errorKeys[0]}"]`
        );
        if (!errorNode) {
            return;
        }

        const inputNode = errorNode.parentElement.querySelector('input');
        if (inputNode) {
            setTimeout(() => inputNode.focus(), 1000);
            inputNode.scrollIntoView({ behavior: 'smooth' });
        } else {
            errorNode.scrollIntoView({ behavior: 'smooth' });
        }
    }, [checkout.result, checkout.errors]);

    // Set delivery widget to edit mode when the cart changed
    useEffect(() => {
        if (
            cart?.quantity !== prevQuantity ||
            cart?.discount !== prevDiscount
        ) {
            setIsEditingDeliveryWidget(true);
        }
    }, [cart?.quantity, cart?.discount, prevQuantity, prevDiscount]);

    if (!cart || !cart.orderRows || cart.orderRows.length < 1) {
        return (
            <div className="row">
                <div className="small-12">
                    <h2 className="checkout__title">
                        {translate(`checkout.cart.empty`)}
                    </h2>
                </div>
            </div>
        );
    }

    const { payload, errors = {} } = checkout,
        {
            shipmentWidget,
            paymentWidget,
            authenticated,
            isBusinessCustomer,
            checkoutMode,
        } = payload;
    const responseString = paymentWidget ? paymentWidget.responseString : null;
    const updateKey = paymentWidget ? paymentWidget._force_update : null;
    const shipmentResponseString = shipmentWidget
        ? shipmentWidget.responseString
        : null;
    const shipmentUpdateKey = shipmentWidget
        ? shipmentWidget._force_update
        : null;
    const showPaymentWidget =
        paymentWidget &&
        paymentWidgetVisibility &&
        (selectedDeliveryMethod?.integrationType !==
            ShippingIntegrationType.DeliveryCheckout ||
            !isEditingDeliveryWidget);
    const isSelectedDeliveryCheckout =
        selectedDeliveryMethod.integrationType ===
        ShippingIntegrationType.DeliveryCheckout;
    const isSelectedIframeWidget =
        selectedPaymentMethod.integrationType ===
        PaymentIntegrationType.IframeCheckout;
    const showShippingWidget =
        shipmentWidget &&
        isSelectedDeliveryCheckout &&
        (!customerDetailsVisibility || !isEditingAddress) &&
        isEditingDeliveryWidget;
    const showShippingSummary =
        !isEditingDeliveryWidget &&
        isSelectedDeliveryCheckout &&
        (!customerDetailsVisibility ||
            (!isSelectedIframeWidget && !isEditingAddress));
    return (
        <Fragment>
            <CheckoutCart errors={errors} />
            <CheckoutDiscountCodes />

            {shippingOptionVisibility && (
                <CheckoutDeliveryMethods errors={errors} />
            )}

            {paymentOptionVisibility && (
                <CheckoutPaymentMethods errors={errors} />
            )}

            {customerDetailsVisibility && (
                <CheckoutCustomerInfo
                    checkout={checkout}
                    addressFormValue={addressFormValue}
                    isEditingAddress={isEditingAddress}
                    setAddressFormValue={setAddressFormValue}
                    setIsEditingAddress={(bool) => {
                        setIsEditingAddress(bool);
                        if (bool) {
                            setIsEditingDeliveryWidget(true);
                        }
                    }}
                    signUpCheckboxVisibility={signUpCheckboxVisibility}
                />
            )}

            <div
                className={`row checkout-info__container checkout-info__summary ${
                    !showShippingWidget && 'hide'
                }`}
            >
                <div className="columns small-12">
                    <ShipmentWidget
                        key={shipmentUpdateKey}
                        responseString={shipmentResponseString}
                    />
                </div>
                <div className="small-12 columns flex-container align-justify">
                    <Button
                        onClick={() => {
                            setIsSubmittingShippingOption(true);
                            dispatch(
                                setShippingOption({
                                    shippingInfo: selectedShippingInfo.value,
                                    onSuccess: () => {
                                        setIsEditingDeliveryWidget(false);
                                        setIsSubmittingShippingOption(false);
                                    },
                                    onError: () => {
                                        setIsSubmittingShippingOption(false);
                                    },
                                })
                            );
                        }}
                        title={translate('checkout.continue')}
                        rounded={true}
                        disabled={
                            !selectedShippingInfo?.value ||
                            isSubmittingShippingOption
                        }
                    />
                </div>
            </div>

            {showShippingSummary && (
                <Fragment>
                    <div className="row align-justify">
                        <div className="flex-container checkout__flex-wrapper">
                            <h3 className="checkout__section-title">
                                Shipping option
                            </h3>
                        </div>
                        <Button
                            onClick={() => {
                                setIsEditingDeliveryWidget(true);
                            }}
                            title={translate('checkout.edit')}
                            isLink={true}
                        />
                    </div>
                    <div className="row checkout-info__container">
                        <div className="small-12 medium-12 columns">
                            <span>{selectedShippingInfo?.name || ''}</span>
                        </div>
                    </div>
                </Fragment>
            )}

            {showPaymentWidget && (
                <PaymentWidget
                    key={updateKey}
                    responseString={responseString}
                />
            )}

            {orderSummaryVisibility &&
                (!shipmentWidget || !isEditingDeliveryWidget) && (
                    <Fragment>
                        <div className="row">
                            <h3 className="checkout__section-title">
                                {translate('checkout.order.title')}
                            </h3>
                        </div>

                        <section className="row checkout-info__container checkout-info__summary">
                            <CheckoutOrderNote />
                            <CheckoutOrderInfo />
                        </section>

                        <div className="row">
                            <input
                                className="checkout-info__checkbox-input"
                                type="checkbox"
                                id="acceptTermsOfCondition"
                                checked={payload.acceptTermsOfCondition}
                                onChange={(event) =>
                                    dispatch(
                                        acceptTermsOfCondition(
                                            event.target.checked
                                        )
                                    )
                                }
                            />
                            <label
                                className="checkout-info__checkbox-label"
                                htmlFor="acceptTermsOfCondition"
                            >
                                {translate(
                                    'checkout.terms.acceptTermsOfCondition'
                                )}{' '}
                                <a
                                    className="checkout__link"
                                    href={payload.termsUrl}
                                    target="_blank"
                                    rel="noreferrer"
                                >
                                    {translate('checkout.terms.link')}
                                </a>
                            </label>
                            {errors['acceptTermsOfCondition'] && (
                                <span
                                    className="form__validator--error form__validator--top-narrow"
                                    data-error-for="acceptTermsOfCondition"
                                >
                                    {errors['acceptTermsOfCondition'][0]}
                                </span>
                            )}
                        </div>

                        <div className="row checkout__submit">
                            {!authenticated &&
                            (isBusinessCustomer ||
                                checkoutMode ===
                                    constants.checkoutMode.companyCustomers) ? (
                                <Button
                                    onClick={() =>
                                        (location.href = payload.loginUrl)
                                    }
                                    title={translate(
                                        'checkout.login.to.placeorder'
                                    )}
                                    fluid={true}
                                />
                            ) : (
                                <Button
                                    disabled={!checkout.enableConfirmButton}
                                    onClick={placeOrder}
                                    title={translate('checkout.placeorder')}
                                    fluid={true}
                                    type="submit"
                                />
                            )}
                        </div>
                    </Fragment>
                )}

            <div className="row">
                {errors && errors['general'] && (
                    <p className="checkout__validator--error">
                        {errors['general'][0]}
                    </p>
                )}
                {errors && errors['payment'] && (
                    <p className="checkout__validator--error">
                        {errors['payment'][0]}
                    </p>
                )}
            </div>
        </Fragment>
    );
};

export default Checkout;

function usePrevious(value) {
    const ref = useRef();
    useEffect(() => {
        ref.current = value; //assign the value of ref to the argument
    }, [value]); //this code will run when the value of 'value' changes
    return ref.current; //in the end, return the current ref value.
}
