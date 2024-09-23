import { html, customElement, property, css, when, state, map } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { HUBSPOT_CONTEXT_TOKEN } from '@umbraco-integrations/hubspot/context';
import { Field, Property } from '@umbraco-integrations/hubspot/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { HubspotMappingValue } from '../models/hubspot.model';

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
  private selectedHubspotField: string = "";

  @state()
  private selectedFormField: string = "";

  @state()
  private hubspotFields: Array<Property> | undefined = [];

  @state()
  private formdFields: Array<Field> | undefined = [];

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

    if(!this.#settingsModel){
      const { data } = await this.#hubspotContext.getAuthenticationUrl();
      this.authenticationUrl = data;


    }else{
      this.authorizationStatus = this.#settingsModel!;
    }

    if(this.value){
      this.hubspotMappingArray = JSON.parse(this.value);
    }

    await this.#getHubspotFields();
    await this.#getFormFields();
  }

  async #openAuth(){
    window.open(this.authenticationUrl);
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

  #onDeleteClick(idx: number){
    
  }

  async #onConnect(){
    const { data } = await this.#hubspotContext.authorize(this.authorizationCode);
    if(data?.result.success){
      this.authorizationStatus = "OAuth";
      this.authorizationCode = "";
    }
  }

  isDisabled(){
    return false;
  }

  #addButtonClick(){
    this.hubspotMappingArray.push({
      formField: "",
      hubspotField: "",
      appendValue: false
    });
  }

  #onContactSelectChange(e: UUISelectEvent){
    this.selectedHubspotField = e.target.value.toString();
  }

  #onFormFieldSelectChange(e: UUISelectEvent){
    this.selectedFormField = e.target.value.toString();
  }

  render() {
    return html`
    ${this.authorizationStatus === "Unauthenticated" 
      ? html`
        <div>
          <p>Umbraco Forms is not configured with a HubSpot CRM account.</p>
          <p>To do this you can either create and save an API key or a Private Access Token into the <i>appsettings.json</i> file.</p>
          <p>Or you can click <a style="text-decoration: underline">here</a> to complete an OAuth connection.</p>
          <uui-button label="Open" @click=${this.#openAuth}></uui-button>
          <p><em>If your browser is unable to process the automated connection, paste the provided authorization code below and click to complete the authentication.</em></p>
          <uui-input placeholder="Enter authorization code" value=${this.authorizationCode}></uui-input>
          <uui-button type="button" @click=${this.#onConnect} label="Authorize"></uui-button>
        </div>
      ` 
      : html`
        <div class="umb-forms-settings-note ng-scope">
            Umbraco Forms is configured with a HubSpot CRM account using: <b>${this.authorizationStatus}</b></p>
        </div>

        <div class="activecampaign-wf-button">
          <uui-button look="primary" ?disabled=${this.isDisabled()} label="Add mapping" @click=${this.#addButtonClick}></uui-button>
        </div>

        <div>
          ${this.hubspotMappingArray.length > 0 
              ? html`
              <table>
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
                          placeholder=${this.localize.term("formProviderWorkflows_SelectContactField")}
                          @change=${(e : UUISelectEvent) => this.#onContactSelectChange(e)}
                          .options=${
                            this.hubspotFields?.map((ft) => ({
                              name: ft.label,
                              value: ft.name,
                              selected: ft.name === this.selectedHubspotField,
                            })) ?? []}></uui-select>
                      </td>
                      <td>
                        <uui-select
                            placeholder=${this.localize.term("formProviderWorkflows_SelectFormField")}
                            @change=${(e : UUISelectEvent) => this.#onFormFieldSelectChange(e)}
                            .options=${
                              this.formdFields?.map((ft) => ({
                                name: ft.caption,
                                value: ft.id,
                                selected: ft.id === this.selectedFormField,
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
                  `)}
                </tbody>
              </table>
              ` 
              : html``}
        </div>

        <div class="activecampaign-wf-button">
          <uui-button look="primary" ?disabled=${this.isDisabled()} label="De-authorize from Hubspot" @click=${this.#addButtonClick}></uui-button>
        </div>
      `}
    `;
  }
}
  
  export default HubspotMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: HubspotMappingPropertyUiElement;
      }
  }
  