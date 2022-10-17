function contactMapperController($scope, notificationsService, activeCampaignService, umbracoFormsIntegrationsCrmActiveCampaignResource) {

    var vm = this;

    init();

    vm.addMapping = function () {

        if (vm.contactMappings.find(p => p.contactField === vm.selectedMapping.contactField)) {
            notificationsService.warning("Contact field is already mapped.");
            return;
        }

        vm.contactMappings.push({
            contactField: vm.selectedMapping.contactField,
            formField: vm.formFields.find(p => p.id === vm.selectedMapping.formField)
        });

        $scope.setting.value = JSON.stringify(vm.contactMappings);

        vm.selectedMapping.clear();
    }

    vm.deleteMapping = function (index) {

        vm.contactMappings.splice(index, 1);

        $scope.setting.value = JSON.stringify(vm.mappings);
    }


    function init() {

        vm.selectedMapping = {
            customerField: "",
            formField: "",
            clear: function () {
                this.customerField = "";
                this.formField = "";
            }
        };

        vm.contactMappings = $scope.setting.value
            ? JSON.parse($scope.setting.value)
            : [];

        umbracoFormsIntegrationsCrmActiveCampaignResource.checkApiAccess().then(function (response) {
            if (!response.isApiConfigurationValid)
                notificationsService.error("ActiveCampaign API", "Invalid API Configuration");
        });

        activeCampaignService.getFormFields(function (response) {
            vm.formFields = response;
        });

        umbracoFormsIntegrationsCrmActiveCampaignResource.getContactFields().then(function (response) {
            vm.contactFields = response;

            vm.requiredContactFields = vm.contactFields.filter(x => x.required).map(x => x.displayName).join(',');
        });
    }
}

angular.module("umbraco")
    .controller("UmbracoForms.Integrations.Crm.ActiveCampaign.ContactMapperController", contactMapperController);