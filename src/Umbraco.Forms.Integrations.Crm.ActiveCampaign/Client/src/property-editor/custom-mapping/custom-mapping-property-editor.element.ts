import { html, customElement, property, css, when } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = "custom-mapping-property-editor";

@customElement(elementName)
export class CustomMappingPropertyUiElement extends UmbLitElement implements UmbPropertyEditorUiElement {
    render() {
      return html`<div><span>ABCD</span></div>`;
    }
  }
  
  export default CustomMappingPropertyUiElement;
  
  declare global {
      interface HTMLElementTagNameMap {
          [elementName]: CustomMappingPropertyUiElement;
      }
  }
  