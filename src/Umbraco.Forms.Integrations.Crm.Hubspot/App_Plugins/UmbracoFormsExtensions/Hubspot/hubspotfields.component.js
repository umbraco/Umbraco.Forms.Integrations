angular
    .module("umbraco")
    .component("umbFormsHubspotFields", {
        controller: HubSpotFieldsController,
        controllerAs: "vm",
        templateUrl: "/App_Plugins/UmbracoFormsExtensions/hubspot/hubspot-field-mapper-template.html",
        bindings: {
            setting: "<"
        },
        
    }
);

function HubSpotFieldsController($scope, $compile, $element, $routeParams, hubspotResource, pickerResource) {
    var vm = this;

    // its repating with umb-control-group for each workflow setting
    // parent view/controller that has the model.workflow.settings...
    // umbracoForms.Overlays.WorkflowSettingsOverlayController as vm

    vm.$onInit = function() {
        console.log('$scope', $scope);
        console.log('vm', vm);

        //console.log('vm.parentDirective', vm.parentDirective);

        if (!vm.setting.value) {
            vm.mappings = [];
        } else {
            vm.mappings = JSON.parse(vm.setting.value);
        }

        var formId = $routeParams.id;
        if (formId !== -1){

            // Available Form Fields
            pickerResource.getAllFields(formId).then(function (response) {
                vm.fields = response.data;
            });

            hubspotResource.getAllProperties().then(function (response) {
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
        // Add new empty object into array
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