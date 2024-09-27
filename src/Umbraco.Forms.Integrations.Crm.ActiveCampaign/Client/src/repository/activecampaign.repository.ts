import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
import { AccountsService, ContactsService, FormsService, GetFormFieldsData } from "@umbraco-integrations/activecampaign/generated";

export class ActiveCampaignRepository extends UmbControllerBase {
    constructor(host: UmbControllerHost) {
        super(host);
    }

    async getAccount() {
        const { data, error } = await tryExecuteAndNotify(this, AccountsService.getAccounts());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async checkApiAccess() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.checkApiAccess());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getContactFields() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.getContactFields());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getCustomFields() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.getCustomFields());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getFormFields(formId: string) {
        const { data, error } = await tryExecuteAndNotify(this, FormsService.getFormFields({formId: formId}));

        if (error || !data) {
            return { error };
        }

        return { data };
    }
}