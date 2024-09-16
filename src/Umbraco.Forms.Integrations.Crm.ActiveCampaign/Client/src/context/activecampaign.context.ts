import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { ActiveCampaignRepository } from "../repository/activecampaign.repository";
import { ApiAccessDto } from "@umbraco-integrations/activecampaign/generated"

export class ActiveCampaignContext extends UmbControllerBase {
    #repository: ActiveCampaignRepository;
    #configurationModel = new UmbObjectState<ApiAccessDto | undefined>(undefined);
    configurationModel = this.#configurationModel.asObservable();

    constructor(host: UmbControllerHost) {
        super(host);

        this.provideContext(ACTIVECAMPAIGN_FORMS_CONTEXT_TOKEN, this);
        this.#repository = new ActiveCampaignRepository(host);
    }

    async hostConnected() {
        super.hostConnected();
        this.checkApiAccess();
    }

    async checkApiAccess() {
        const { data } = await this.#repository.checkApiAccess();

        this.#configurationModel.setValue(data);
    }

    async getAccount() {
        return await this.#repository.getAccount();
    }

    async getContactFields() {
        return await this.#repository.getContactFields();
    }

    async getCustomFields() {
        return await this.#repository.getCustomFields();
    }
}

export default ActiveCampaignContext;

export const ACTIVECAMPAIGN_FORMS_CONTEXT_TOKEN =
    new UmbContextToken<ActiveCampaignContext>(ActiveCampaignContext.name);