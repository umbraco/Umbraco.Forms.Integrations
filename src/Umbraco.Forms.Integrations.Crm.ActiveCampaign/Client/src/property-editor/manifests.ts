import { manifest as accountManifest } from "./account/manifests";
import { manifest as contactMappingManifest } from "./contact-mapping/manifests";
import { manifest as customMappingManifest } from "./custom-mapping/manifests";

export const manifests: Array<UmbExtensionManifest> = [
    accountManifest,
    contactMappingManifest,
    customMappingManifest
];