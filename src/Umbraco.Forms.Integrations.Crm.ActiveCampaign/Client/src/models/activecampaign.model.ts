export interface ContactMappingValue {
    contactField: string;
    formField: FormFieldValue | undefined;
}

export interface FormFieldValue {
    id: string;
    value: string;
}