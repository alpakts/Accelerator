import { combineReducers } from 'redux';
import { cart } from './Reducers/Cart.reducer';
import { quickSearch } from './Reducers/QuickSearch.reducer';
import { navigation } from './Reducers/Navigation.reducer';
import { facetedSearch } from './Reducers/FacetedSearch.reducer';
import { checkout } from './Reducers/Checkout.reducer';
import { myPage } from './Reducers/MyPage.reducer';

const staticReducers = {
    cart,
    quickSearch,
    navigation,
    facetedSearch,
    checkout,
    myPage,
};

const app = combineReducers(staticReducers);

export const createReducer = (asyncReducers) => {
    return combineReducers({
        ...staticReducers,
        ...asyncReducers,
    });
};

export default app;
