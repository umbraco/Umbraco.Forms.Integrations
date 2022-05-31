function CustomerDetailsMapperController($scope, $routeParams, pickerResource, notificationsService) {

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

    vm.deleteMapping = function(index) {
        vm.mappings.splice(index);

        $scope.setting.value = JSON.stringify(vm.mappings);
    }

    function init() {
        vm.customerProperties = ["Email", "FirstName", "LastName", "Phone", "Address", "ZipCode", "City", "State", "Country"];

        vm.fields = [];
        vm.selectedCustomerProperty = "";
        vm.selectedField = "";

        var formId = $routeParams.id;

        if (formId !== -1) {
            pickerResource.getAllFields($routeParams.id).then(function (response) {
                vm.fields = response.data;
            });
        }

        vm.mappings = $scope.setting.value
            ? JSON.parse($scope.setting.value)
            : [];
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Commerce.eMerchantPay.CustomerDetailsMapperController", CustomerDetailsMapperController);