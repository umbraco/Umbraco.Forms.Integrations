function mainController($scope, $routeParams, pickerResource, notificationsService) {

    var vm = this;

    var formId = $routeParams.id;

    if (formId !== -1) {
        getFormFields(formId);
    } else {
        notificationsService.warning("Cannot retrieve form details");
    }

    function getFormFields(formId) {
        pickerResource.getAllFields(formId).then(function(response) {
        });
    }

}

angular.module("umbraco")
    .controller("Umbraco.Forms.Integrations.Automation.Zapier.MainController", mainController)