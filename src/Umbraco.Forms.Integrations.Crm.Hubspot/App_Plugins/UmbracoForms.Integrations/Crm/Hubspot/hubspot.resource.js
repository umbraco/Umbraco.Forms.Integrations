function hubspotResource($http, umbRequestHelper) {

    return {   
        isAuthorizationConfigured: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "umbracoFormsIntegrationsCrmHubspotBaseUrl",
                        "IsAuthorizationConfigured")),
                'Failed to get Hubspot authentication status');
        },
        getAuthenticationUrl: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "umbracoFormsIntegrationsCrmHubspotBaseUrl",
                        "GetAuthenticationUrl")),
                'Failed to get Hubspot authentication URL');
        },
        authorize: function (authorizationCode) {
            return umbRequestHelper.resourcePromise(
                $http.post(
                    umbRequestHelper.getApiUrl(
                        "umbracoFormsIntegrationsCrmHubspotBaseUrl",
                        "Authorize"), { code: authorizationCode }),
                'Failed to authentication with HubSpot');
        }, 
        getAllProperties: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "umbracoFormsIntegrationsCrmHubspotBaseUrl",
                        "GetAllProperties")),
                'Failed to get Hubspot Properties');
        },
    };
}

angular.module('umbraco.resources').factory('umbracoFormsIntegrationsCrmHubspotResource', hubspotResource);