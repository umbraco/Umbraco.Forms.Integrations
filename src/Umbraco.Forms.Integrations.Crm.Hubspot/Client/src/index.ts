import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { manifest as hubspotContext } from "./context/manifest.js";
import { manifest as hubspotPropertyEditor } from "./property-editor/manifest.js";
import { manifests as localizationManifests } from "./lang/manifests.js";

import { OpenAPI } from "@umbraco-integrations/hubspot/generated";

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
  extensionRegistry.registerMany([
    hubspotContext,
    hubspotPropertyEditor,
    ...localizationManifests
  ]);

  host.consumeContext(UMB_AUTH_CONTEXT, async (instance) => {
    const umbOpenApi = instance.getOpenApiConfiguration();
    OpenAPI.TOKEN = umbOpenApi.token;
    OpenAPI.BASE = umbOpenApi.base;
    OpenAPI.WITH_CREDENTIALS = true;
  });
};