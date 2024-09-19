export interface ContactMappingValue {
    contactField: string;
    formField: FormFieldValue | undefined;
}

export interface FormFieldValue {
    id: string;
    value: string;
}

export interface CustomMappingValue {
    customField: string;
    formField: FormFieldValue | undefined;
}