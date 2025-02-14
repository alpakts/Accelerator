import { post } from './http';

export const add = async ({ articleNumber, quantity = 1 }) => {
    if (!quantity || isNaN(quantity) || parseFloat(quantity) <= 0) {
        throw 'Invalid quantity';
    }

    const response = await post('/api/cart/add', {
        articleNumber,
        quantity: parseFloat(quantity),
    });
    return response.json();
};

export const reorder = async (orderId) => {
    const response = await post('/api/cart/reorder', { orderId });
    return response.json();
};
