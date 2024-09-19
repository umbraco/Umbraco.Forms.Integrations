import { html, customElement, property, state, map, css } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ACTIVECAMPAIGN_CONTEXT_TOKEN } from '@umbraco-integrations/activecampaign/context';
import { CustomFieldDto, Field } from '@umbraco-integrations/activecampaign/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { CustomMappingValue } from '../../models/activecampaign.model';

const elementName = "custom-mapping-property-editor";

@customElement(elementName)
export class CustomMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #activeCampaignContext!: typeof ACTIVECAMPAIGN_CONTEXT_TOKEN.TYPE;

  @property({ type: String })
  public value = "";

  @state()
  public customMappingArray : Array<CustomMappingValue> = [];

  @state()
  private selectedCustomField: string = "";

  @state()
  private selectedFormField: string = "";

  @state()
  private customFields: Array<CustomFieldDto> | undefined = [];

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

    if(this.value){
      this.customMappingArray = JSON.parse(this.value);
    }

    await this.#getCustomFields();
    await this.#getFormFields();
  }

  async #getCustomFields(){
    var result = await this.#activeCampaignContext.getCustomFields();
    if (!result) return;

    this.customFields = result.data?.fields;
  }

  async #getFormFields(){
    var formId = window.location.pathname.split("/")[7]; //Get the formid based on current url.
    var result = await this.#activeCampaignContext.getFormFields(formId);

    if (!result) return;

    this.formdFields = result.data;
  }

  #onCustomSelectChange(e: UUISelectEvent){
    this.selectedCustomField = e.target.value.toString();
  }

  #onFormFieldSelectChange(e: UUISelectEvent){
    this.selectedFormField = e.target.value.toString();
  }

  #addButtonClick(){
    this.customMappingArray.push({
      customField: {
        id: this.selectedCustomField,
        title: this.getSelectedCustomFieldCaption()!
      },
      formField: {
        id: this.selectedFormField,
        value: this.getSelectedFormFieldCaption()!
      }
    });

    this.value = JSON.stringify(this.customMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  getSelectedFormFieldCaption(){
    return this.formdFields?.find(f => f.id == this.selectedFormField)?.caption;
  }

  getSelectedCustomFieldCaption(){
    return this.customFields?.find(f => f.id == this.selectedCustomField)?.title;
  }

  isDisabled(){
    return !this.selectedCustomField || !this.selectedFormField;
  }

  #onDeleteClick(idx: number){
    this.customMappingArray.splice(idx, 1);

    this.value = JSON.stringify(this.customMappingArray);
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  render() {
    return html`
      <div>
        <uui-select
        placeholder=${this.localize.term("formProviderWorkflows_SelectCustomField")}
          @change=${(e : UUISelectEvent) => this.#onCustomSelectChange(e)}
          .options=${
            this.customFields?.map((ft) => ({
              name: ft.title,
              value: ft.id,
              selected: ft.id === this.selectedCustomField,
            })) ?? []}></uui-select>
        <uui-select
        placeholder=${this.localize.term("formProviderWorkflows_SelectFormField")}
          @change=${(e : UUISelectEvent) => this.#onFormFieldSelectChange(e)}
          .options=${
            this.formdFields?.map((ft) => ({
              name: ft.caption,
              value: ft.id,
              selected: ft.id === this.selectedFormField,
            })) ?? []}></uui-select>
      </div>

      <div class="activecampaign-wf-button">
        <uui-button look="primary" ?disabled=${this.isDisabled()} label=${this.localize.term("formProviderWorkflows_AddMapping")} @click=${this.#addButtonClick}></uui-button>
      </div>

      <div class="activecampaign-wf-table">
        ${this.customMappingArray.length > 0 
          ? html`
            <table>
              <thead>
                <tr>
                  <th>${this.localize.term("formProviderWorkflows_CustomField")}</th>
                  <th>${this.localize.term("formProviderWorkflows_FormField")}</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                ${map(this.customMappingArray, (mapping, idx) => html`
                  <tr>
                    <td>${mapping.customField?.title}</td>
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
  
export default CustomMappingPropertyUiElement;

declare global {
    interface HTMLElementTagNameMap {
        [elementName]: CustomMappingPropertyUiElement;
    }
}
  