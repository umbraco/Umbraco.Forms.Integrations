import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifest: ManifestPropertyEditorUi = {
		type: 'propertyEditorUi',
		alias: 'ActiveCampaign.Contacts.PropertyEditorUi.Account',
		name: 'Account Property Editor',
		element: () => import('./account-property-editor.element'),
		meta: {
			label: 'Account',
			icon: 'icon-umb-contour',
			group: 'forms'
		},
	};
