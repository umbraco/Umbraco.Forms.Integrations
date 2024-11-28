import type { ManifestLocalization } from "@umbraco-cms/backoffice/localization";

const localizationManifests: Array<ManifestLocalization> = [
    {
      type: "localization",
      alias: "Hubspot.Localization.En",
      weight: -100,
      name: "English (US)",
      meta: {
        culture: "en",
      },
      js: () => import("./en.js"),
    },
  ];
  export const manifests = [...localizationManifests];