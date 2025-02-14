import { get } from '../Services/http';
import { catchError } from './Error.action';
import {
    FACETED_SEARCH_QUERY,
    FACETED_SEARCH_RECEIVE,
    FACETED_SEARCH_ERROR,
    FACETED_SEARCH_TOGGLE_VALUE,
    FACETED_SEARCH_TOGGLE_COMPACT,
    PRODUCT_VIEW_CACHED,
} from '../constants';

export const query = (
    queryString = '',
    withHtmlResult = true,
    productsViewCachedId = new Date().getTime() + ''
) => (dispatch, getState) => {
    let url = withHtmlResult
        ? '/api/productFilter/withHtmlResult'
        : '/api/productFilter';
    if (queryString && queryString.trim() !== '') {
        url += `?${queryString}`;
    }
    return get(url)
        .then((response) => response.json())
        .then((result) => {
            const {
                productsView,
                sortCriteria,
                subNavigation,
                ...others
            } = result;
            if (withHtmlResult) {
                window.__litium.cache[PRODUCT_VIEW_CACHED] = {
                    productsViewCachedId,
                    productsView,
                };
            }
            result = {
                ...others,
                sortCriteria,
                subNavigation,
                productsViewCachedId,
            };
            dispatch(receive(result));
        })
        .catch((ex) => dispatch(catchError(ex, (error) => searchError(error))));
};

const submit = (facetFilters) => (dispatch, getState) => {
    const filterCriteria = toFilterCriteria(facetFilters);
    const filterIds = facetFilters.map((filter) => filter.id);
    const ignoredParams = ['page'];
    const unChangedParams = (window.location.search.substr(1) || '')
        .split('&')
        .filter((param) => {
            const [id, value] = param.split('=');
            return (
                param.length > 0 &&
                !filterIds.includes(id) &&
                !ignoredParams.includes(id)
            );
        });
    const q = [...unChangedParams, ...filterCriteria].join('&');
    dispatch(saveQuery(q));
    return dispatch(query(q));
};

const toFilterCriteria = (groups) =>
    groups
        .map((group) =>
            group.selectedOptions
                .filter((val) => val && val.length > 0)
                .map(
                    (val) =>
                        `${encodeURIComponent(group.id)}=${encodeURIComponent(
                            val
                        )}`
                )
        )
        .reduce((flat, current) => [...flat, ...current], []);

/**
 *
 * @param filter : the group has changed facet
 * @param option : changed facet
 */
export const searchFacetChange = (filter = null, option = null) => (
    dispatch,
    getState
) => {
    if (filter) {
        const allFilters = getState().facetedSearch.facetFilters;
        const newFilters = updateFilterOption(allFilters, filter, option);
        return dispatch(submit(newFilters));
    }
};

export const updateFilterOption = (
    allFilters = [],
    filter = null,
    option = null
) => {
    if (!filter) return allFilters;
    const filterIndex = allFilters.findIndex((f) => f.id === filter.id);
    const newFilter = option
        ? toggleFilterValue(filter, option)
        : { ...filter, selectedOptions: [] };
    const newFilters = [
        ...allFilters.slice(0, filterIndex),
        newFilter,
        ...allFilters.slice(filterIndex + 1),
    ];
    return newFilters;
};

const toggleFilterValue = (filter, option) => {
    const { singleSelect, selectedOptions } = filter;
    const optionIndex = selectedOptions.indexOf(option.id);
    const newSelectedOptions =
        optionIndex < 0
            ? // not yet selected, select it
              singleSelect
                ? [option.id]
                : [...selectedOptions, option.id]
            : // selected, deSelect  it
              [
                  ...selectedOptions.slice(0, optionIndex),
                  ...selectedOptions.slice(optionIndex + 1),
              ];
    return { ...filter, selectedOptions: newSelectedOptions };
};

export const submitSearchFacet = (allFilters) => (dispatch, getState) => {
    dispatch(facetClientChanged(allFilters));
    return dispatch(submit(allFilters));
};

export const facetClientChanged = (facetFilters) => ({
    type: FACETED_SEARCH_TOGGLE_VALUE,
    payload: {
        facetFilters,
    },
});

export const toggleVisibleDropdownMenu = ({ id }) => ({
    type: FACETED_SEARCH_TOGGLE_COMPACT,
    payload: {
        visibleDropdownMenu: id,
    },
});

export const searchError = (error) => ({
    type: FACETED_SEARCH_ERROR,
    payload: {
        error,
    },
});

export const receive = (payload) => {
    return {
        type: FACETED_SEARCH_RECEIVE,
        payload,
    };
};

export const saveQuery = (queryString) => ({
    type: FACETED_SEARCH_QUERY,
    payload: {
        query: queryString,
    },
});
