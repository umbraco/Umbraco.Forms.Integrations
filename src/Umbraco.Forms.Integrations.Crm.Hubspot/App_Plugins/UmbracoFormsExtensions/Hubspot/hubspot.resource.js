function hubspotResource($http, umbRequestHelper) {

    return {       
        getAllProperties: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    umbRequestHelper.getApiUrl(
                        "umbracoFormsExtensionsHubspotBaseUrl",
                        "GetAllProperties")),
                'Failed to get Hubspot Properties');
        },
    };
}

angular.module('umbraco.resources').factory('hubspotResource', hubspotResource);