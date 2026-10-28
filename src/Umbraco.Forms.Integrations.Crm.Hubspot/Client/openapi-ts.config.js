import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    logs: {
        level: 'debug',
    },
    input: 'http://localhost:45490/umbraco/openapi/hubspot-management.json',
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


