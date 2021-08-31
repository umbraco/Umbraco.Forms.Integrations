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

function HubSpotFieldsController($routeParams, umbracoFormsIntegrationsCrmHubspotResource, pickerResource) {
    var vm = this;

    vm.$onInit = function() {

        if (!vm.setting.value) {
            vm.mappings = [];
        } else {
            vm.mappings = JSON.parse(vm.setting.value);
        }

        var formId = $routeParams.id;
        if (formId !== -1){

            // Get the fields for the form.
            pickerResource.getAllFields(formId).then(function (response) {
                vm.fields = response.data;
            });

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
    }

    vm.getHubspotFieldDescription = function(value) {
        var item = vm.hubspotFields.find(x => {
            return x.value === value;
        });

        if (item) {
            return item.description;
        }
        
        return '';
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