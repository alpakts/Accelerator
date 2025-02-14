import React, {
    Fragment,
    useCallback,
    useState,
    useEffect,
    useRef,
} from 'react';
import { useSelector, useDispatch } from 'react-redux';
import FacetedSearchCompact from './FacetedSearchCompact';
import { AccordionPanel, Accordion } from '../Components/Accordion';
import { query } from '../Actions/FacetedSearch.action';
import SubNav from './SubNavCompact';
import SortCriteriaCompact from './SortCriteriaCompact';
import FilterTag from './FilterTags';
import { translate } from '../Services/translation';
import { updateFilterOption } from '../Actions/FacetedSearch.action';
import { PRODUCT_VIEW_CACHED } from '../constants';

const FacetedSearchCompactContainer = () => {
    const dispatch = useDispatch();
    const {
        subNavigation,
        sortCriteria,
        facetFilters: globalFacetFilters,
        navigationTheme,
        productsViewCachedId,
    } = useSelector((state) => state.facetedSearch);

    const [facetFilters, setFacetFilters] = useState(globalFacetFilters || []);
    const previousCachedId = useRef(productsViewCachedId);
    useEffect(() => {
        if (previousCachedId.current !== productsViewCachedId) {
            setFacetFilters(globalFacetFilters);
            previousCachedId.current = productsViewCachedId;
        }
    }, [globalFacetFilters, productsViewCachedId]);

    useEffect(() => {
        dispatch(query(window.location.search.substr(1) || '', false));
    }, [dispatch]);

    const onFacetChange = useCallback(
        (filter, option) => {
            setFacetFilters((prevFacetFilters) => {
                return updateFilterOption(prevFacetFilters, filter, option);
            });
        },
        [setFacetFilters]
    );

    const onSearchResultDataChange = (dom) => {
        if ([null, undefined].includes(dom)) {
            return;
        }
        const container = document.createElement('div');
        container.innerHTML = dom;
        const existingResult = document.querySelector('#search-result');
        const tempResult = container.querySelector('#search-result');
        const existingFilter = existingResult.querySelector(
            '#facetedSearchCompact'
        );
        const tempFilter = tempResult.querySelector('#facetedSearchCompact');
        const replace = (node, newNode) =>
            node.parentNode.replaceChild(newNode, node);
        // move existingFilter from existingResult to tempResult
        replace(tempFilter, existingFilter);
        // replace existingResult with tempResult ( newResult )
        replace(existingResult, tempResult);
        // bootstrap react components if any exists in the search result
        window.__litium.bootstrapComponents();
    };

    useEffect(() => {
        const productViewCached = window.__litium.cache
            ? window.__litium.cache[PRODUCT_VIEW_CACHED] || {}
            : {};
        if (!productViewCached.used) {
            productViewCached.used = true;
            const dom = productViewCached.productsView;
            dom && onSearchResultDataChange(dom);
        }
    });

    const empty = (array) => !(array && array.length);

    const subNavigations = !subNavigation ? null : [subNavigation];
    const sortCriterias =
        !sortCriteria || !sortCriteria.sortItems
            ? null
            : sortCriteria.sortItems;

    const hidden = [subNavigations, facetFilters, sortCriterias].every((arr) =>
        empty(arr)
    );

    return hidden ? null : (
        <Fragment>
            <FilterTag
                {...{
                    facetFilters: globalFacetFilters,
                    navigationTheme: navigationTheme || '',
                }}
            />
            <Accordion className="compact-filter hide-for-large">
                {!empty(subNavigations) && (
                    <AccordionPanel
                        header={translate('facet.header.categories')}
                    >
                        <SubNav {...{ subNavigation: subNavigations }} />
                    </AccordionPanel>
                )}
                {!empty(facetFilters) && (
                    <AccordionPanel header={translate('facet.header.filter')}>
                        <FacetedSearchCompact
                            {...{
                                facetFilters,
                                onFacetChange,
                            }}
                        />
                    </AccordionPanel>
                )}
                {!empty(sortCriterias) && (
                    <AccordionPanel
                        header={translate('facet.header.sortCriteria')}
                    >
                        <SortCriteriaCompact
                            {...{ sortCriteria: sortCriterias }}
                        />
                    </AccordionPanel>
                )}
            </Accordion>
            {navigationTheme === 'category' && (
                <div className="compact-filter category-theme show-for-large">
                    <FacetedSearchCompact
                        {...{
                            facetFilters,
                            onFacetChange,
                        }}
                    />
                </div>
            )}
        </Fragment>
    );
};

export default FacetedSearchCompactContainer;
