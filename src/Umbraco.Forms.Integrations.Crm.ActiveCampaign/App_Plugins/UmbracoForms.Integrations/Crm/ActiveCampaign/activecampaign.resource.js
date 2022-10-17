function activeCampaignResource($http, umbRequestHelper) {

    const apiEndpoint = "backoffice/UmbracoFormsIntegrationsCrmActiveCampaign/Contacts";

    return {
        checkApiAccess: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/CheckApiAccess`),
                "Failed to get resource");
        },
        getContactFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetContactFields`),
                "Failed to get resource");
        },
        getCurrencies: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetCurrencies`),
                "Failed to get resource");
        },
        getAvailableMappingFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetAvailableMappingFields`),
                "Failed to get resource");
        },
        getRequiredMappingFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetRequiredMappingFields`),
                "Failed to get resource");
        }
    };
}

angular.module('umbraco.resources').factory('umbracoFormsIntegrationsCrmActiveCampaignResource', activeCampaignResource);