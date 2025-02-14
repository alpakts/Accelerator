import React, { Fragment, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import {
    clearError,
    setBusinessCustomer,
    setSelectedCompanyAddress,
    submitError,
    updateCustomerDetails,
    setCountry,
    saveCustomerDetail,
    setValidateStatus,
} from '../Actions/Checkout.action';
import CheckoutBusinessCustomerInfo from './Checkout.BusinessCustomerInfo';
import CheckoutPrivateCustomerInfo from './Checkout.PrivateCustomerInfo';
import {
    businessCustomerDetailsSchema,
    privateCustomerAddressSchema,
    privateCustomerAlternativeAddressSchema,
} from './Checkout.ValidationSchema';
import constants from '../constants';
import { translate } from '../Services/translation';
import Button from './Button';

const validateAlternativeAddress = (alternativeAddress) => {
    return (
        alternativeAddress &&
        alternativeAddress.showAlternativeAddress &&
        (alternativeAddress.firstName ||
            alternativeAddress.lastName ||
            alternativeAddress.address ||
            alternativeAddress.zipCode ||
            alternativeAddress.city ||
            alternativeAddress.phoneNumber)
    );
};

const validateCustomerInfo = (result, isBusinessCustomer) => {
    const {
        customerDetails,
        selectedCompanyAddressId,
        alternativeAddress,
    } = result;

    if (isBusinessCustomer) {
        return businessCustomerDetailsSchema.validate({
            ...customerDetails,
            selectedCompanyAddressId,
        });
    }

    return privateCustomerAddressSchema
        .validate({
            ...customerDetails,
        })
        .then((result) => {
            if (validateAlternativeAddress(alternativeAddress)) {
                return privateCustomerAlternativeAddressSchema
                    .validate({
                        ...alternativeAddress,
                    })
                    .catch((error) => {
                        error.addressPath = 'alternativeAddress';
                        throw error;
                    });
            } else {
                return result;
            }
        });
};

const CheckoutCustomerInfo = ({
    checkout,
    addressFormValue,
    setAddressFormValue,
    isEditingAddress,
    setIsEditingAddress,
    signUpCheckboxVisibility,
}) => {
    const dispatch = useDispatch();
    const {
        payload: { loginUrl, authenticated, isBusinessCustomer, checkoutMode },
    } = checkout;

    const onCustomerInfoChange = useCallback(
        (stateKey, id, value) => {
            setAddressFormValue((previousState) => ({
                ...previousState,
                [stateKey]: {
                    ...previousState[stateKey],
                    [id]: value,
                },
            }));
        },
        [setAddressFormValue]
    );
    const onCompanyAddressChange = useCallback(
        (companyAddressId, country) => {
            setAddressFormValue((previousState) => ({
                ...previousState,
                selectedCompanyAddressId: companyAddressId,
                customerDetails: {
                    ...previousState.customerDetails,
                    country,
                },
            }));
        },
        [setAddressFormValue]
    );

    const onEditForm = useCallback(() => {
        setIsEditingAddress(true);
    }, [setIsEditingAddress]);

    const onUpdateCustomerDetails = useCallback(() => {
        const notCustomerDetailFields = ['selectedCompanyAddressId'];

        const onValid = () => {
            const { payload } = checkout,
                { isBusinessCustomer } = payload;
            const {
                customerDetails,
                selectedCompanyAddressId,
                alternativeAddress,
            } = addressFormValue;

            dispatch(updateCustomerDetails('customerDetails', customerDetails));
            if (isBusinessCustomer) {
                dispatch(setSelectedCompanyAddress(selectedCompanyAddressId));
                const companyAddress = payload.companyAddresses.find(
                    (c) => c.systemId === selectedCompanyAddressId
                );
                dispatch(setCountry(companyAddress.country));
            } else {
                dispatch(
                    updateCustomerDetails(
                        'alternativeAddress',
                        alternativeAddress
                    )
                );
                dispatch(setCountry(customerDetails.country));
            }
            setIsEditingAddress(false);
        };

        const onSetCustomerDetail = () =>
            dispatch(saveCustomerDetail(addressFormValue));

        const onError = (error) => {
            error.path =
                notCustomerDetailFields.indexOf(error.path) >= 0
                    ? error.path
                    : `${error?.addressPath || 'customerDetails'}-${
                          error.path
                      }`;
            dispatch(submitError(error));
        };

        dispatch(setValidateStatus(true));
        dispatch(clearError());

        validateCustomerInfo(addressFormValue, isBusinessCustomer)
            .then(onSetCustomerDetail)
            .then(onValid)
            .catch(onError)
            .finally(() => {
                dispatch(setValidateStatus(false));
            });
    }, [
        dispatch,
        addressFormValue,
        isBusinessCustomer,
        checkout,
        setIsEditingAddress,
    ]);

    const privateCustomerInfoComponent = useCallback(() => {
        return (
            <CheckoutPrivateCustomerInfo
                onSave={onUpdateCustomerDetails}
                onChange={onCustomerInfoChange}
                valueForm={addressFormValue}
                isEditingAddress={isEditingAddress}
                signUpCheckboxVisibility={signUpCheckboxVisibility}
            />
        );
    }, [
        addressFormValue,
        isEditingAddress,
        onCustomerInfoChange,
        onUpdateCustomerDetails,
        signUpCheckboxVisibility,
    ]);

    const businessCustomerInfoComponent = useCallback(() => {
        return (
            <CheckoutBusinessCustomerInfo
                onChange={onCustomerInfoChange}
                onCompanyAddressChange={onCompanyAddressChange}
                onSave={onUpdateCustomerDetails}
                valueForm={addressFormValue}
                isEditingAddress={isEditingAddress}
            />
        );
    }, [
        addressFormValue,
        isEditingAddress,
        onCompanyAddressChange,
        onCustomerInfoChange,
        onUpdateCustomerDetails,
    ]);

    if (!authenticated) {
        return (
            <Fragment>
                <div className="row align-justify">
                    <div className="flex-container checkout__flex-wrapper">
                        <h3 className="checkout__section-title">
                            {translate('checkout.customerinfo.title')}
                        </h3>
                        {isEditingAddress && (
                            <Fragment>
                                <label className="checkout__text--in-line">
                                    {translate(
                                        'checkout.customerinfo.existingcustomer'
                                    )}
                                </label>
                                &nbsp;
                                <a href={loginUrl} className="checkout__link">
                                    {translate(
                                        'checkout.customerinfo.clicktologin'
                                    )}
                                </a>
                                &nbsp;
                                {!isBusinessCustomer &&
                                    checkoutMode ===
                                        constants.checkoutMode.both && (
                                        <a
                                            onClick={() =>
                                                dispatch(
                                                    setBusinessCustomer(true)
                                                )
                                            }
                                            className="checkout__link"
                                        >
                                            {translate(
                                                'checkout.customerinfo.businesscustomer'
                                            )}
                                        </a>
                                    )}
                                {isBusinessCustomer &&
                                    checkoutMode ===
                                        constants.checkoutMode.both && (
                                        <a
                                            onClick={() =>
                                                dispatch(
                                                    setBusinessCustomer(false)
                                                )
                                            }
                                            className="checkout__link"
                                        >
                                            {translate(
                                                'checkout.customerinfo.privatecustomer'
                                            )}
                                        </a>
                                    )}
                            </Fragment>
                        )}
                    </div>
                    {!isEditingAddress && (
                        <Button
                            onClick={onEditForm}
                            title={translate('checkout.edit')}
                            isLink={true}
                        />
                    )}
                </div>
                {!isBusinessCustomer &&
                    checkoutMode !== constants.checkoutMode.companyCustomers &&
                    privateCustomerInfoComponent()}
                {(isBusinessCustomer ||
                    checkoutMode === constants.checkoutMode.companyCustomers) &&
                    businessCustomerInfoComponent()}
            </Fragment>
        );
    }

    return (
        <Fragment>
            <div className="row align-justify">
                <h3 className="checkout__section-title">
                    {translate('checkout.customerinfo.title')}
                </h3>
                {!isEditingAddress && (
                    <Button onClick={onEditForm} title="edit" isLink={true} />
                )}
            </div>
            {!isBusinessCustomer && privateCustomerInfoComponent()}
            {isBusinessCustomer && businessCustomerInfoComponent()}
        </Fragment>
    );
};

export default CheckoutCustomerInfo;
export { validateAlternativeAddress, validateCustomerInfo };
