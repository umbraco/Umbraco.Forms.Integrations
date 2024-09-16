import { html, customElement, property, css, when } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = "account-property-editor";

@customElement(elementName)
export class AccountPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
    render() {
      return html`<div><span>ABCD</span></div>`;
    }
  }
  
  export default AccountPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: AccountPropertyUiElement;
      }
  }
  