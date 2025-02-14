import React, { Fragment, useCallback, useEffect, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { translate } from '../Services/translation';
import { query, searchFacetChange } from '../Actions/FacetedSearch.action';
import { useStateWithCallbackLazy } from 'use-state-with-callback';

const FacetedSearchGroup = ({ group }) => {
    const [collapsed, setCollapsed] = useStateWithCallbackLazy(false);
    const showLessItemCount = useRef(5);
    const list = useRef(null);
    const showLess = useRef(null);
    const showMore = useRef(null);
    const listTop = useRef(null);
    const showLessBottom = useRef(null);
    const showMoreBottom = useRef(null);
    const firstRender = useRef(true);

    const toggleShowMore = useCallback(() => {
        setCollapsed(
            (prevState) => !prevState,
            (collapsed) => {
                list.current.style.height = `${
                    (collapsed
                        ? showLessBottom.current
                        : showMoreBottom.current) - listTop.current
                }px`;
            }
        );
    }, [setCollapsed]);

    useEffect(() => {
        // Make sure it is executed only once.
        // firstRender is used to prevent eslint warning when having empty dependencies array
        if (!firstRender.current) {
            return;
        }
        firstRender.current = false;

        const {
            height: listHeight,
            top: listPosTop,
        } = list.current.getBoundingClientRect();

        listTop.current = listPosTop;
        showLessBottom.current = showLess.current
            ? showLess.current.getBoundingClientRect().bottom
            : null;
        showMoreBottom.current = showMore.current.getBoundingClientRect().bottom;

        const visible = listHeight !== 0;
        const tooMuchItem = group.options.length > showLessItemCount.current;
        visible && tooMuchItem && toggleShowMore();
    }, [group, toggleShowMore]);

    return (
        <Fragment>
            <ul className="faceted-search__group" role="group" ref={list}>
                <div className="faceted-search__group-header">
                    {group.label}
                </div>
                {group.options &&
                    group.options.map((item, itemIndex, array) => (
                        <li
                            key={`${item.label}-${itemIndex}`}
                            className="faceted-search__item"
                            role="menuitemcheckbox"
                            ref={(elem) => {
                                if (
                                    itemIndex ===
                                    showLessItemCount.current - 1
                                ) {
                                    showLess.current = elem;
                                }
                                if (itemIndex === array.length - 1) {
                                    showMore.current = elem;
                                }
                            }}
                        >
                            <FacetedFilterCheckbox item={item} group={group} />
                        </li>
                    ))}
            </ul>
            {group.options.length > showLessItemCount.current && (
                <span
                    className="faceted-search__show-more"
                    onClick={toggleShowMore}
                >
                    {`${
                        collapsed
                            ? translate('filter.showmore')
                            : translate('filter.showless')
                    }`}
                </span>
            )}
        </Fragment>
    );
};

const FacetedFilterCheckbox = ({ item, group }) => {
    const dispatch = useDispatch();
    const onChange = useCallback(
        (group, item) => dispatch(searchFacetChange(group, item)),
        [dispatch]
    );
    return (
        <label className="faceted-filter">
            <input
                className="faceted-filter__input"
                type="checkbox"
                onChange={(event) => onChange(group, item)}
                checked={
                    group.selectedOptions != null &&
                    group.selectedOptions.includes(item.id)
                }
            />
            <span className="faceted-filter__label">
                {item.label}
                {!isNaN(item.quantity) && item.quantity != null && (
                    <span className="faceted-filter__quantity">
                        &nbsp;({item.quantity})
                    </span>
                )}
            </span>
        </label>
    );
};

const FacetedSearch = () => {
    const dispatch = useDispatch();
    const { facetFilters, navigationTheme } = useSelector(
        (state) => state.facetedSearch
    );

    useEffect(() => {
        // listen to history events (back, forward) of window
        window.onpopstate =
            window.onpopstate ||
            ((event) => {
                dispatch(query(window.location.search.substr(1) || ''));
            });
    }, [dispatch]);

    return (
        navigationTheme !== 'category' &&
        facetFilters &&
        facetFilters.length > 0 && (
            <div className="faceted-search" role="menu">
                {facetFilters.map((group, groupIndex) => (
                    <FacetedSearchGroup
                        key={`${group.label}-${groupIndex}`}
                        group={group}
                    />
                ))}
            </div>
        )
    );
};

export default FacetedSearch;
