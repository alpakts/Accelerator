import React, { Fragment } from 'react';
const ProductList = ({ productList }) => (
    <div className="small-block-grid-2 medium-block-grid-4 product-list">
        {productList &&
            productList.map((item, index, array) => (
                <div key={index} className="product-list__item"></div>
            ))}
    </div>
);

export default ProductList;
