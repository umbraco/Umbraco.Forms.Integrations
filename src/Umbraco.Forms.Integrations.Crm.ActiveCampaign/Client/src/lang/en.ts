import type { UmbLocalizationDictionary } from "@umbraco-cms/backoffice/localization-api";
export default {
    formProviderWorkflows:{
        AccountLabel : `Account`,
        AccountDescription: `Please select an account`,
        ContactMappingsLabel: `Contact Mappings`,
        ContactMappingsDescription: `Map contact details with form fields`,
        CustomFieldMappingsLabel: `Custom Fields Mappings`,
        CustomFieldMappingsDescription: `Map contact custom fields with form fields`
    },
    activeCampaignFormWorkflows: {
        DeleteMapping: "Delete",
        AddMapping: "Add mapping",
        SelectContactField: "Select contact field",
        SelectFormField: "Select form field",
        SelectCustomField: "Select custom field",
        ContactField: "Contact Field",
        FormField: "Form Field",
        CustomField: "Custom Field"
    }
} as UmbLocalizationDictionary;