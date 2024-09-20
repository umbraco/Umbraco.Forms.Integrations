import { html, customElement, property, css, when, state, map } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { HUBSPOT_CONTEXT_TOKEN } from '@umbraco-integrations/hubspot/context';
import { Property } from '@umbraco-integrations/hubspot/generated';

const elementName = "hubspot-mapping-property-editor";

@customElement(elementName)
export class HubspotMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #hubspotContext!: typeof HUBSPOT_CONTEXT_TOKEN.TYPE;

  @property({ type: String })
  public value = "";

  @state()
  public hubspotMappingArray : Array<string> = [];

  @state()
  private selectedContactField: string = "";

  @state()
  private selectedFormField: string = "";

  @state()
  private hubspotFields: Array<Property> | undefined = [];

  @state()
  private formdFields: Array<string> | undefined = [];

  constructor() {
    super();
    this.consumeContext(HUBSPOT_CONTEXT_TOKEN, (context) => {
        if (!context) return;
        this.#hubspotContext = context;
    });
  }

  async connectedCallback() {
    super.connectedCallback();

    if(this.value){
      this.hubspotMappingArray = JSON.parse(this.value);
    }

    await this.#getHubspotFields();
    //await this.#getFormFields();
  }

  async #getHubspotFields(){
    var result = await this.#hubspotContext.getAll();
    if (!result) return;

    this.hubspotFields = result.data;
  }

  // async #getFormFields(){
  //   var formId = window.location.pathname.split("/")[7]; //Get the formid based on current url.
  //   var result = await this.#activeCampaignContext.getFormFields(formId);

  //   if (!result) return;

  //   this.formdFields = result.data;
  // }

  #onDeleteClick(idx: number){
    
  }

  isDisabled(){
    return false;
  }

  #addButtonClick(){
    this.hubspotMappingArray.push("");
  }
  
  render() {
    return html`
      <div>Umbraco forms is configured.</div>

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
                      <uui-select></uui-select>
                    </td>
                    <td>
                      <uui-select></uui-select>
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
        <uui-button look="primary" ?disabled=${this.isDisabled()} label="Add mapping" @click=${this.#addButtonClick}></uui-button>
      </div>

      <div class="activecampaign-wf-button">
        <uui-button look="primary" ?disabled=${this.isDisabled()} label="De-authorize from Hubspot" @click=${this.#addButtonClick}></uui-button>
      </div>
    `;
  }
}
  
  export default HubspotMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: HubspotMappingPropertyUiElement;
      }
  }
  