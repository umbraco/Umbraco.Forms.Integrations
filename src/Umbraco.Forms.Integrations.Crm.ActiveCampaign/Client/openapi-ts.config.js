import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    logs: {
        level: 'debug',
    },
    input: 'http://localhost:62622/umbraco/swagger/activecampaign-management/swagger.json',
    output: {
        path: 'generated',
    },
    plugins: [
        {
            name: '@hey-api/client-fetch',
            bundle: false,
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
        },
    ],
});


