import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
		type: 'propertyEditorUi',
		alias: 'ActiveCampaign.Contacts.PropertyEditorUi.CustomMapping',
		name: 'Custom Mapping Property Editor',
		element: () => import('./custom-mapping-property-editor.element'),
		meta: {
			label: 'Custom Mapping',
			icon: 'icon-umb-contour',
			group: 'forms'
		},
	};
