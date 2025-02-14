import React from 'react';
import { useCallback } from 'react';
const Button = ({
    onClick,
    title,
    disabled = false,
    type = 'button',
    fluid = false,
    rounded = false,
    isLink = false,
}) => {
    const getClassNames = useCallback(() => {
        const classes = ['button'];
        if (fluid) classes.push('expanded');
        if (rounded) classes.push('rounded');
        if (isLink) classes.push('link');
        return classes.join(' ');
    }, [fluid, rounded, isLink]);
    return (
        <button
            className={getClassNames()}
            onClick={onClick}
            disabled={disabled}
            type={type}
        >
            {title}
        </button>
    );
};
export default Button;
