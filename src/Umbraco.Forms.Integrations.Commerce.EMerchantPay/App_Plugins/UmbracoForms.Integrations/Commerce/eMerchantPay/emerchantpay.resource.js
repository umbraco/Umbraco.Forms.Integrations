function eMerchantPayResource($http, umbRequestHelper) {

    const apiEndpoint = "backoffice/UmbracoFormsIntegrationsCommerceEmerchantPay/EmerchantPay";

    return {
        getCurrencies: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetCurrencies`),
                "Failed to get resource");
        },
        getMappingFields: function () {
            return umbRequestHelper.resourcePromise(
                $http.get(`${apiEndpoint}/GetMappingFields`),
                "Failed to get resource");
        }
    };
}

angular.module('umbraco.resources').factory('umbracoFormsIntegrationsCommerceEMerchantPayResource', eMerchantPayResource);