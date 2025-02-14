import React, { Fragment } from 'react';

const sameCategory = (item, index, array) =>
    (index > 0 && array[index - 1].category === array[index].category) ||
    item.showAll;

const QuickSearchResult = ({ result, selectedItem, searchUrl }) => (
    <ul className="quick-search-result">
        {result &&
            result.map((item, index, array) => (
                <Fragment key={`${item.name}-${index}`}>
                    {item.category === 'NoHit' ||
                    sameCategory(item, index, array) ? null : (
                        <li className="quick-search-result__item quick-search-result__group-header">
                            {item.category}
                        </li>
                    )}
                    <li
                        className={`quick-search-result__item ${
                            selectedItem === index
                                ? 'quick-search-result__item--selected'
                                : ''
                        }`}
                    >
                        <a
                            className={
                                item.showAll
                                    ? 'quick-search-result__show-all'
                                    : `quick-search-result__link ${
                                          item.url
                                              ? ''
                                              : 'quick-search-result__link--disabled'
                                      }`
                            }
                            href={item.showAll ? searchUrl : item.url}
                        >
                            {item.hasImage && item.imageSource && (
                                <img
                                    className="quick-search-result__image"
                                    src={item.imageSource}
                                />
                            )}
                            <div>{item.name}</div>
                        </a>
                    </li>
                </Fragment>
            ))}
    </ul>
);

export default QuickSearchResult;
