import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { ContactsService, FormsService, type PostContactsAuthorizeData } from "@umbraco-integrations/hubspot/generated";

export class HubspotRepository extends UmbControllerBase{
    constructor(host: UmbControllerHost) {
        super(host);
    }

    async isAuthorizationConfigured() {
        const { data, error } = await tryExecute(this, ContactsService.getContactsAuthConfigured());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getAuthenticationUrl() {
        const { data, error } = await tryExecute(this, ContactsService.getContactsAuthUrl());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async authorize(code: string) {
        const { data, error } = await tryExecute(this, ContactsService.postContactsAuthorize({ body: { code: code } }));

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async deauthorize() {
        const { data, error } = await tryExecute(this, ContactsService.postContactsDeauthorize());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getAll() {
        const { data, error } = await tryExecute(this, ContactsService.getContactsProperties());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getFormFields(formId: string) {
        const { data, error } = await tryExecute(this, FormsService.getFormsFields({ query: { formId } }));

        if (error || !data) {
            return { error };
        }

        return { data };
    }
}