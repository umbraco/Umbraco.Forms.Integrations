import type { ManifestGlobalContext } from "@umbraco-cms/backoffice/extension-registry";

const contextManifest: ManifestGlobalContext = {
    type: "globalContext",
    alias: "activecampaign.context",
    name: "ActiveCampaign Context",
    js: () => import("./activecampaign.context.js")
};

export const manifest = contextManifest;