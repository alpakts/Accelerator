import { HttpClient } from '@angular/common/http';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    HostListener,
    OnInit
} from '@angular/core';
import { UntypedFormArray, UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import {
    ComponentCanDeactivate,
    DialogService,
    ErrorActions,
    NOTIFICATION_STATUS,
    NotificationActions,
    shallowClone
} from 'litium-ui';
import { BehaviorSubject } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { IIndexingModel, ITemplate, IFieldGroup } from '../../interfaces/indexingModel';

@Component({
    selector: 'app-grouping',
    templateUrl: 'grouping.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})

export class GroupingComponent implements OnInit, ComponentCanDeactivate {
    /**
     * Flag to show loading indicator when loading or submitting data.
     */
    loading$ = new BehaviorSubject<boolean>(true);

    /**
     * The form, which is built using FormBuilder.
     * https://angular.io/guide/reactive-forms#generating-form-controls-with-formbuilder
     */
    form: UntypedFormGroup;

    private _groupingFormErrorKey = 'groupingFormErrorKey';
    private _serviceUrl = '/site/administration/extensions/api/indexing';
    private _data: IIndexingModel;
    private _backupValue: IIndexingModel;

    /**
     * A function to extract Id from a field
     */
    idSelector = (field: IFieldGroup) => field ? field.fieldId : '';

    /**
     * A function to extract title from a field. A field can either by
     * an `IFieldGroup` or a `string`, cause it is used both in the single
     * select and in the tags component.
     */
    textSelector = (fields: IFieldGroup[]) => (field: IFieldGroup | string) => {
        const id = typeof field === 'string' ? field : field.fieldId;
        const item = fields.find(f => f.fieldId === id);
        return item ? item.title : id;
    }

    constructor(private _http: HttpClient,
        private _fb: UntypedFormBuilder,
        private _errorActions: ErrorActions,
        private _notificationActions: NotificationActions,
        private _dialogService: DialogService,
        private _cdr: ChangeDetectorRef) { }

    ngOnInit() {
        // Use FormBuilder to create form based on the server's response:
        // https://angular.io/guide/reactive-forms#generating-form-controls-with-formbuilder
        this.form = this._fb.group({
            templates: this._fb.array([]),
        });

        this._http.get<IIndexingModel>(this._serviceUrl)
            .pipe(finalize(() => this.loading$.next(false)))
            .subscribe(
                data => this.data = data,
                error => this._errorActions.setErrorDispatch(this._groupingFormErrorKey, error));
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
     * Gets the inner data, which is used to render the field select.
     */
    get data(): any {
        return this._data;
    }

    /**
     * Sets the inner data.
     */
    set data(data: any) {
        this._data = data;
        this._generateTemplateForms(this._data);
    }

    /**
     * A function that defines how to track changes for items in the iterable
     * https://angular.io/api/common/NgForOf#ngForTrackBy
     */
    trackById = (index, item) => item.templateId || index;

    private _generateTemplateForms(data: IIndexingModel) {
        if (this._templateFormArray.length === 0) {
            data.templates.forEach(template => this._templateFormArray.push(this._createTemplateForm(template)));
        } else {
            this.form.patchValue({
                'templates': data.templates,
            });
        }
        this._backupValue = shallowClone(this.form.value);
        this._cdr.markForCheck();
    }

    private _createTemplateForm(template: ITemplate): UntypedFormGroup {
        return this._fb.group({
            templateId: template.templateId,
            groupingFieldId: [template.groupingFieldId],
        });
    }

    private get _templateFormArray(): UntypedFormArray {
        return this.form.get('templates') as UntypedFormArray;
    }

    /**
     * Resets form's value to its backup value.
     */
    reset() {
        this.form.reset(this._backupValue);
    }

    /**
     * Submits form's value to the server.
     */
    submit() {
        this.loading$.next(true);
        this._http.put<IIndexingModel>(this._serviceUrl, this.form.value)
            .pipe(finalize(() => this.loading$.next(false)))
            .subscribe(
                data => {
                    this.form.markAsPristine(); // to disable reset and submit buttons
                    this.data = data;
                    this._notificationActions.addMessageDispatch(data.message, NOTIFICATION_STATUS.SUCCESS);
                },
                error => this._errorActions.setErrorDispatch(this._groupingFormErrorKey, error));
    }
}
