function eMerchantPayResource($http, umbRequestHelper) {

    const apiEndpoint = "backoffice/UmbracoFormsIntegrationsCommerceEmerchantPay/EmerchantPay";

    return {
        getCurrencies: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetCurrencies`),
                "Failed to get resource");
        },
        getDefaultMappingFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetDefaultMappingFields`),
                "Failed to get resource");
        },
        getRequiredMappingFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetRequiredMappingFields`),
                "Failed to get resource");
        }
    };
}

angular.module('umbraco.resources').factory('umbracoFormsIntegrationsCommerceEMerchantPayResource', eMerchantPayResource);