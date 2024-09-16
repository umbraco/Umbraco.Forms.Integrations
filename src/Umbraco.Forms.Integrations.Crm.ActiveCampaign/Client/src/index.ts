import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { manifests as activeCampaignPropertyEditor } from "./property-editor/manifests";
import { OpenAPI } from "@umbraco-integrations/activecampaign/generated";
import { manifests as localizationManifests } from "./lang/manifests.js";
import { manifest as activecampaignContext } from "./context/manifest.js";

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
    extensionRegistry.registerMany([
        ...activeCampaignPropertyEditor,
        ...localizationManifests,
        activecampaignContext
  ]);

  host.consumeContext(UMB_AUTH_CONTEXT, async (instance) => {
    const umbOpenApi = instance.getOpenApiConfiguration();
    OpenAPI.TOKEN = umbOpenApi.token;
    OpenAPI.BASE = umbOpenApi.base;
    OpenAPI.WITH_CREDENTIALS = true;
  });
};