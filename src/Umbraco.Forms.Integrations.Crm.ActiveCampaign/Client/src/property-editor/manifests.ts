import { ManifestTypes, type ManifestPropertyEditorUi } from "@umbraco-cms/backoffice/extension-registry";
import { manifest as accountManifest } from "./account/manifests";
import { manifest as contactMappingManifest } from "./contact-mapping/manifests";
import { manifest as customMappingManifest } from "./custom-mapping/manifests";

export const manifests : Array<ManifestTypes> = [
    accountManifest,
    contactMappingManifest,
    customMappingManifest
];