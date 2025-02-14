import React, { useRef, useEffect, useState, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import QuickSearchResult from './QuickSearchResult';
import { translate } from '../Services/translation';
import {
    query,
    setSearchQuery,
    toggleShowFullForm,
    handleKeyDown,
    handleClickSearch,
} from '../Actions/QuickSearch.action';
import usePrevious from '@react-hook/previous';
import { debounce } from 'lodash-es';

// debouncing function to 200ms so we don't send query request on every key stroke
const debouncedQuery = debounce((dispatch, text) => dispatch(query(text)), 200);

const QuickSearch = () => {
    const {
        query,
        result,
        showResult,
        showFullForm,
        selectedItem,
    } = useSelector((state) => state.quickSearch);
    const [lastClickedNode, setLastClickedNode] = useState(null);
    const searchUrl =
        window.__litium.quickSearchUrl +
        (query.length > 0 ? `?q=${query}` : '');
    const searchContainer = useRef(null);
    const searchInput = useRef(null);
    const dispatch = useDispatch();
    const previousSelectedItem = usePrevious(selectedItem);

    const clickHandler = useCallback((event) => {
        setLastClickedNode(event.target);
    }, []);
    useEffect(() => {
        // listen for click event to hide the search when clicking outside
        window.addEventListener('mousedown', clickHandler);
        return () => window.removeEventListener('mousedown', clickHandler);
    }, [clickHandler]);

    useEffect(() => {
        // set query value if it is avaialble in the Url
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.has('q')) {
            const query = urlParams.get('q');
            dispatch(setSearchQuery(query));
        }
    }, [dispatch]);

    useEffect(() => {
        if (selectedItem !== previousSelectedItem) {
            const el = document.querySelector(
                '.quick-search-result__item--selected'
            );
            el &&
                el.scrollIntoView({
                    behavior: 'smooth',
                    block: 'end',
                    inline: 'nearest',
                });
        }
    }, [selectedItem, previousSelectedItem]);

    return (
        <div className="quick-search" role="search" ref={searchContainer}>
            <a
                className="quick-search__link--block"
                rel="nofollow"
                href="#"
                title={translate('general.search')}
                onClick={(e) => {
                    e.preventDefault();
                    dispatch(toggleShowFullForm());
                    setTimeout(() => {
                        searchInput.current && searchInput.current.focus();
                    }, 0);
                }}
            >
                <i className="quick-search__icon"></i>
                <span className="quick-search__title hide">
                    {translate('general.search')}
                </span>
            </a>
            <div
                className={`quick-search__form ${
                    showFullForm ? 'quick-search__form--force-show' : ''
                }`}
                role="search"
            >
                <i
                    className="quick-search__icon"
                    onClick={(e) => dispatch(toggleShowFullForm())}
                ></i>
                <input
                    className="quick-search__input"
                    type="search"
                    placeholder={translate('general.search')}
                    autoComplete="off"
                    value={decodeURIComponent(query)}
                    onChange={(event) => {
                        const text = encodeURIComponent(event.target.value);
                        dispatch(setSearchQuery(text));
                        debouncedQuery(dispatch, text);
                    }}
                    onKeyDown={(event) =>
                        dispatch(handleKeyDown(event, { searchUrl }))
                    }
                    ref={searchInput}
                    onBlur={() => {
                        if (
                            searchContainer.current &&
                            !searchContainer.current.contains(lastClickedNode)
                        ) {
                            showFullForm && dispatch(toggleShowFullForm());
                        }
                    }}
                />
                <button
                    className="quick-search__submit-button"
                    type="button"
                    onClick={(e) => {
                        e.preventDefault();
                        dispatch(handleClickSearch({ searchUrl }));
                    }}
                >
                    <i className="quick-search__submit-icon"></i>
                    <span className="quick-search__submit-title">
                        {translate('general.search')}
                    </span>
                </button>
                {showResult && (
                    <QuickSearchResult
                        result={result}
                        selectedItem={selectedItem}
                        searchUrl={searchUrl}
                    />
                )}
            </div>
        </div>
    );
};

export default QuickSearch;
