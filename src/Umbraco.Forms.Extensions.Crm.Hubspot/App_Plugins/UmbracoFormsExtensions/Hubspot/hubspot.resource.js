function hubspotResource($http, umbRequestHelper) {

    return {       
        fetch: function (id) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "packageInstallApiBaseUrl",
                        "Fetch",
                        [{ packageGuid: id }])),
                'Failed to download package with guid ' + id);
        },
        getAllProperties: function (apiKey) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "umbracoFormsExtensionsHubspotBaseUrl",
                        "GetAllProperties",
                        [{ apiKey: apiKey }])),
                'Failed to get Hubspot Properties');
        },
    };
}

angular.module('umbraco.resources').factory('hubspotResource', hubspotResource);