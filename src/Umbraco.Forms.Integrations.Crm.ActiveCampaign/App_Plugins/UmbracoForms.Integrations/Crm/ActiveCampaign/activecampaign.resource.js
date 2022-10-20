function activeCampaignResource($http, umbRequestHelper) {

    const accountsApiEndpoint = "backoffice/UmbracoFormsIntegrationsCrmActiveCampaign/Accounts";
    const contactsApiEndpoint = "backoffice/UmbracoFormsIntegrationsCrmActiveCampaign/Contacts";

    return {
        checkApiAccess: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${contactsApiEndpoint}/CheckApiAccess`),
                "Failed to get resource");
        },
        getAccounts: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${accountsApiEndpoint}/GetAccounts`),
                "Failed to get resource");
        },
        getContactFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${contactsApiEndpoint}/GetContactFields`),
                "Failed to get resource");
        },
        getCustomFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${contactsApiEndpoint}/GetCustomFields`),
                "Failed to get resource");
        }
    };
}

angular.module('umbraco.resources').factory('umbracoFormsIntegrationsCrmActiveCampaignResource', activeCampaignResource);