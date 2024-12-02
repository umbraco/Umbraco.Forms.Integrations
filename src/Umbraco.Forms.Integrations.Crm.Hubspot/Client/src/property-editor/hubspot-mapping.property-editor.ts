import { html, customElement, property, css, when, state, map } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { HUBSPOT_CONTEXT_TOKEN } from '@umbraco-integrations/hubspot/context';
import { HubspotWorkflowFormFieldDto, Property } from '@umbraco-integrations/hubspot/generated';
import { UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { HubspotMappingValue } from '../models/hubspot.model';
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationColor } from '@umbraco-cms/backoffice/notification';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

const elementName = "hubspot-mapping-property-editor";

@customElement(elementName)
export class HubspotMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #hubspotContext!: typeof HUBSPOT_CONTEXT_TOKEN.TYPE;
  #settingsModel?: string;

  @property({ type: String })
  public value = "";

  @state()
  public hubspotMappingArray : Array<HubspotMappingValue> = [];

  @state()
  private hubspotFields: Array<Property> | undefined = [];

  @state()
  private formdFields: Array<HubspotWorkflowFormFieldDto> | undefined = [];

  @state()
  private authorizationCode: string = "";

  @state()
  private authorizationStatus: string = "Unauthenticated";

  @state()
  private authenticationUrl: string | undefined = "";

  constructor() {
    super();
    this.consumeContext(HUBSPOT_CONTEXT_TOKEN, (context) => {
        if (!context) return;
        this.#hubspotContext = context;

        this.observe(context.settingsModel, (settingsModel) => {
          this.#settingsModel = settingsModel;
      });
    });
  }

  async connectedCallback() {
    super.connectedCallback();

    if (!this.#settingsModel || this.#settingsModel === "Unauthenticated"){
      const { data } = await this.#hubspotContext.getAuthenticationUrl();
      this.authenticationUrl = data;
    } else{
      this.authorizationStatus = this.#settingsModel!;

      await this.loadData();
    }
  }

  async #getHubspotFields(){
    var result = await this.#hubspotContext.getAll();
    if (!result) return;

    this.hubspotFields = result.data;
  }

  async #getFormFields(){
    var formId = window.location.pathname.split("/")[7]; //Get the formid based on current url.
    var result = await this.#hubspotContext.getFormFields(formId);

    if (!result) return;

    this.formdFields = result.data;
  }

  async #openAuth(){
    window.open(this.authenticationUrl);
    window.addEventListener("message", async (event: MessageEvent) => {
      if (event.data.type === "hubspot:oauth:success") {
          this.authorizationCode = event.data.code;

          await this.#onConnect();
      }
    }, false);
  }

  async #onConnect(){
    const { data } = await this.#hubspotContext.authorize(this.authorizationCode);

    if (!data) return;

    if (data.success){
      this.authorizationStatus = "OAuth";
      this.authorizationCode = "";

      await this.loadData();

      this.requestUpdate();
      this.dispatchEvent(new CustomEvent("authorizationStatus"));
      this._showSuccess("Your Umbraco Forms installation is now connected to your HubSpot account");
    } else{
      this._showError(data.errorMessage);
    }
  }

  async loadData(){
    await this.#getHubspotFields();
    await this.#getFormFields();

    if (this.value){
      this.hubspotMappingArray = JSON.parse(this.value);
    }
  }

  async #deauthorize(){
    await umbConfirmModal(this, {
      color: "danger",
      headline: "Confirmation",
      content: "Are you sure you wish to disconnect your Hubspot account?",
      confirmLabel: 'Disconnect',
    });

    const { data } = await this.#hubspotContext.deauthorize();

    if(!data) return;

    if (data.success){
      this.authorizationStatus = "Unauthenticated";
      this.authorizationCode = "";

      const { data } = await this.#hubspotContext.getAuthenticationUrl();
      if (data){
        this.authenticationUrl = data;
      }

      this._showSuccess("Your Umbraco Forms installation is no longer connected to your HubSpot account");
    } else{
      this._showError(data.errorMessage!);
    }

    this.dispatchEvent(new CustomEvent("authorizationStatus"));
  }

  private async _showSuccess(message: string) {
    await this._showMessage(message, "positive");
  }

  private async _showError(message: string) {
    await this._showMessage(message, "danger");
  }

  private async _showMessage(message: string, color: UmbNotificationColor) {
    const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    notificationContext?.peek(color, {
        data: { message },
    });
  }

  #onDeleteClick(idx: number){
    this.hubspotMappingArray.splice(idx, 1);

    this.value = JSON.stringify(this.hubspotMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  #addButtonClick(){
    this.hubspotMappingArray.push({
      formField: "",
      hubspotField: "",
      appendValue: false
    });

    this.value = JSON.stringify(this.hubspotMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  #onHubspotSelectChange(e: UUISelectEvent, idx: number){
    this.hubspotMappingArray[idx].hubspotField = e.target.value.toString();

    this.value = JSON.stringify(this.hubspotMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  #onFormFieldSelectChange(e: UUISelectEvent, idx: number){
    this.hubspotMappingArray[idx].formField = e.target.value.toString();

    this.value = JSON.stringify(this.hubspotMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  #onInputChange(e: UUIInputEvent){
    this.authorizationCode = e.target.value.toString();
  }

  #getHubspotDescription(name: string){
    if (!this.hubspotFields) return;
    var result = this.hubspotFields.find(h => h.name == name);
    if (!result) return;
    return result.description;
  }

  render() {
    return html`
      ${this.authorizationStatus === "Unauthenticated"
        ? html`
          <div>
            <p>Umbraco Forms is not configured with a HubSpot CRM account.</p>
            <p>To do this you can either create and save an API key or a Private Access Token into the <i>appsettings.json</i> file.</p>
            <p>Or you can click <a class="hubspot-wf-auth-link" @click=${this.#openAuth} style="text-decoration: underline">here</a> to complete an OAuth connection.</p>
            <p><em>If your browser is unable to process the automated connection, paste the provided authorization code below and click to complete the authentication.</em></p>
            <uui-input placeholder="Enter authorization code" @change=${(e : UUIInputEvent) => this.#onInputChange(e)}></uui-input>
            <uui-button look="primary" type="button" ?disabled=${!this.authorizationCode} @click=${this.#onConnect} label="Authorize"></uui-button>
          </div>
        ` 
        : html`
          <div class="hubspot-wf-status">
              <span>Umbraco Forms is configured with a HubSpot CRM account using: </span>
              <b>${this.authorizationStatus}</b></p>
          </div>

          <div class="hubspot-wf-button">
            <table>
              <tr>
                <td>
                  <uui-button look="primary"} label="Add mapping" @click=${this.#addButtonClick}></uui-button>
                </td>
                <td>
                  <uui-button look="secondary"} label="De-authorize from Hubspot" @click=${this.#deauthorize}></uui-button>
                </td>
              </tr>
            </table>
          </div>

          <div>
            ${this.hubspotMappingArray.length > 0 
              ? html`
                <table class="hubspot-wf-table">
                  <thead>
                    <tr>
                      <th>Form Field</th>
                      <th>Hubspot Field</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    ${map(this.hubspotMappingArray, (mapping, idx) => html`
                      <tr>
                        <td>
                          <uui-select
                              placeholder=${this.localize.term("hubspotFormWorkflow_SelectFormField")}
                              @change=${(e : UUISelectEvent) => this.#onFormFieldSelectChange(e, idx)}
                              .options=${
                                this.formdFields?.map((ft) => ({
                                  name: ft.caption,
                                  value: ft.id,
                                  selected: ft.id === mapping.formField,
                                })) ?? []}></uui-select>
                        </td>
                        <td>
                          <uui-select
                            placeholder=${this.localize.term("hubspotFormWorkflow_SelectHubspotField")}
                            @change=${(e : UUISelectEvent) => this.#onHubspotSelectChange(e, idx)}
                            .options=${
                              this.hubspotFields?.map((ft) => ({
                                name: ft.label,
                                value: ft.name,
                                selected: ft.name === mapping.hubspotField,
                              })) ?? []}></uui-select>
                        </td>
                        <td>
                          <uui-button
                            label=${this.localize.term("formProviderWorkflows_delete")}
                            look="secondary"
                            color="default"
                            @click=${() => this.#onDeleteClick(idx)}>
                            <uui-icon name="delete"></uui-icon>
                          </uui-button>
                        </td>
                      </tr>
                      <tr>
                        <td class="description" colspan="3">
                          <b>Description: </b>${this.#getHubspotDescription(mapping.hubspotField)}
                        </td>
                      </tr>
                    `)}
                  </tbody>
                </table>
              ` 
              : html``}
          </div>
      `}
    `;
  }

  static styles = [
    css`
      .hubspot-wf-status{
        margin-top: 10px;
        padding: 10px;
        background-color: #202454;
        color: #ffffff;
      }

      .hubspot-wf-auth-link{
        cursor: pointer;
      }

      .hubspot-wf-button, .hubspot-wf-table { 
          margin-top: 10px; 
      }

      .hubspot-wf-table th{
        text-align: justify;
      }

      .hubspot-wf-table td.description{
        padding-bottom: 15px;
      }
  `];
}
  
  export default HubspotMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: HubspotMappingPropertyUiElement;
      }
  }
  