import React from 'react';
import { useDispatch } from 'react-redux';
import { reorder as reorderService } from '../Services/Cart.service';
import withReactiveStyleBuyButton from './withReactiveStyleBuyButton';
import { receive, loadError } from '../Actions/Cart.action';
import { catchError } from '../Actions/Error.action';

const ReorderButton = ({ label, title, cssClass, orderId, onClick }) => {
    return (
        <button
            className={cssClass}
            type="button"
            title={title}
            onClick={() => onClick({ orderId })}
        >
            {label}
        </button>
    );
};

const StyledButton = (props) => {
    const dispatch = useDispatch();
    const onClick = async ({ orderId }) => {
        try {
            const cart = await reorderService(orderId);
            dispatch(receive(cart));
            return true;
        } catch (ex) {
            dispatch(catchError(ex, (error) => loadError(error)));
            return false;
        }
    };
    const Button = withReactiveStyleBuyButton(
        ReorderButton,
        onClick,
        'buy-button'
    );
    return <Button {...props} />;
};

export default StyledButton;
