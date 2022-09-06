function CustomerDetailsMapperController($scope, notificationsService, emerchantpayService, umbracoFormsIntegrationsCommerceEMerchantPayResource) {

    var vm = this;

    init();

    vm.addMapping = function () {

        if (vm.mappings.find(p => p.customerProperty === vm.selectedCustomerProperty)) {
            notificationsService.warning("Customer property is already mapped.");
            return;
        }

        vm.mappings.push({
            customerProperty: vm.selectedCustomerProperty,
            field: vm.fields.find(p => p.id === vm.selectedField)
        });

        $scope.setting.value = JSON.stringify(vm.mappings);

        vm.selectedCustomerProperty = "";
        vm.selectedField = "";
    }

    vm.deleteMapping = function (index) {

        vm.mappings.splice(index, 1);

        $scope.setting.value = JSON.stringify(vm.mappings);
    }


    function init() {

        umbracoFormsIntegrationsCommerceEMerchantPayResource.getMappingFields().then(function (response) {
            vm.customerProperties = response;
        });

        vm.fields = [];

        emerchantpayService.getFormFields(function (response) {
            vm.fields = response;
        });

        vm.selectedCustomerProperty = "";
        vm.selectedField = "";

        vm.mappings = $scope.setting.value
            ? JSON.parse($scope.setting.value)
            : [];
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Commerce.eMerchantPay.CustomerDetailsMapperController", CustomerDetailsMapperController);