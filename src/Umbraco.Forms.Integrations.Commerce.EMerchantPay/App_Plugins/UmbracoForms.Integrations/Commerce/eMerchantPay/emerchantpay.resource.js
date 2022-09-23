function emerchantpayResource($http, umbRequestHelper) {

    const apiEndpoint = "backoffice/UmbracoFormsIntegrationsCommerceEmerchantpay/Emerchantpay";

    return {
        isAccountAvailable: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/IsAccountAvailable`),
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

angular.module('umbraco.resources').factory('umbracoFormsIntegrationsCommerceEmerchantpayResource', emerchantpayResource);