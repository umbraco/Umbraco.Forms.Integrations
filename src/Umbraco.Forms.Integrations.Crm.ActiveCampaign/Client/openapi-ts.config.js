import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    debug: true,
    input: 'http://localhost:45490/umbraco/openapi/activecampaign-management.json',
    output: {
        path: 'generated',
    },
    plugins: [
        {
            name: '@hey-api/client-fetch',
            exportFromIndex: true,
            throwOnError: true,
        },
        {
            name: '@hey-api/typescript',
            enums: 'typescript',
        },
        {
            name: '@hey-api/sdk',
            asClass: true,
            classNameBuilder: (name) => `${name}Service`,
            responseStyle: 'fields',
        },
    ],
});


