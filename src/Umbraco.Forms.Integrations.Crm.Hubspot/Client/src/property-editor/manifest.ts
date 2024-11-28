import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
		type: 'propertyEditorUi',
		alias: 'Hubspot.PropertyEditorUi.Mapping',
		name: 'Hubspot Mapping Property Editor',
		element: () => import('./hubspot-mapping.property-editor'),
		meta: {
			label: 'Custom Mapping',
			icon: 'icon-umb-contour',
			group: 'forms'
		},
	};