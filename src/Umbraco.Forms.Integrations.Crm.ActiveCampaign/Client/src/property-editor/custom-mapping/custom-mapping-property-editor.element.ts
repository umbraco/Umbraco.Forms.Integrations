import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ACTIVECAMPAIGN_CONTEXT_TOKEN } from '@umbraco-integrations/activecampaign/context';
import { CustomFieldDto } from '@umbraco-integrations/activecampaign/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

const elementName = "custom-mapping-property-editor";

@customElement(elementName)
export class CustomMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #activeCampaignContext!: typeof ACTIVECAMPAIGN_CONTEXT_TOKEN.TYPE;

  @property({ type: String })
  public value = "";
  @property({ type: String })
  public mappingValue = "";

  @state()
  private customFields: Array<CustomFieldDto> | undefined = [];

  constructor() {
    super();
    this.consumeContext(ACTIVECAMPAIGN_CONTEXT_TOKEN, (context) => {
        if (!context) return;
        this.#activeCampaignContext = context;
    });
  }

  async connectedCallback() {
    super.connectedCallback();

    await this.#getCustomFields();
}

  async #getCustomFields(){
    var result = await this.#activeCampaignContext.getCustomFields();
    if(!result) return;

    this.customFields = result.data?.fields;
    debugger;
  }

  #onSelectChange(e: UUISelectEvent){
    this.value = e.target.value.toString();
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  addButtonClick(){

  }

  render() {
    return html`
      <div>
        <uui-select
          @change=${(e : UUISelectEvent) => this.#onSelectChange(e)}
          .options=${
            this.customFields?.map((ft) => ({
              name: ft.title,
              value: ft.id,
              selected: ft.id === this.value,
            })) ?? []}
        ></uui-select>
        <uui-select
          @change=${(e : UUISelectEvent) => this.#onSelectChange(e)}
          .options=${
            this.customFields?.map((ft) => ({
              name: ft.title,
              value: ft.id,
              selected: ft.id === this.value,
            })) ?? []}
        ></uui-select>
      </div>

      <div>
        <uui-button label="Add mapping" click=${this.addButtonClick()}></uui-button>
      </div>

      <div>
        <table>
          <thead>
            <tr>
              <th>Contact Field</th>
              <th>Form Field</th>
              <th></th>
            </tr>
        </thead>
        <tbody>
        </tbody>
        </table>
      </div>
    `;
  }
  }
  
  export default CustomMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: CustomMappingPropertyUiElement;
      }
  }
  