import { NgRedux } from '@angular-redux3/store';
import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnInit,
} from '@angular/core';
import { FieldEditorFieldSelector, IAppState } from 'litium-ui';
import { FilterFieldsActions } from '../../redux/actions/accelerator-filter-fields.action';

@Component({
    selector: 'app-field-editor-filter-fields',
    templateUrl: './field-editor-filter-fields.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldEditorFilterFieldsComponent
    extends FieldEditorFieldSelector
    implements OnInit
{
    sortable = true;

    constructor(
        ngRedux: NgRedux<IAppState>,
        changeDetection: ChangeDetectorRef,
        private _filterFieldsActions: FilterFieldsActions
    ) {
        super(ngRedux, changeDetection);
    }

    ngOnInit() {
        super.ngOnInit();
        this._filterFieldsActions.getDispatch();
    }

    public get viewItems(): any[] {
        return this.getValue(this.viewLanguage);
    }

    public set viewItems(value: any[]) {
        if (this.valueAsDictionary) {
            this.value[this.viewLanguage] = value;
        } else {
            this.value = value;
        }
    }

    idSelector = (field) => field.id || field.fieldId;

    stateSelector = (state: IAppState) => this.fieldsSelector(state);

    protected fieldsSelector(state: IAppState) {
        return state['accelerator']?.productfiltering.items || [];
    }
}
