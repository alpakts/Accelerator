export interface IIndexingModel {
    templates: ITemplate[];
    message: string;
}

export interface ITemplate {
    title: string;
    groupingFieldId: string;
    fields: IFieldGroup[];
    templateId: string;
}

export interface IFieldGroup {
    title: string;
    fieldId: string;
}
