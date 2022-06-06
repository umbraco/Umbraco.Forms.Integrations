function CurrencyController($scope, umbracoFormsIntegrationsCommerceEMerchantPayResource) {

    var vm = this;

    umbracoFormsIntegrationsCommerceEMerchantPayResource.getCurrencies().then(function (response) {
        vm.currencies = response;
    });

    vm.saveCurrency = function () {
        $scope.setting.value = vm.selectedCurrency;
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Commerce.eMerchantPay.CurrencyController", CurrencyController);