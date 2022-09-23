function FieldPickerController($scope, emerchantpayService) {

    var vm = this;

    init();

    vm.saveField = function () {
        $scope.setting.value = vm.selectedField;
    }

    function init() {

        vm.fields = [];

        emerchantpayService.getFormFields(function (response) {
            vm.fields = response;
        });

        if ($scope.setting && $scope.setting.value) vm.selectedField = $scope.setting.value;
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Commerce.Emerchantpay.FieldPickerController", FieldPickerController);