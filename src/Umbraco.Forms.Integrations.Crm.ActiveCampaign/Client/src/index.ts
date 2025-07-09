import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { manifests as propertyEditorManifests } from "./property-editor/manifests";
import { client } from "@umbraco-integrations/activecampaign/generated";
import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { manifests as localizationManifests } from "./lang/manifests.js";
import { manifest as activecampaignContext } from "./context/manifest.js";

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
    extensionRegistry.registerMany([
        ...propertyEditorManifests,
        ...localizationManifests,
        activecampaignContext
  ]);

  host.consumeContext(UMB_AUTH_CONTEXT, async (auth) => {
      if (!auth) return;

      client.setConfig(umbHttpClient.getConfig());
  });
};