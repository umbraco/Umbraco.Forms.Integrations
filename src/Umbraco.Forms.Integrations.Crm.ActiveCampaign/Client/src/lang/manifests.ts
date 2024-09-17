import type { ManifestLocalization } from "@umbraco-cms/backoffice/extension-registry";

const localizationManifests: Array<ManifestLocalization> = [
    {
      type: "localization",
      alias: "Forms.Integrations.Crm.ActiveCampaign.Localization.En",
      weight: -100,
      name: "English (US)",
      meta: {
        culture: "en",
      },
      js: () => import("./en.js")
    }
  ];
  export const manifests = [...localizationManifests];