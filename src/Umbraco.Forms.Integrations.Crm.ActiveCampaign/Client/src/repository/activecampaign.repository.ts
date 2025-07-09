import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { AccountsService, ContactsService, FormsService, GetFormsFieldsData } from "@umbraco-integrations/activecampaign/generated";

export class ActiveCampaignRepository extends UmbControllerBase {
    constructor(host: UmbControllerHost) {
        super(host);
    }

    async getAccount() {
        const { data, error } = await tryExecute(this, AccountsService.getAccounts());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async checkApiAccess() {
        const { data, error } = await tryExecute(this, ContactsService.getContactsApiAccess());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getContactFields() {
        const { data, error } = await tryExecute(this, ContactsService.getContactsFields());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getCustomFields() {
        const { data, error } = await tryExecute(this, ContactsService.getContactsCustom());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getFormFields(formId: string) {
        const { data, error } = await tryExecute(this, FormsService.getFormsFields({ query: { formId: formId } }));

        if (error || !data) {
            return { error };
        }

        return { data };
    }
}