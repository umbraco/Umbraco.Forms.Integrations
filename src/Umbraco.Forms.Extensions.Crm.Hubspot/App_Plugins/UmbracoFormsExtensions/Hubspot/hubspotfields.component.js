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

            // TODO: Remove hard coded API key & remember to revoke it later on
            // Need to get API key from other field
            // May need to do parent.parent scope traversal - YUK :S

            // Niels recommendation that this is a component & be able to get the data from a parent item
            // Using require (but finding which one it is I have no idea)

            hubspotResource.getAllProperties('6a488b25-b7e7-489d-ad45-2da52a878ff9').then(function (response) {
                vm.hubspotFields = response.map(x =>{
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

        if(item){
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