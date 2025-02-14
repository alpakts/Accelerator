import { combineReducers } from 'redux';
import { filterFieldReducer } from './accelerator-filter-fields.reducer';

export const accelerator = combineReducers({
    productfiltering: filterFieldReducer,
});