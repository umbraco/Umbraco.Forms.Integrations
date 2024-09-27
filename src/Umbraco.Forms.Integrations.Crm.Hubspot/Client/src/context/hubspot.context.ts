import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { HubspotRepository } from "../repository/hubspot.repository";
import { type AuthorizeData } from "@umbraco-integrations/hubspot/generated";

export class HubspotContext extends UmbControllerBase{
    #repository: HubspotRepository;

    constructor(host: UmbControllerHost) {
        super(host);

        this.provideContext(HUBSPOT_CONTEXT_TOKEN, this);
        this.#repository = new HubspotRepository(host);
    }

    async hostConnected() {
        super.hostConnected();
    }

    async isAuthorizationConfigured() {
        return await this.#repository.isAuthorizationConfigured();
    }

    async getAuthenticationUrl() {
        return await this.#repository.getAuthenticationUrl();
    }
    
    async authorize(authData: AuthorizeData) {
        return await this.#repository.authorize(authData);
    }

    async deauthorize() {
        return await this.#repository.deauthorize();
    }

    async getAll() {
        return await this.#repository.getAll();
    }
}

export default HubspotContext;

export const HUBSPOT_CONTEXT_TOKEN =
    new UmbContextToken<HubspotContext>(HubspotContext.name);