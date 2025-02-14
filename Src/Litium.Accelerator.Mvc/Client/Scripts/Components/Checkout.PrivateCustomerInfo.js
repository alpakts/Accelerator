import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import constants from '../constants';
import { translate } from '../Services/translation';
import {
    setSignUp,
    setShowAlternativeAddress,
} from '../Actions/Checkout.action';
import InputField from './InputField';
import { Fragment } from 'react';
import InfoField from './InfoField';
import { getCountry } from './Checkout.BusinessCustomerInfo';
import Button from './Button';

const CheckoutPrivateCustomerInfo = ({
    onChange,
    onSave,
    valueForm,
    isEditingAddress,
    signUpCheckboxVisibility,
}) => {
    const dispatch = useDispatch();
    const { payload, errors = {} } = useSelector((state) => state.checkout);
    const { authenticated, signUp, isValidating } = payload;
    const { customerDetails = {}, alternativeAddress = {} } = valueForm;

    const input = (
        cssClass,
        stateKey,
        id,
        autoComplete = 'on',
        placeholder = null,
        type = 'text',
        maxLength = 200
    ) => (
        <InputField
            cssClass={cssClass}
            id={`${stateKey}-${id}`}
            name={`${stateKey}-${id}`}
            autoComplete={autoComplete}
            value={(valueForm[stateKey] || {})[id] || ''}
            onChange={(value) => onChange(stateKey, id, value)}
            label={translate(`checkout.customerinfo.${id.toLowerCase()}`)}
            errors={errors}
            placeholder={placeholder}
            type={type}
            maxLength={maxLength}
        />
    );

    return (
        <div className="row checkout-info__container">
            {isEditingAddress ? (
                <Fragment>
                    <div className="small-12 medium-6 columns">
                        <div className="row-inner">
                            <div className="small-12 columns checkout-info__placeholder-container"></div>
                        </div>
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
                            {input(
                                'small-12 columns',
                                'customerDetails',
                                'careOf',
                                'on',
                                null,
                                'text',
                                100
                            )}
                            {input(
                                'small-12 columns',
                                'customerDetails',
                                'address',
                                'billing street-address'
                            )}
                            {input(
                                'small-6 columns',
                                'customerDetails',
                                'zipCode',
                                'billing postal-code',
                                null,
                                'text',
                                50
                            )}
                            {input(
                                'small-6 columns',
                                'customerDetails',
                                'city',
                                'billing address-level2',
                                null,
                                'text',
                                100
                            )}
                            <div className="small-12 columns">
                                <label
                                    className="form__label"
                                    htmlFor="country"
                                >
                                    {translate('checkout.customerinfo.country')}
                                </label>
                                <select
                                    className="form__input"
                                    id="country"
                                    value={customerDetails.country}
                                    autoComplete="country"
                                    onChange={(event) => {
                                        onChange(
                                            'alternativeAddress',
                                            'country',
                                            event.target.value
                                        );
                                        onChange(
                                            'customerDetails',
                                            'country',
                                            event.target.value
                                        );
                                    }}
                                >
                                    <option value="" disabled>
                                        {translate(
                                            'checkout.customerinfo.country.placeholder'
                                        )}
                                    </option>
                                    {constants.countries &&
                                        constants.countries.map(
                                            ({ text, value }) => (
                                                <option
                                                    value={value}
                                                    key={`country-${value}`}
                                                >
                                                    {text}
                                                </option>
                                            )
                                        )}
                                </select>
                            </div>
                            {input(
                                'small-12 columns',
                                'customerDetails',
                                'phoneNumber',
                                'billing tel',
                                null,
                                'tel'
                            )}
                        </div>
                    </div>
                    <div className="small-12 medium-6 columns">
                        <div className="row-inner">
                            <div className="small-12 columns">
                                <input
                                    className="checkout-info__checkbox-input"
                                    type="checkbox"
                                    id="showAlternativeAddress"
                                    name="showAlternativeAddress"
                                    checked={
                                        alternativeAddress.showAlternativeAddress ||
                                        false
                                    }
                                    onChange={(event) => {
                                        onChange(
                                            'alternativeAddress',
                                            'showAlternativeAddress',
                                            event.target.checked
                                        );
                                        dispatch(
                                            setShowAlternativeAddress(
                                                event.target.checked
                                            )
                                        );
                                    }}
                                />
                                <label
                                    className="checkout-info__checkbox-label"
                                    htmlFor="showAlternativeAddress"
                                >
                                    {translate(
                                        'checkout.customerinfo.showAlternativeAddress'
                                    )}
                                </label>
                            </div>
                        </div>
                        {alternativeAddress.showAlternativeAddress && (
                            <div className="row-inner">
                                {input(
                                    'small-6 columns',
                                    'alternativeAddress',
                                    'firstName',
                                    'shipping given-name'
                                )}
                                {input(
                                    'small-6 columns',
                                    'alternativeAddress',
                                    'lastName',
                                    'shipping family-name'
                                )}
                                {input(
                                    'small-12 columns',
                                    'alternativeAddress',
                                    'careOf',
                                    'on',
                                    null,
                                    'text',
                                    100
                                )}
                                {input(
                                    'small-12 columns',
                                    'alternativeAddress',
                                    'address',
                                    'shipping street-address'
                                )}
                                {input(
                                    'small-6 columns',
                                    'alternativeAddress',
                                    'zipCode',
                                    'shipping postal-code',
                                    null,
                                    'text',
                                    50
                                )}
                                {input(
                                    'small-6 columns',
                                    'alternativeAddress',
                                    'city',
                                    'shipping address-level2',
                                    null,
                                    'text',
                                    100
                                )}
                                <div className="small-12 columns">
                                    <label
                                        className="form__label"
                                        htmlFor="country2"
                                    >
                                        {translate(
                                            'checkout.customerinfo.country'
                                        )}
                                    </label>
                                    <select
                                        className="form__input"
                                        id="country2"
                                        value={alternativeAddress.country}
                                        autoComplete="country"
                                        onChange={(event) => {
                                            onChange(
                                                'alternativeAddress',
                                                'country',
                                                event.target.value
                                            );
                                            onChange(
                                                'customerDetails',
                                                'country',
                                                event.target.value
                                            );
                                        }}
                                    >
                                        <option value="" disabled>
                                            {translate(
                                                'checkout.customerinfo.country.placeholder'
                                            )}
                                        </option>
                                        {constants.countries &&
                                            constants.countries.map(
                                                ({ text, value }) => (
                                                    <option
                                                        value={value}
                                                        key={`country2-${value}`}
                                                    >
                                                        {text}
                                                    </option>
                                                )
                                            )}
                                    </select>
                                </div>
                                {input(
                                    'small-12 columns',
                                    'alternativeAddress',
                                    'phoneNumber',
                                    'shipping tel',
                                    null,
                                    'tel'
                                )}
                            </div>
                        )}
                    </div>
                    <div className="small-12 medium-6 columns">
                        <div className="row-inner">
                            {input(
                                'small-12 columns',
                                'customerDetails',
                                'email',
                                'email',
                                null,
                                'email'
                            )}
                        </div>
                    </div>
                    <div
                        className={`small-12 columns flex-container ${
                            authenticated ? 'align-right' : 'align-justify'
                        }`}
                    >
                        <div>
                            {!authenticated && signUpCheckboxVisibility && (
                                <>
                                    <input
                                        className="checkout-info__checkbox-input"
                                        type="checkbox"
                                        id="signupandlogin"
                                        checked={signUp}
                                        onChange={(event) =>
                                            dispatch(
                                                setSignUp(event.target.checked)
                                            )
                                        }
                                    />
                                    <label
                                        className="checkout-info__checkbox-label"
                                        htmlFor="signupandlogin"
                                    >
                                        {translate(
                                            'checkout.customerinfo.signupandlogin'
                                        )}
                                    </label>
                                </>
                            )}
                        </div>
                        <Button
                            onClick={onSave}
                            title={translate('checkout.continue')}
                            rounded={true}
                            disabled={isValidating}
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
                    {valueForm['customerDetails']['careOf'] && (
                        <InfoField
                            values={[valueForm['customerDetails']['careOf']]}
                        />
                    )}
                    <InfoField
                        values={[valueForm['customerDetails']['address']]}
                    />
                    <InfoField
                        values={[
                            valueForm['customerDetails']['zipCode'],
                            valueForm['customerDetails']['city'],
                        ]}
                    />
                    <InfoField values={[getCountry(customerDetails)]} />

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

export default CheckoutPrivateCustomerInfo;
