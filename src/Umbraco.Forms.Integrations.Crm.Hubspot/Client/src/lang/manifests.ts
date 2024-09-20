import type { ManifestLocalization } from "@umbraco-cms/backoffice/extension-registry";

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
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.Cs_CZ",
    //   weight: -100,
    //   name: "Czech",
    //   meta: {
    //     culture: "cs-cz",
    //   },
    //   js: () => import("./cs-cz.js"),
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.Da_DK",
    //   weight: -100,
    //   name: "Danish",
    //   meta: {
    //     culture: "da-dk",
    //   },
    //   js: () => import("./da-dk.js"),
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.En_GB",
    //   weight: -100,
    //   name: "English (UK)",
    //   meta: {
    //     culture: "en-gb",
    //   },
    //   js: () => import("./en-gb.js"),
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.Es_ES",
    //   weight: -100,
    //   name: "Spanish",
    //   meta: {
    //     culture: "es-es",
    //   },
    //   js: () => import("./es-es.js"),
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.Fr_FR",
    //   weight: -100,
    //   name: "French",
    //   meta: {
    //     culture: "fr-fr",
    //   },
    //   js: () => import("./fr-fr.js"),
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.It_IT",
    //   weight: -100,
    //   name: "French",
    //   meta: {
    //     culture: "it-it",
    //   },
    //   js: () => import("./it-it.js"),
    // },
    // {
    //   type: "localization",
    //   alias: "Forms.Localization.Pl_PL",
    //   weight: -100,
    //   name: "Polish",
    //   meta: {
    //     culture: "pl-pl",
    //   },
    //   js: () => import("./pl-pl.js"),
    },
  ];
  export const manifests = [...localizationManifests];