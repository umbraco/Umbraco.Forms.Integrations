import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ACTIVECAMPAIGN_CONTEXT_TOKEN } from '@umbraco-integrations/activecampaign/context';
import { ContactFieldSettings } from '@umbraco-integrations/activecampaign/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

const elementName = "contact-mapping-property-editor";

@customElement(elementName)
export class ContactMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #activeCampaignContext!: typeof ACTIVECAMPAIGN_CONTEXT_TOKEN.TYPE;

  @property({ type: String })
  public value = "";

  @state()
  private contactFields: Array<ContactFieldSettings> | undefined = [];

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
  }

  async #getContactFields(){
    var result = await this.#activeCampaignContext.getContactFields();
    if (!result) return;

    this.contactFields = result.data;
  }

  #onSelectChange(e: UUISelectEvent){
    this.value = e.target.value.toString();
    this.requestUpdate();
    this.dispatchEvent(new CustomEvent('property-value-change'));
  }

  render() {
    return html`<uui-select
      @change=${(e : UUISelectEvent) => this.#onSelectChange(e)}
      .options=${
        this.contactFields?.map((ft) => ({
          name: ft.displayName,
          value: ft.name,
          selected: ft.name === this.value,
        })) ?? []}
    ></uui-select>`;
  }
}
  
  export default ContactMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: ContactMappingPropertyUiElement;
      }
  }
  