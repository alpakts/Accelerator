import React, { useCallback } from 'react';
import { approveOrder } from '../Actions/Order.action';
import withReactiveStyleBuyButton from './withReactiveStyleBuyButton';
import { useDispatch } from 'react-redux';

const ApproveOrderButton = ({
    label,
    title,
    cssClass,
    orderId,
    onClick,
    callback,
}) => {
    return (
        <button
            onClick={() => onClick({ orderId, callback })}
            className={cssClass}
            title={title || label}
        >
            {label}
        </button>
    );
};

const StyledButton = (props) => {
    const dispatch = useDispatch();
    const onClick = ({ orderId, callback }) => {
        try {
            dispatch(approveOrder(orderId, callback));
            return true;
        } catch (ex) {
            return false;
        }
    };
    const Button = withReactiveStyleBuyButton(
        ApproveOrderButton,
        onClick,
        'buy-button'
    );
    return <Button {...props} />;
};

export default StyledButton;
