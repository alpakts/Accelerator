import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { string, object } from 'yup';
import { translate } from '../Services/translation';
import constants from '../constants';
import { add, edit, setError } from '../Actions/Person.action';

const personSchema = object().shape({
    phone: string().required(translate(`validation.required`)),
    email: string()
        .required(translate(`validation.required`))
        .email(translate(`validation.email`)),
    lastName: string().required(translate(`validation.required`)),
    firstName: string().required(translate(`validation.required`)),
});

const PersonForm = ({ person: inputPerson, onDismiss }) => {
    const dispatch = useDispatch();
    const errors = useSelector((state) => state.myPage.persons.errors) || {};

    const [person, setPerson] = useState(inputPerson);
    useEffect(() => {
        setPerson(inputPerson);
    }, [setPerson, inputPerson]);

    const onChange = (propName, value) => {
        setPerson((prevState) => {
            return {
                ...prevState,
                [propName]: value,
            };
        });
    };

    const onSubmit = () => {
        if (!person || !person.editable) {
            return;
        }
        personSchema
            .validate(person)
            .then(() => {
                if (person.systemId) {
                    dispatch(edit(person));
                } else {
                    dispatch(add(person));
                }
            })
            .catch((error) => dispatch(setError(error)));
    };

    return (
        <div>
            <h2>
                {translate(
                    person.systemId
                        ? 'mypage.person.edittitle'
                        : 'mypage.person.addtitle'
                )}
            </h2>
            <div className="row">
                <div className="columns small-12 medium-8">
                    <label className="form__label" htmlFor="firstName">
                        {translate('mypage.person.firstname')}
                    </label>
                    <input
                        className="form__input"
                        id="firstName"
                        name="firstName"
                        type="text"
                        autoComplete="given-name"
                        value={person.firstName || ''}
                        onChange={(event) =>
                            onChange('firstName', event.target.value)
                        }
                    />
                    {errors['firstName'] && (
                        <span className="form__validator--error form__validator--top-narrow">
                            {errors['firstName'][0]}
                        </span>
                    )}
                </div>
                <div className="columns small-12 medium-8">
                    <label className="form__label" htmlFor="lastName">
                        {translate('mypage.person.lastname')}
                    </label>
                    <input
                        className="form__input"
                        id="lastName"
                        name="lastName"
                        type="text"
                        autoComplete="family-name"
                        value={person.lastName || ''}
                        onChange={(event) =>
                            onChange('lastName', event.target.value)
                        }
                    />
                    {errors['lastName'] && (
                        <span className="form__validator--error form__validator--top-narrow">
                            {errors['lastName'][0]}
                        </span>
                    )}
                </div>
            </div>

            <div className="row">
                <div className="columns small-12 medium-8">
                    <label className="form__label" htmlFor="email">
                        {translate('mypage.person.email')}
                    </label>
                    <input
                        className="form__input"
                        id="email"
                        name="email"
                        type="email"
                        autoComplete="email"
                        value={person.email || ''}
                        onChange={(event) =>
                            onChange('email', event.target.value)
                        }
                    />
                    {errors['email'] && (
                        <span className="form__validator--error form__validator--top-narrow">
                            {errors['email'][0]}
                        </span>
                    )}
                </div>
                <div className="columns small-12 medium-8">
                    <label className="form__label" htmlFor="phone">
                        {translate('mypage.person.phone')}
                    </label>
                    <input
                        className="form__input"
                        id="phone"
                        name="phone"
                        type="tel"
                        autoComplete="tel"
                        value={person.phone || ''}
                        onChange={(event) =>
                            onChange('phone', event.target.value)
                        }
                    />
                    {errors['phone'] && (
                        <span className="form__validator--error form__validator--top-narrow">
                            {errors['phone'][0]}
                        </span>
                    )}
                </div>
            </div>

            <div className="row">
                <div className="columns small-12 medium-8">
                    <label className="form__control">
                        <input
                            type="radio"
                            name="role"
                            className="form__radio"
                            value={constants.role.approver}
                            checked={person.role === constants.role.approver}
                            onChange={(event) =>
                                onChange('role', event.target.value)
                            }
                        />
                        {translate('mypage.person.role.approver')}
                    </label>
                </div>
                <div className="columns small-12 medium-8">
                    <label className="form__control">
                        <input
                            type="radio"
                            name="role"
                            className="form__radio"
                            value={constants.role.buyer}
                            checked={person.role === constants.role.buyer}
                            onChange={(event) =>
                                onChange('role', event.target.value)
                            }
                        />
                        {translate('mypage.person.role.buyer')}
                    </label>
                </div>
            </div>

            {errors['general'] && <div>{errors['general'][0]}</div>}
            <button className="form__button" onClick={onDismiss}>
                {translate('general.cancel')}
            </button>
            <span className="form__space"></span>
            <button className="form__button" onClick={onSubmit}>
                {translate('general.save')}
            </button>
        </div>
    );
};

export default PersonForm;
