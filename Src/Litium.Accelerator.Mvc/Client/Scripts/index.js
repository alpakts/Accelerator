import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { applyMiddleware, createStore } from 'redux';
import { composeWithDevTools } from 'redux-devtools-extension/developmentOnly';
import thunk from 'redux-thunk';
import app, { createReducer } from './reducers';
import { historyMiddleware } from './Middlewares/History.middleware';
import MiniCart from './Components/MiniCart';
import QuickSearch from './Components/QuickSearch';
import Navigation from './Components/Navigation';
import FacetedSearch from './Components/FacetedSearch';
import FacetedSearchCompactContainer from './Components/FacetedSearchCompactContainer';
import DynamicComponent from './Components/DynamicComponent';

window.__litium = window.__litium || {};
const preloadState = window.__litium.preloadState || {};
const store = createStore(
    app,
    preloadState,
    composeWithDevTools(applyMiddleware(thunk, historyMiddleware))
);

// Add a dictionary to keep track of the registered async reducers
store.asyncReducers = {};

// Create an inject reducer function
// This function adds the async reducer, and creates a new combined reducer
store.injectReducer = (key, asyncReducer) => {
    if (!store.asyncReducers[key]) {
        store.asyncReducers[key] = asyncReducer;
        store.replaceReducer(createReducer(store.asyncReducers));
    }
};

window.__litium = {
    ...window.__litium,
    bootstrapComponents: () => {
        // bootstrap React components, in case the HTML response we receive from the server
        // has React components. ReactDOM.render performs only an update on previous rendered
        // components and only mutate the DOM as necessary to reflect latest React element.
        bootstrapComponents();
    },
    cache: {}, // for storing cache data for current request
};

const registeredContainers = [];
const renderReact = (element, container, callback) => {
    registeredContainers.push(container);
    ReactDOM.render(element, container, callback);
};

window.onunload = () => {
    // make sure components are unmounted, redux's listener are unsubscribed
    // to fix issue with iframe navigation in IE
    registeredContainers.forEach((container) => {
        ReactDOM.unmountComponentAtNode(container);
    });
};

const bootstrapComponents = () => {
    if (document.getElementById('miniCart')) {
        renderReact(
            <Provider store={store}>
                <MiniCart />
            </Provider>,
            document.getElementById('miniCart')
        );
    }
    if (document.getElementById('quickSearch')) {
        renderReact(
            <Provider store={store}>
                <QuickSearch />
            </Provider>,
            document.getElementById('quickSearch')
        );
    }
    if (document.getElementById('navbar')) {
        renderReact(
            <Provider store={store}>
                <Navigation />
            </Provider>,
            document.getElementById('navbar')
        );
    }
    if (document.getElementById('facetedSearch')) {
        renderReact(
            <Provider store={store}>
                <FacetedSearch />
            </Provider>,
            document.getElementById('facetedSearch')
        );
    }
    if (document.getElementById('facetedSearchCompact')) {
        renderReact(
            <Provider store={store}>
                <FacetedSearchCompactContainer />
            </Provider>,
            document.getElementById('facetedSearchCompact')
        );
    }
    if (document.getElementById('myPagePersons')) {
        const PersonList = DynamicComponent({
            loader: () => import('./Components/PersonListContainer'),
        });
        renderReact(
            <Provider store={store}>
                <PersonList />
            </Provider>,
            document.getElementById('myPagePersons')
        );
    }
    if (document.getElementById('myPageAddresses')) {
        const AddressList = DynamicComponent({
            loader: () => import('./Components/AddressListContainer'),
        });
        renderReact(
            <Provider store={store}>
                <AddressList />
            </Provider>,
            document.getElementById('myPageAddresses')
        );
    }
    if (document.getElementById('checkout')) {
        const Checkout = DynamicComponent({
            loader: () => import('./Components/Checkout'),
        });
        renderReact(
            <Provider store={store}>
                <Checkout />
            </Provider>,
            document.getElementById('checkout')
        );
    }
    if (document.getElementById('lightBoxImages')) {
        const LightboxImages = DynamicComponent({
            loader: () => import('./Components/LightboxImages'),
        });
        import('./Reducers/LightboxImages.reducer').then(
            ({ lightboxImages }) => {
                store.injectReducer('lightboxImages', lightboxImages);
                const rootElement = document.getElementById('lightBoxImages');
                const images = Array.from(
                    rootElement.querySelectorAll('template')
                ).map((img) => ({
                    html: img.innerHTML,
                    src: img.dataset.src,
                }));
                renderReact(
                    <Provider store={store}>
                        <LightboxImages images={images} />
                    </Provider>,
                    document.getElementById('lightBoxImages')
                );
            }
        );
    }

    if (document.querySelectorAll('.slider').length > 0) {
        const Slider = DynamicComponent({
            loader: () => import('./Components/Slider'),
        });
        Array.from(document.querySelectorAll('.slider')).forEach(
            (slider, index) => {
                const values = [...slider.querySelectorAll('template')].map(
                    (block) => {
                        return {
                            html: block.innerHTML,
                        };
                    }
                );
                if (values.length > 0) {
                    renderReact(<Slider values={values} />, slider);
                }
            }
        );
    }

    if (document.querySelectorAll('buy-button').length > 0) {
        const BuyButton = DynamicComponent({
            loader: () => import('./Components/BuyButton'),
        });
        Array.from(document.querySelectorAll('buy-button')).forEach(
            (button) => {
                const {
                    articleNumber,
                    quantityFieldId,
                    href,
                    cssClass,
                    label,
                } = button.dataset;
                renderReact(
                    <Provider store={store}>
                        <BuyButton
                            {...{
                                label,
                                articleNumber,
                                quantityFieldId,
                                href,
                                cssClass,
                            }}
                        />
                    </Provider>,
                    button
                );
            }
        );
    }

    if (document.getElementById('orderHistoryPage')) {
        const OrderList = DynamicComponent({
            loader: () => import('./Components/OrderHistoryListContainer'),
        });
        renderReact(
            <Provider store={store}>
                <OrderList />
            </Provider>,
            document.getElementById('orderHistoryPage')
        );
    }
};

bootstrapComponents();
