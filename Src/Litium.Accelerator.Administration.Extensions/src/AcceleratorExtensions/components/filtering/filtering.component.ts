import { HttpClient } from '@angular/common/http';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    HostListener,
    OnInit
} from '@angular/core';
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import {
    ComponentCanDeactivate,
    DialogService,
    ErrorActions,
    NOTIFICATION_STATUS,
    NotificationActions,
    shallowClone
} from 'litium-ui';
import groupBy from 'lodash-es/groupBy';
import { BehaviorSubject } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { IFilteringModel } from '../../interfaces/filteringModel';

@Component({
    selector: 'app-filtering',
    templateUrl: 'filtering.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})

export class FilteringComponent implements OnInit, ComponentCanDeactivate {
    private _data: IFilteringModel;
    private _backupData: IFilteringModel;
    private _filteringErrorKey = 'filteringErrorKey';
    private _serviceUrl = '/site/administration/extensions/api/filtering/setting';

    /**
     * Flag to show loading indicator when loading or submitting data.
     */
    loading$ = new BehaviorSubject<boolean>(true);

    /**
     * The form, which is built using FormBuilder.
     * https://angular.io/guide/reactive-forms#generating-form-controls-with-formbuilder
     */
    form: UntypedFormGroup;

    idSelector = field => field.fieldId;

    constructor(private _http: HttpClient,
        private _fb: UntypedFormBuilder,
        private _errorActions: ErrorActions,
        private _cdr: ChangeDetectorRef,
        private _dialogService: DialogService,
        private _notificationActions: NotificationActions) {}

    set data(value: IFilteringModel) {
        this._data = value;
        this.form.patchValue({
            'items': value ? value.items : [],
        })
        this._backupData = shallowClone(this.form.value);
        this._cdr.markForCheck();
    }

    get data(): IFilteringModel {
        return this._data;
    }

    ngOnInit() {
        this.form = this._fb.group({
            items: [[]],
        });

        this._http.get<IFilteringModel>(this._serviceUrl)
            .pipe(finalize(() => this.loading$.next(false)))
            .subscribe(
                data => this.data = data,
                error => this._errorActions.setErrorDispatch(this._filteringErrorKey, error));
    }

    /**
     * Checks and returns false if user is leaving the component while having pending changes.
     * A confirmation dialog will be appeared if false is returned.
     * Decorated by HostListener to also show the confirmation when user leaving Angular.
     * For example, navigate to other pages.
     */
    @HostListener('window:beforeunload')
    canDeactivate(): boolean {
        return !this.form.dirty;
    }

    /**
     * Displays a confirmation dialog when user is leaving the component with pending changes.
     */
    confirmDeactivate(): Promise<boolean> {
        return this._dialogService.showConfirmation();
    }

    /**
     * Reset to backupData
     */
    reset() {
        this.form.reset(this._backupData);
    }

    /**
     * Return grouping items when getting filterOptions
     */
    get filterOptions(): any {
        if (!this.data) {
            return [];
        }

        const filtersGrouping = groupBy(this.data.filters, (item) => item.groupName);
        const filtersArr = [];
        for (const [groupName, groupItems] of Object.entries(filtersGrouping)) {
            filtersArr.push({
                title: groupName,
                isGroup: true
            });

            groupItems.forEach(item => filtersArr.push(item));
        }

        return filtersArr;
    }

    submit() {
        this.loading$.next(true);
        this._http.post<IFilteringModel>(this._serviceUrl, this.form.value)
            .pipe(finalize(() => this.loading$.next(false)))
            .subscribe(
                (data) => {
                    this.form.markAsPristine(); // to disable reset and submit buttons
                    this.data = data;
                    this._notificationActions.addMessageDispatch(data.message, NOTIFICATION_STATUS.SUCCESS);
                },
                error => this._errorActions.setErrorDispatch(this._filteringErrorKey, error));
    }
}
