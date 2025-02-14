import React from 'react';
import { useDispatch } from 'react-redux';
import { searchFacetChange } from '../Actions/FacetedSearch.action';
import { translate } from '../Services/translation';

const FilterTags = ({ facetFilters, navigationTheme }) => {
    const dispatch = useDispatch();

    const selectedFacetGroup = facetFilters.filter(
        (filter) => filter.selectedOptions.length > 0
    );
    const selectedFacetOption = selectedFacetGroup.reduce(
        (accumulator, group) => {
            if (!group) {
                return accumulator;
            }
            const { options, selectedOptions } = group;
            const groupOptions = options
                .filter((option) => selectedOptions.includes(option.id))
                .map((option) => ({ ...option, group }));
            return [...accumulator, ...groupOptions];
        },
        []
    );
    return selectedFacetGroup.length === 0 ? null : (
        <div className="selected-filter hide-for-large">
            <span>
                <span> {translate('search.yourfilter')} : </span>
                {selectedFacetGroup &&
                    selectedFacetGroup.map((group, index) => (
                        <span
                            className="selected-filter__tag"
                            onClick={(event) =>
                                dispatch(searchFacetChange(group))
                            }
                            key={`group-${index}`}
                        >
                            {' '}
                            {group.label}{' '}
                        </span>
                    ))}
            </span>
            {navigationTheme === 'category' && (
                <span className="show-for-large">
                    <span> {translate('search.yourfilter')} : </span>
                    {selectedFacetOption &&
                        selectedFacetOption.map((option, index) => (
                            <span
                                className="selected-filter__tag"
                                onClick={(event) =>
                                    dispatch(
                                        searchFacetChange(option.group, option)
                                    )
                                }
                                key={`option-${index}`}
                            >
                                {' '}
                                {option.label}{' '}
                            </span>
                        ))}
                </span>
            )}
        </div>
    );
};

export default FilterTags;
