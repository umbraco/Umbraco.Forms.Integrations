function accountPickerController($scope, umbracoFormsIntegrationsCrmActiveCampaignResource) {

    var vm = this;

    umbracoFormsIntegrationsCrmActiveCampaignResource.getAccounts().then(function (response) {
        vm.accounts = response.accounts;

        vm.selectedAccount = $scope.setting.value;
    });

    vm.save = function () {
        $scope.setting.value = vm.selectedAccount.length > 0
            ? vm.selectedAccount : "";


    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Crm.ActiveCampaign.AccountPickerController", accountPickerController)