angular.module("umbraco").controller("UmbracoFormsExtensions.Hubspot.Fields", function ($scope, $routeParams, pickerResource, hubspotResource) {

    function init() {
        if (!$scope.setting.value) {
            $scope.mappings = [];
        } else {
            $scope.mappings = JSON.parse($scope.setting.value);
        }

        var formId = $routeParams.id;

        if (formId === -1 && $scope.model && $scope.model.fields) {

        } else {

            // Available Form Fields
            pickerResource.getAllFields($routeParams.id).then(function (response) {
                $scope.fields = response.data;
            });

            // TODO: Remove hard coded API key & remember to revoke it later on
            // Need to get API key from other field
            // May need to do parent.parent scope traversal - YUK :S
            hubspotResource.getAllProperties('6a488b25-b7e7-489d-ad45-2da52a878ff9').then(function (response) {
                $scope.hubspotFields = response.map(x =>{
                    return {
                        value: x.name,
                        name: x.label,
                        description: x.description
                    }
                });
            });
        }
    }

    $scope.getHubspotFieldDescription = function(value) {
        var item = $scope.hubspotFields.find(x => {
            return x.value === value;
        });

        if(item){
            return item.description;
        }
        
        return '';
    }

    $scope.addMapping = function () {
        // Add new empty object into array
        $scope.mappings.push({
            formField: "",
            hubspotField: ""
        });
    };

    $scope.deleteMapping = function (index) {
        $scope.mappings.splice(index, 1);
        $scope.setting.value = JSON.stringify($scope.mappings);
    };

    $scope.stringifyValue = function () {
        $scope.setting.value = JSON.stringify($scope.mappings);
    };

    init();

});