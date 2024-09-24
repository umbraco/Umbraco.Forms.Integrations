import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { HubspotRepository } from "../repository/hubspot.repository";
import { type AuthorizeData } from "@umbraco-integrations/hubspot/generated";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";

export class HubspotContext extends UmbControllerBase{
    #repository: HubspotRepository;

    #settingsModel = new UmbObjectState<string | undefined>(undefined);
    settingsModel = this.#settingsModel.asObservable();
    
    constructor(host: UmbControllerHost) {
        super(host);

        this.provideContext(HUBSPOT_CONTEXT_TOKEN, this);
        this.#repository = new HubspotRepository(host);
    }

    async hostConnected() {
        super.hostConnected();
        this.isAuthorizationConfigured();
    }

    async isAuthorizationConfigured() {
        const { data } = await this.#repository.isAuthorizationConfigured();
        this.#settingsModel.setValue(data);
    }

    async getAuthenticationUrl() {
        return await this.#repository.getAuthenticationUrl();
    }
    
    async authorize(code: string) {
        return await this.#repository.authorize(code);
    }

    async deauthorize() {
        return await this.#repository.deauthorize();
    }

    async getAll() {
        return await this.#repository.getAll();
    }

    async getFormFields(formId : string) {
        return await this.#repository.getFormFields(formId);
    }
}

export default HubspotContext;

export const HUBSPOT_CONTEXT_TOKEN =
    new UmbContextToken<HubspotContext>(HubspotContext.name);