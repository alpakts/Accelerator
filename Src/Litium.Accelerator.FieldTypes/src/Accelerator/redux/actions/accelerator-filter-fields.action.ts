import { NgRedux } from '@angular-redux3/store';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ErrorActions, IAppState, LoadingActions } from 'litium-ui';
import { finalize } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class FilterFieldsActions {
    static ACCELERATOR_FILTER_FIELDS_GET_REQUEST =
        'ACCELERATOR_FILTER_FIELDS_GET_REQUEST';
    static ACCELERATOR_FILTER_FIELDS_GET_SUCCESS =
        'ACCELERATOR_FILTER_FIELDS_GET_SUCCESS';
    LoadingState = 'LoadAcceleratorFilterFields';

    constructor(
        private _ngRedux: NgRedux<IAppState>,
        private _http: HttpClient,
        private _errorActions: ErrorActions,
        private _loadingActions: LoadingActions
    ) {}

    getRequest() {
        return {
            type: FilterFieldsActions.ACCELERATOR_FILTER_FIELDS_GET_REQUEST,
            payload: {
                items: [],
            },
        };
    }

    getSuccess(items: any[]) {
        return {
            type: FilterFieldsActions.ACCELERATOR_FILTER_FIELDS_GET_SUCCESS,
            payload: {
                items,
            },
        };
    }

    get(serviceUrl: string) {
        return (dispatch) => {
            dispatch(this.getRequest());
            dispatch(
                this._loadingActions.setIsLoading(this.LoadingState, true)
            );
            return this._http
                .get<any>(serviceUrl)
                .pipe(
                    finalize(() =>
                        dispatch(
                            this._loadingActions.setIsLoading(
                                this.LoadingState,
                                false
                            )
                        )
                    )
                )
                .subscribe(
                    (data) => dispatch(this.getSuccess(data)),
                    (error) =>
                        dispatch(
                            this._errorActions.setError(
                                this.LoadingState,
                                error
                            )
                        )
                );
        };
    }

    getDispatch(serviceUrl = `/site/administration/extensions/api/filtering`) {
        this._ngRedux.dispatch<any>(this.get(serviceUrl));
    }
}
