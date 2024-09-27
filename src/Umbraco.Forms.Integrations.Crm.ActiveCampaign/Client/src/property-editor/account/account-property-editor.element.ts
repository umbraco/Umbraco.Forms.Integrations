import { html, customElement, property , state} from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ACTIVECAMPAIGN_CONTEXT_TOKEN } from '@umbraco-integrations/activecampaign/context';
import { AccountDto } from '@umbraco-integrations/activecampaign/generated';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

const elementName = "account-property-editor";

@customElement(elementName)
export class AccountPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  #activeCampaignContext!: typeof ACTIVECAMPAIGN_CONTEXT_TOKEN.TYPE;

  @property({ type: String })
  public value = "";

  @state()
  private accounts: Array<AccountDto> | undefined = [];
 
  constructor() {
    super();
    this.consumeContext(ACTIVECAMPAIGN_CONTEXT_TOKEN, (context) => {
        if (!context) return;
        this.#activeCampaignContext = context;
    });
  }

  async connectedCallback() {
    super.connectedCallback();

    await this.#getAccount();
}

  async #getAccount(){
    var result = await this.#activeCampaignContext.getAccount();
    if(!result) return;

    this.accounts = result.data?.accounts;
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
        this.accounts?.map((ft) => ({
          name: ft.name,
          value: ft.id,
          selected: ft.id === this.value,
        })) ?? []}
    ></uui-select>`;
  }
}
  
  export default AccountPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: AccountPropertyUiElement;
      }
  }
  