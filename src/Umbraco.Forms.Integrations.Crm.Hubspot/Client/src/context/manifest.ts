import type { ManifestGlobalContext } from "@umbraco-cms/backoffice/extension-registry";

const contextManifest: ManifestGlobalContext = {
    type: "globalContext",
    alias: "hubspot.context",
    name: "Hubspot Context",
    js: () => import("./hubspot.context.js")
};

export const manifest = contextManifest;