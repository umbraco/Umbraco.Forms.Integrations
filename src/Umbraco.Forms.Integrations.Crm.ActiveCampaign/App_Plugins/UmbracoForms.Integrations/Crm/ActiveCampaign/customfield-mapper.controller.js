function customFieldMapperController($scope, notificationsService, activeCampaignService, umbracoFormsIntegrationsCrmActiveCampaignResource) {

    var vm = this;

    init();

    vm.addMapping = function () {

        if (vm.customFieldMappings.find(p => p.customField === vm.selectedMapping.customField)) {
            notificationsService.warning("Custom field is already mapped.");
            return;
        }

        vm.customFieldMappings.push({
            customField: vm.customFields.find(p => p.id === vm.selectedMapping.customField),
            formField: vm.formFields.find(p => p.id === vm.selectedMapping.formField)
        });

        $scope.setting.value = JSON.stringify(vm.customFieldMappings);

        vm.selectedMapping.clear();
    }

    vm.deleteMapping = function (index) {

        vm.customFieldMappings.splice(index, 1);

        $scope.setting.value = JSON.stringify(vm.customFieldMappings);
    }

    function init() {
        vm.selectedMapping = {
            customField: "",
            formField: "",
            clear: function () {
                this.customField = "";
                this.formField = "";
            }
        };

        vm.customFieldMappings = $scope.setting.value
            ? JSON.parse($scope.setting.value)
            : [];

        console.log(vm.customFieldMappings);

        activeCampaignService.getFormFields(function (response) {
            vm.formFields = response;
        });

        umbracoFormsIntegrationsCrmActiveCampaignResource.getCustomFields().then(function (response) {
            vm.customFields = response.fields;
        });
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Crm.ActiveCampaign.CustomFieldMapperController", customFieldMapperController);