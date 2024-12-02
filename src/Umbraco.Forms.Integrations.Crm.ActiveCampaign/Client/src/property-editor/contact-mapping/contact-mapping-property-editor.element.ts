import { html, customElement, property, state, css, map } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ACTIVECAMPAIGN_CONTEXT_TOKEN } from '@umbraco-integrations/activecampaign/context';
import { ContactFieldSettings, ActiveCampaignFormFieldDto } from '@umbraco-integrations/activecampaign/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { ContactMappingValue } from '../../models/activecampaign.model';

const elementName = "contact-mapping-property-editor";

@customElement(elementName)
export class ContactMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #activeCampaignContext!: typeof ACTIVECAMPAIGN_CONTEXT_TOKEN.TYPE;

  @property({ type: String })
  public value = "";

  @state()
  public contactMappingArray : Array<ContactMappingValue> = [];

  @state()
  private selectedContactField: string = "";

  @state()
  private selectedFormField: string = "";

  @state()
  private contactFields: Array<ContactFieldSettings> | undefined = [];

  @state()
  private formdFields: Array<ActiveCampaignFormFieldDto> | undefined = [];

  constructor() {
    super();
    this.consumeContext(ACTIVECAMPAIGN_CONTEXT_TOKEN, (context) => {
        if (!context) return;
        this.#activeCampaignContext = context;
    });
  }

  async connectedCallback() {
    super.connectedCallback();

    if(this.value){
      this.contactMappingArray = JSON.parse(this.value);
    }

    await this.#getContactFields();
    await this.#getFormFields();
  }

  async #getContactFields(){
    var result = await this.#activeCampaignContext.getContactFields();
    if (!result) return;

    this.contactFields = result.data;
  }

  async #getFormFields(){
    var formId = window.location.pathname.split("/")[7]; //Get the formid based on current url.
    var result = await this.#activeCampaignContext.getFormFields(formId);

    if (!result) return;

    this.formdFields = result.data;
  }

  #onContactSelectChange(e: UUISelectEvent){
    this.selectedContactField = e.target.value.toString();
  }

  #onFormFieldSelectChange(e: UUISelectEvent){
    this.selectedFormField = e.target.value.toString();
  }

  #addButtonClick(){
    this.contactMappingArray.push({
      contactField: this.selectedContactField,
      formField: {
        id: this.selectedFormField,
        value: this.getSelectedFieldCaption()!
      }
    });

    this.value = JSON.stringify(this.contactMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  getSelectedFieldCaption(){
    return this.formdFields?.find(f => f.id == this.selectedFormField)?.caption;
  }

  isDisabled(){
    return !this.selectedContactField || !this.selectedFormField;
  }

  #onDeleteClick(idx: number){
    this.contactMappingArray.splice(idx, 1);

    this.value = JSON.stringify(this.contactMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  render() {
    return html`
      <div>
        <uui-select
          placeholder=${this.localize.term("activeCampaignFormWorkflows_SelectContactField")}
          @change=${(e : UUISelectEvent) => this.#onContactSelectChange(e)}
          .options=${
            this.contactFields?.map((ft) => ({
              name: ft.displayName,
              value: ft.name,
              selected: ft.name === this.selectedContactField,
            })) ?? []}></uui-select>
        <uui-select
          placeholder=${this.localize.term("activeCampaignFormWorkflows_SelectFormField")}
          @change=${(e : UUISelectEvent) => this.#onFormFieldSelectChange(e)}
          .options=${
            this.formdFields?.map((ft) => ({
              name: ft.caption,
              value: ft.id,
              selected: ft.id === this.selectedFormField,
            })) ?? []}></uui-select>
      </div>

      <div class="activecampaign-wf-required">
        Mandatory fields: ${this.contactFields?.filter(c => c.required).map(c => c.displayName).join(", ")}
      </div>

      <div class="activecampaign-wf-button">
        <uui-button look="primary" ?disabled=${this.isDisabled()} label=${this.localize.term("activeCampaignFormWorkflows_AddMapping")} @click=${this.#addButtonClick}></uui-button>
      </div>

      <div class="activecampaign-wf-table">
        ${this.contactMappingArray.length > 0 
          ? html`
          <table>
            <thead>
              <tr>
                <th>${this.localize.term("activeCampaignFormWorkflows_ContactField")}</th>
                <th>${this.localize.term("activeCampaignFormWorkflows_FormField")}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              ${map(this.contactMappingArray, (mapping, idx) => html`
                <tr>
                  <td>${mapping.contactField}</td>
                  <td>${mapping.formField?.value}</td>
                  <td>
                    <uui-button
                      label=${this.localize.term("activeCampaignFormWorkflows_delete")}
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
    `;
  }

  static styles = [
    css`
        .activecampaign-wf-required{
          margin-top: 10px;
          padding: 10px 15px;
          background-color: #202454;
          color: #ffffff;
        }

        .activecampaign-wf-button, .activecampaign-wf-table { 
            margin-top: 10px; 
        }

        .activecampaign-wf-table th{
          padding-right: 20px;
        }
    `];
}
  
  export default ContactMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: ContactMappingPropertyUiElement;
      }
  }
  