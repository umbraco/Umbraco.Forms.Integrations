import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
import { ContactsService, type AuthorizeData } from "@umbraco-integrations/hubspot/generated";

export class HubspotRepository extends UmbControllerBase{
    constructor(host: UmbControllerHost) {
        super(host);
    }

    async isAuthorizationConfigured() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.isAuthorizationConfigured());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getAuthenticationUrl() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.getAuthenticationUrl());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async authorize(authData: AuthorizeData) {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.authorize(authData));

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async deauthorize() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.deauthorize());

        if (error || !data) {
            return { error };
        }

        return { data };
    }

    async getAll() {
        const { data, error } = await tryExecuteAndNotify(this, ContactsService.getAll());

        if (error || !data) {
            return { error };
        }

        return { data };
    }
}