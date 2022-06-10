function CurrencyController($scope, umbracoFormsIntegrationsCommerceEMerchantPayResource) {

    var vm = this;

    if ($scope.setting && $scope.setting.value) vm.selectedCurrency = $scope.setting.value;

    umbracoFormsIntegrationsCommerceEMerchantPayResource.getCurrencies().then(function (response) {
        vm.currencies = response;
    });

    vm.saveCurrency = function () {
        $scope.setting.value = vm.selectedCurrency;
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Commerce.eMerchantPay.CurrencyController", CurrencyController);