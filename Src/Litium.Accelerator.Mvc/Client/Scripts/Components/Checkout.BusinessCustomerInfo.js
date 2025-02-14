import React, { Fragment, useMemo } from 'react';
import { useSelector } from 'react-redux';
import constants from '../constants';
import { translate } from '../Services/translation';
import InputField from './InputField';
import InfoField from './InfoField';
import Button from './Button';

const getCountry = (address) => {
    const addressCountry = constants.countries
        ? constants.countries.find(
              (country) => country.value === address.country
          )
        : null;
    return addressCountry ? addressCountry.text : address.country;
};

const CheckoutBusinessCustomerInfo = ({
    onChange,
    onSave,
    onCompanyAddressChange,
    valueForm,
    isEditingAddress,
}) => {
    const { payload, errors = {} } = useSelector((state) => state.checkout);
    const { companyAddresses = [], companyName, authenticated } = payload;

    const input = (
        cssClass,
        stateKey,
        id,
        autoComplete = 'on',
        type = 'text'
    ) => (
        <InputField
            cssClass={cssClass}
            id={`${stateKey}-${id}`}
            name={`${stateKey}-${id}`}
            autoComplete={autoComplete}
            disabled={!authenticated}
            value={(valueForm[stateKey] || {})[id] || ''}
            onChange={(value) => onChange(stateKey, id, value)}
            label={translate(`checkout.customerinfo.${id.toLowerCase()}`)}
            errors={errors}
            type={type}
        />
    );

    const selectedAddress = useMemo(() => {
        return valueForm.selectedCompanyAddressId && companyAddresses
            ? companyAddresses.find(
                  (address) =>
                      address.systemId === valueForm.selectedCompanyAddressId
              )
            : null;
    }, [valueForm.selectedCompanyAddressId, companyAddresses]);

    return (
        <div className="row checkout-info__container">
            {isEditingAddress ? (
                <Fragment>
                    <div className="small-12 medium-6 columns">
                        <div className="row-inner">
                            {input(
                                'small-6 columns',
                                'customerDetails',
                                'firstName',
                                'billing given-name'
                            )}

                            {input(
                                'small-6 columns',
                                'customerDetails',
                                'lastName',
                                'billing family-name'
                            )}
                        </div>
                        <div className="row-inner">
                            {input(
                                'small-12 columns',
                                'customerDetails',
                                'phoneNumber',
                                'billing tel',
                                'tel'
                            )}
                        </div>
                        <div className="row-inner">
                            {input(
                                'small-12 columns',
                                'customerDetails',
                                'email',
                                'email',
                                'email'
                            )}
                        </div>
                    </div>
                    <div className="small-12 medium-6 columns">
                        <div className="row-inner">
                            <div className="small-12 columns">
                                <label
                                    className="form__label"
                                    htmlFor="address"
                                >
                                    {translate('checkout.customerinfo.address')}
                                </label>

                                <select
                                    className="form__input"
                                    value={
                                        valueForm.selectedCompanyAddressId || ''
                                    }
                                    disabled={!authenticated}
                                    onChange={(event) =>
                                        onCompanyAddressChange(
                                            event.target.value,
                                            companyAddresses.find(
                                                (address) =>
                                                    address.systemId ===
                                                    event.target.value
                                            ).country
                                        )
                                    }
                                >
                                    <option value="" disabled>
                                        {translate(
                                            'checkout.customerinfo.companyaddress.placeholder'
                                        )}
                                    </option>
                                    {companyAddresses &&
                                        companyAddresses.map((address) => (
                                            <option
                                                value={address.systemId}
                                                key={`companyAddress-${address.systemId}`}
                                            >{`${address.address}, ${
                                                address.zipCode
                                            }, ${address.city}, ${getCountry(
                                                address
                                            )}`}</option>
                                        ))}
                                </select>
                                {errors['selectedCompanyAddressId'] && (
                                    <span
                                        className="form__validator--error form__validator--top-narrow"
                                        data-error-for="selectedCompanyAddressId"
                                    >
                                        {errors['selectedCompanyAddressId'][0]}
                                    </span>
                                )}
                            </div>
                        </div>
                        {selectedAddress && (
                            <div className="row-inner">
                                <div className="small-12 columns">
                                    {companyName}
                                </div>
                                <div className="small-12 columns">
                                    {selectedAddress.address}
                                </div>
                                <div className="small-12 columns">
                                    <span>{selectedAddress.zipCode}</span>&nbsp;
                                    <span>{selectedAddress.city}</span>
                                </div>
                                <div className="small-12 columns">
                                    {getCountry(selectedAddress)}
                                </div>
                            </div>
                        )}
                    </div>
                    <div className="small-12 columns flex-container align-right">
                        <Button
                            onClick={onSave}
                            title={translate('checkout.continue')}
                            rounded={true}
                        />
                    </div>
                </Fragment>
            ) : (
                <Fragment>
                    <InfoField
                        values={[
                            valueForm['customerDetails']['firstName'],
                            valueForm['customerDetails']['lastName'],
                        ]}
                    />
                    <InfoField values={[companyName]} />
                    <InfoField values={[selectedAddress.address]} />
                    <InfoField
                        values={[selectedAddress.zipCode, selectedAddress.city]}
                    />
                    <InfoField values={[getCountry(selectedAddress)]} />
                    <InfoField
                        values={[valueForm['customerDetails']['phoneNumber']]}
                    />
                    <InfoField
                        values={[valueForm['customerDetails']['email']]}
                    />
                </Fragment>
            )}
        </div>
    );
};

export default CheckoutBusinessCustomerInfo;
export { getCountry };
