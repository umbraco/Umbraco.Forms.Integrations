angular
    .module("umbraco")
    .component("umbFormsIntegrationsCrmHubspotFields", {
        controller: HubSpotFieldsController,
        controllerAs: "vm",
        templateUrl: "/App_Plugins/UmbracoForms.Integrations/Crm/Hubspot/hubspot-field-mapper-template.html",
        bindings: {
            setting: "<"
        },        
    }
);

function HubSpotFieldsController($routeParams, umbracoFormsIntegrationsCrmHubspotResource, pickerResource, notificationsService) {
    var vm = this;

    vm.authorizationCode = "";
    vm.authenticationUrl = "";
    vm.loading = false;
    vm.isAuthorizationConfigured = false;

    function getFieldsForMapping() {

        // Get the fields for the form.
        var formId = $routeParams.id;
        if (formId !== -1) {
            pickerResource.getAllFields(formId).then(function (response) {
                vm.fields = response.data;
            });
        }

        // Get the HubSpot contact fields.
        umbracoFormsIntegrationsCrmHubspotResource.getAllProperties().then(function (response) {
            vm.hubspotFields = response.map(x => {
                return {
                    value: x.name,
                    name: x.label,
                    description: x.description
                }
            });
        });
    }

    vm.$onInit = function() {

        if (!vm.setting.value) {
            vm.mappings = [];
        } else {
            vm.mappings = JSON.parse(vm.setting.value);
        }

        umbracoFormsIntegrationsCrmHubspotResource.isAuthorizationConfigured().then(function (response) {
            vm.loading = false;
            if (response) {
                console.log('ok');
                vm.isAuthorizationConfigured = true;
                getFieldsForMapping();
            }
            else {
                vm.isAuthorizationConfigured = false;
                umbracoFormsIntegrationsCrmHubspotResource.getAuthenticationUrl().then(function (response) {
                    vm.authenticationUrl = response;
                });
            }
        });
    }

    vm.authorize = function () {
        umbracoFormsIntegrationsCrmHubspotResource.authorize(vm.authorizationCode).then(function (response) {
            if (response.success) {
                vm.isAuthorizationConfigured = true;
                notificationsService.showNotification({
                    type: 0,
                    header: "Authorization succeeded",
                    message: "Your Umbraco Forms installation is now connected to your HubSpot account",
                });
                getFieldsForMapping();
            } else {
                notificationsService.showNotification({
                    type: 2,
                    header: "Authorization failed",
                    message: response.errorMessage
                });
            }
        });
    }

    vm.getHubspotFieldDescription = function (value) {
        if (!vm.hubspotFields) {
            return "";
        }
        var item = vm.hubspotFields.find(x => {
            return x.value === value;
        });

        if (item) {
            return item.description;
        }
        
        return "";
    }

    vm.addMapping = function () {

        // Add new empty object into the mappings array.
        vm.mappings.push({
            formField: "",
            hubspotField: ""
        });
    };

    vm.deleteMapping = function (index) {
        vm.mappings.splice(index, 1);
        vm.setting.value = JSON.stringify(vm.mappings);
    };

    vm.stringifyValue = function () {
        vm.setting.value = JSON.stringify(vm.mappings);
    };
}