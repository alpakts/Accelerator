import React, { Fragment } from 'react';

const PaginationItem = ({
    name = '',
    current = false,
    disabled = false,
    onChange,
}) => {
    const cssClass = `pagination__link 
    ${current ? 'pagination__link--current' : ''} 
    ${disabled ? 'pagination__link--disabled' : ''}`;

    return (
        <li className="pagination__item">
            <a className={cssClass.trim()} onClick={() => onChange()}>
                {name}
            </a>
        </li>
    );
};

const RenderFirstHalfItems = ({
    intervalStart,
    edgeEntries,
    currentPageIndex,
    onChange,
}) => {
    const renderItems = [];
    const end = Math.min(edgeEntries, intervalStart);
    for (let i = 0; i < end; i++) {
        const publicPageIndex = i + 1;
        renderItems.push(
            <PaginationItem
                key={publicPageIndex}
                name={publicPageIndex}
                current={publicPageIndex === currentPageIndex}
                onChange={() => onChange(publicPageIndex)}
            />
        );
    }
    if (edgeEntries < intervalStart) {
        renderItems.push(
            <PaginationItem key="first_indicator" name="..." disabled={true} />
        );
    }
    return renderItems;
};

const RenderMiddleItems = ({
    intervalStart,
    intervalEnd,
    currentPageIndex,
    onChange,
}) => {
    const renderItems = [];
    for (let i = intervalStart; i < intervalEnd; i++) {
        const publicPageIndex = i + 1;
        renderItems.push(
            <PaginationItem
                key={publicPageIndex}
                name={publicPageIndex}
                current={publicPageIndex === currentPageIndex}
                onChange={() => onChange(publicPageIndex)}
            />
        );
    }
    return renderItems;
};

const RenderSecondHalfItems = ({
    intervalEnd,
    edgeEntries,
    pageCount,
    currentPageIndex,
    onChange,
}) => {
    const renderItems = [];
    if (pageCount - edgeEntries > intervalEnd) {
        renderItems.push(
            <PaginationItem key="second_indicator" name="..." disabled={true} />
        );
    }
    const begin = Math.max(pageCount - edgeEntries, intervalEnd);
    for (let i = begin; i < pageCount; i++) {
        const publicPageIndex = i + 1;
        renderItems.push(
            <PaginationItem
                key={publicPageIndex}
                name={publicPageIndex}
                current={publicPageIndex === currentPageIndex}
                onChange={() => onChange(publicPageIndex)}
            />
        );
    }
    return renderItems;
};

const Pagination = ({ model, onChange }) => {
    const {
        currentPageIndex,
        pageCount,
        intervalStart,
        intervalEnd,
        edgeEntries,
    } = model;

    return (
        <Fragment>
            {pageCount > 1 && (
                <ul className="pagination">
                    {currentPageIndex > 1 && (
                        <PaginationItem
                            name="<<"
                            current={false}
                            disabled={false}
                            onChange={() => onChange(currentPageIndex - 1)}
                        />
                    )}
                    {intervalStart > 0 && edgeEntries > 0 && (
                        <RenderFirstHalfItems
                            intervalStart={intervalStart}
                            edgeEntries={edgeEntries}
                            currentPageIndex={currentPageIndex}
                            onChange={onChange}
                        />
                    )}
                    <RenderMiddleItems
                        intervalStart={intervalStart}
                        intervalEnd={intervalEnd}
                        currentPageIndex={currentPageIndex}
                        onChange={onChange}
                    />
                    {intervalEnd < pageCount && edgeEntries > 0 && (
                        <RenderSecondHalfItems
                            intervalEnd={intervalEnd}
                            pageCount={pageCount}
                            edgeEntries={edgeEntries}
                            currentPageIndex={currentPageIndex}
                            onChange={onChange}
                        />
                    )}
                    {currentPageIndex < pageCount && (
                        <PaginationItem
                            name=">>"
                            current={false}
                            disabled={false}
                            onChange={() => onChange(currentPageIndex + 1)}
                        />
                    )}
                </ul>
            )}
        </Fragment>
    );
};

export default Pagination;
