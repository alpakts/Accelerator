import React, { Fragment } from 'react';

const SortCriteriaCompact = ({ sortCriteria }) => {
    const selectedOption = [
        ...sortCriteria.filter((sort) => sort.selected === true),
        {},
    ][0];
    return (
        <div className="columns">
            <select
                value={selectedOption.value}
                className="form__input"
                aria-label="Sort"
                onChange={(event) => (window.location = event.target.value)}
            >
                {sortCriteria &&
                    sortCriteria.map(({ text, value, selected }, index) => (
                        <option value={value} key={`sorteriaCompact-${index}`}>
                            {text}
                        </option>
                    ))}
            </select>
        </div>
    );
};

export default SortCriteriaCompact;
