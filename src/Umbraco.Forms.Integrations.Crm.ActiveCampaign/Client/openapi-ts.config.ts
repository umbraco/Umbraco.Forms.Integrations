export default {
    input: 'http://localhost:35411/umbraco/swagger/active-campaign-management/swagger.json',
    output: {
      lint: 'eslint',
      path: 'generated',
    },
    schemas: false,
    services: {
      asClass: true
    },
    types: {
      enums: 'typescript',
    }
  }