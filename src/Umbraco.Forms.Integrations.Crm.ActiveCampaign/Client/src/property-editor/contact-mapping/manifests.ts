import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
		type: 'propertyEditorUi',
		alias: 'ActiveCampaign.Contacts.PropertyEditorUi.ContactMapping',
		name: 'Contact Mapping Property Editor',
		element: () => import('./contact-mapping-property-editor.element'),
		meta: {
			label: 'Contact Mapping',
			icon: 'icon-umb-contour',
			group: 'forms'
		},
	};
