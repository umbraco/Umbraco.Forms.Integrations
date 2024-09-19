import { html, customElement, property, state, css, map, when } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ACTIVECAMPAIGN_CONTEXT_TOKEN } from '@umbraco-integrations/activecampaign/context';
import { ContactFieldSettings, Field } from '@umbraco-integrations/activecampaign/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { ContactMappingValue } from '../../models/activecampaign.model';

const elementName = "contact-mapping-property-editor";

@customElement(elementName)
export class ContactMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #activeCampaignContext!: typeof ACTIVECAMPAIGN_CONTEXT_TOKEN.TYPE;

  @property({ attribute: false })
  public contactMapping : Array<ContactMappingValue> = [];

  @state()
  private selectedContactField: string = "";

  @state()
  private selectedFormField: string = "";

  @state()
  private contactFields: Array<ContactFieldSettings> | undefined = [];

  @state()
  private formdFields: Array<Field> | undefined = [];

  constructor() {
    super();
    this.consumeContext(ACTIVECAMPAIGN_CONTEXT_TOKEN, (context) => {
        if (!context) return;
        this.#activeCampaignContext = context;
    });
  }

  async connectedCallback() {
    super.connectedCallback();

    await this.#getContactFields();
    await this.#getFormFields();
  }

  async #getContactFields(){
    var result = await this.#activeCampaignContext.getContactFields();
    if (!result) return;

    this.contactFields = result.data;
  }

  async #getFormFields(){
    var result = await this.#activeCampaignContext.getFormFields("f595361a-37f9-44da-80ca-6a22d699d923");

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
    this.contactMapping.push({
      contactField: this.selectedContactField,
      formField: {
        id: this.selectedFormField,
        value: this.getSelectedFieldCaption()!
      }
    });

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
    this.contactMapping.splice(idx, 1);

    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  render() {
    return html`
      <div>
        <uui-select
          placeholder="Select contact field"
          @change=${(e : UUISelectEvent) => this.#onContactSelectChange(e)}
          .options=${
            this.contactFields?.map((ft) => ({
              name: ft.displayName,
              value: ft.name,
              selected: ft.name === this.selectedContactField,
            })) ?? []}></uui-select>
        <uui-select
          placeholder="Select form field"
          @change=${(e : UUISelectEvent) => this.#onFormFieldSelectChange(e)}
          .options=${
            this.formdFields?.map((ft) => ({
              name: ft.caption,
              value: ft.id,
              selected: ft.id === this.selectedFormField,
            })) ?? []}></uui-select>
      </div>

      <div class="activecampaign-wf-required">
        Mandatory fields: ${this.contactFields?.filter(c => c.required).map(c => c.displayName).join(",")}
      </div>

      <div class="activecampaign-wf-button">
        <uui-button look="primary" ?disabled=${this.isDisabled()} label="Add mapping" @click=${this.#addButtonClick}></uui-button>
      </div>

      <div class="activecampaign-wf-table">
        <table>
          <thead>
            <tr>
              <th>Contact Field</th>
              <th>Form Field</th>
              <th></th>
            </tr>
        </thead>
        <tbody>
          ${map(this.contactMapping, (mapping, idx) => html`
            <tr>
              <td>${mapping.contactField}</td>
              <td>${mapping.formField?.value}</td>
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
      </div>
    `;
  }

  static styles = [
    css`
        .activecampaign-wf-required{
          margin-top: 10px;
          padding: 10px 15px;
          background-color: #3333ff;
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
  