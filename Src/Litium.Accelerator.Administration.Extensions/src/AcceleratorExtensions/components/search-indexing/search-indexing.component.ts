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
import { ISearchIndexingModel, ITemplateGroupModel, ITemplateModel } from '../../interfaces/searchIndexingModel';

@Component({
    selector: 'app-search-indexing.component',
    templateUrl: 'search-indexing.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchIndexingComponent implements ComponentCanDeactivate, OnInit {

    /**
     * Flag to show loading indicator when loading or submitting data.
     */
    loading$ = new BehaviorSubject<boolean>(true);

    /**
     * The form, which is built using FormBuilder.
     * https://angular.io/guide/reactive-forms#generating-form-controls-with-formbuilder
     */
    form: UntypedFormGroup;

    private _data: ISearchIndexingModel;
    private _backupData: ISearchIndexingModel;
    private _serviceUrl = '/site/administration/extensions/api/searchindexing';
    private _searchIndexingErrorKey = 'searchIndexingErrorKey';

    constructor(
        private _http: HttpClient,
        private _fb: UntypedFormBuilder,
        private _cdr: ChangeDetectorRef,
        private _errorActions: ErrorActions,
        private _dialogService: DialogService,
        private _notificationActions: NotificationActions) {}

    ngOnInit() {
        // Use FormBuilder to create form based on the server's response:
        // https://angular.io/guide/reactive-forms#generating-form-controls-with-formbuilder
        this.form = this._fb.group({
            groupedTemplates: this._fb.array([]),
        });

        this._http.get<ISearchIndexingModel>(this._serviceUrl)
            .pipe(finalize(() => this.loading$.next(false)))
            .subscribe(
                data => this.data = data,
                error => this._errorActions.setErrorDispatch(this._searchIndexingErrorKey, error));
    }

    get data(): ISearchIndexingModel {
        return this._data;
    }

    set data(value: ISearchIndexingModel) {
        this._data = value;
        this._generateForms(this._data);
    }

    private _generateForms(data: ISearchIndexingModel) {
        if (this._templateGroupArray.length === 0) {
            data.groupedTemplates.forEach(group => this._templateGroupArray.push(this._createTemplateGroupForm(group)));
        } else {
            this.form.patchValue({
                'groupedTemplates': data.groupedTemplates,
            });
        }
        this._backupData = shallowClone(this.form.value);
        this._cdr.markForCheck();
    }

    private _createTemplateGroupForm(group: ITemplateGroupModel): UntypedFormGroup {
        return this._fb.group({
            templates: this._fb.array(group.templates.map(template => this._createTemplateForm(template))),
        });
    }

    private _createTemplateForm(template: ITemplateModel): UntypedFormGroup {
        return this._fb.group({
            templateId: template.templateId,
            areaType: template.areaType,
            selectedFields: [template.selectedFields],
        });
    }

    private get _templateGroupArray(): UntypedFormArray {
        return this.form.get('groupedTemplates') as UntypedFormArray;
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
     * Id selector for select box
     */
    idSelector = item => item.fieldId;

    /**
     * Text, title selector for select box, tags
     */
    textSelector = item => item.title;


    /**
     * A function that defines how to track changes for items in the iterable
     * https://angular.io/api/common/NgForOf#ngForTrackBy
     */
    trackById = (index, item) => item.templateId || index;
    trackByTitle = (index, item) => item.title || index;

    /**
     * Submits form's value to the server.
     */
    submit() {
        this.loading$.next(true);

        this._http.put<ISearchIndexingModel>(this._serviceUrl, this.form.value)
            .pipe(finalize(() => this.loading$.next(false)))
            .subscribe(
                (data) => {
                    this.form.markAsPristine(); // to disable reset and submit buttons
                    this.data = data;
                    this._notificationActions.addMessageDispatch(data.message, NOTIFICATION_STATUS.SUCCESS);
                },
                error => this._errorActions.setErrorDispatch(this._searchIndexingErrorKey, error));
    }

    /**
     * Resets form's value to its backup value.
     */
    reset() {
        this.form.reset(this._backupData);
    }
}
