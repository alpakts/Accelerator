import React, { Fragment } from 'react';
import { add as addToCartService } from '../Services/Cart.service';
import withReactiveStyleBuyButton from './withReactiveStyleBuyButton';
import { receive, loadError } from '../Actions/Cart.action';
import { catchError } from '../Actions/Error.action';
import { useDispatch } from 'react-redux';

const BuyButton = ({
    label,
    articleNumber,
    quantityFieldId,
    href,
    cssClass,
    onClick,
}) => {
    return (
        <Fragment>
            {articleNumber ? (
                <a
                    className={cssClass}
                    onClick={(e) => {
                        e.preventDefault();
                        onClick({ articleNumber, quantityFieldId });
                    }}
                    href={href || '#'}
                    rel="nofollow"
                >
                    {label}
                </a>
            ) : (
                <a className={cssClass} href={href || '#'} rel="nofollow">
                    {label}
                </a>
            )}
        </Fragment>
    );
};

const StyledButton = (props) => {
    const dispatch = useDispatch();
    const onClick = async ({ articleNumber, quantityFieldId }) => {
        try {
            const quantity = quantityFieldId
                ? document.getElementById(quantityFieldId).value
                : 1;
            const cart = await addToCartService({ articleNumber, quantity });
            dispatch(receive(cart));
            return true;
        } catch (ex) {
            dispatch(catchError(ex, (error) => loadError(error)));
            return false;
        }
    };
    const Button = withReactiveStyleBuyButton(BuyButton, onClick, 'buy-button');
    return <Button {...props} />;
};

export default StyledButton;
