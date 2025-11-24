import type { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { manifest as hubspotContext } from "./context/manifest.js";
import { manifest as hubspotPropertyEditor } from "./property-editor/manifest.js";
import { manifests as localizationManifests } from "./lang/manifests.js";

import { client } from "@umbraco-integrations/hubspot/generated";

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
    extensionRegistry.registerMany([
        hubspotContext,
        hubspotPropertyEditor,
        ...localizationManifests
    ]);

    host.consumeContext(UMB_AUTH_CONTEXT, async (authContext) => {
        if (!authContext) return;
        const config = authContext.getOpenApiConfiguration();

        client.setConfig({
            baseUrl: config.base,
            auth: async () => await authContext.getLatestToken(),
            credentials: config.credentials,
        });
    });
};