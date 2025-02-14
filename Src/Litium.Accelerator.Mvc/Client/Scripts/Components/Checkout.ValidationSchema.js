import { string, object, boolean, mixed } from 'yup';
import { translate } from '../Services/translation';

const privateCustomerAdditionalDetailsSchema = object().shape({
    acceptTermsOfCondition: boolean()
        .required(translate(`validation.checkrequired`))
        .oneOf([true], translate(`validation.checkrequired`)),
    selectedDeliveryMethod: object().required(translate(`validation.required`)),
    selectedPaymentMethod: object().required(translate(`validation.required`)),
});

const privateCustomerAddressSchema = object().shape({
    email: string()
        .required(translate(`validation.required`))
        .email(translate(`validation.email`)),
    phoneNumber: string().required(translate(`validation.required`)),
    country: mixed()
        .required(translate(`validation.required`))
        .notOneOf([''], translate('validation.required')),
    city: string().required(translate(`validation.required`)),
    zipCode: string().required(translate(`validation.required`)),
    address: string().required(translate(`validation.required`)),
    lastName: string().required(translate(`validation.required`)),
    firstName: string().required(translate(`validation.required`)),
});

const privateCustomerAlternativeAddressSchema = object().shape({
    phoneNumber: string().required(translate(`validation.required`)),
    country: mixed()
        .required(translate(`validation.required`))
        .notOneOf([''], translate('validation.required')),
    city: string().required(translate(`validation.required`)),
    zipCode: string().required(translate(`validation.required`)),
    address: string().required(translate(`validation.required`)),
    lastName: string().required(translate(`validation.required`)),
    firstName: string().required(translate(`validation.required`)),
});

const businessCustomerDetailsSchema = object().shape({
    email: string()
        .required(translate(`validation.required`))
        .email(translate(`validation.email`)),
    phoneNumber: string().required(translate(`validation.required`)),
    lastName: string().required(translate(`validation.required`)),
    firstName: string().required(translate(`validation.required`)),
    selectedCompanyAddressId: string().required(
        translate(`validation.required`)
    ),
});

const businessCustomerAdditionalDetailsSchema = object().shape({
    acceptTermsOfCondition: boolean()
        .required(translate(`validation.checkrequired`))
        .oneOf([true], translate(`validation.checkrequired`)),
    selectedDeliveryMethod: object().required(translate(`validation.required`)),
    selectedPaymentMethod: object().required(translate(`validation.required`)),
});

export {
    privateCustomerAdditionalDetailsSchema,
    privateCustomerAddressSchema,
    privateCustomerAlternativeAddressSchema,
    businessCustomerDetailsSchema,
    businessCustomerAdditionalDetailsSchema,
};
