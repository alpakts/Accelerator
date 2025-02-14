import { FilterFieldsActions } from "../actions/accelerator-filter-fields.action";

export interface IFiltering {
    items: any[];
}

export const INIT_STATE: IFiltering = {
    items: [],
}

export function filterFieldReducer(state = INIT_STATE , action): IFiltering {
    const { type, payload } = action;
    switch (type) {
        case FilterFieldsActions.ACCELERATOR_FILTER_FIELDS_GET_REQUEST:
        case FilterFieldsActions.ACCELERATOR_FILTER_FIELDS_GET_SUCCESS:
            return {
                ...state,
                ...payload,
            };
        default:
            return state;
    }
}