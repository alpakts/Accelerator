export interface IFilteringItemModel {
    title: string;
    fieldId: string;
    groupName: string
}

export interface IFilteringModel {
    items: IFilteringItemModel[];
    filters: IFilteringItemModel[];
    message: string
}