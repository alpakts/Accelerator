export interface ISearchIndexingModel {
    groupedTemplates: ITemplateGroupModel[],
    message: string,
}

export interface ITemplateGroupModel {
    title: string;
    templates: ITemplateModel[]
}

export interface ITemplateModel {
    title: string;
    fields: IFieldModel[],
    selectedFields: IFieldModel[],
    templateId: string,
    areaType: string,
}

export interface IFieldModel {
    title: string;
    fieldId: string,
}