import React from 'react';

const InputField = ({
    cssClass,
    name,
    id,
    autoComplete = null,
    placeholder = null,
    type = 'text',
    maxLength = 200,
    disabled,
    onChange,
    value,
    label,
    errors,
}) => {
    return (
        <div className={cssClass}>
            <label className="form__label" htmlFor={id}>
                {label}
            </label>
            <input
                className="form__input"
                disabled={disabled}
                id={id}
                name={name}
                type={type}
                value={value}
                placeholder={placeholder}
                autoComplete={autoComplete}
                onChange={(event) => onChange(event.target.value)}
                maxLength={maxLength}
            />
            {errors[id] && (
                <span
                    className="form__validator--error form__validator--top-narrow"
                    data-error-for={id}
                >
                    {errors[id][0]}
                </span>
            )}
        </div>
    );
};

export default InputField;
