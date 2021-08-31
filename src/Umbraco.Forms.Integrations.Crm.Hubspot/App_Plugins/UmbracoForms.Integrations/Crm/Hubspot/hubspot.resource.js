function hubspotResource($http, umbRequestHelper) {

    return {       
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