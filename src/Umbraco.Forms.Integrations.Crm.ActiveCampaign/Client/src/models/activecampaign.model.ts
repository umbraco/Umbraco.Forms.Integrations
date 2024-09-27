export interface ContactMappingValue {
    contactField: string;
    formField: FormFieldValue | undefined;
}

export interface FormFieldValue {
    id: string;
    value: string;
}

export interface CustomMappingValue {
    customField: CustomFieldValue | undefined;
    formField: FormFieldValue | undefined;
}

export interface CustomFieldValue {
    id: string;
    title: string;
}