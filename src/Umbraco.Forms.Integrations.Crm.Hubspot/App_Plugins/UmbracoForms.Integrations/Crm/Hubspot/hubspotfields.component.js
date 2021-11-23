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

function HubSpotFieldsController($routeParams, umbracoFormsIntegrationsCrmHubspotResource, pickerResource, overlayService, notificationsService) {
    var vm = this;

    vm.authorizationCode = "";
    vm.authenticationUrl = "";
    vm.loading = true;
    vm.authorizationStatus = "Unauthenticated";

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
            vm.loading = false;
        });
    }

    vm.$onInit = function () {

        if (!vm.setting.value) {
            vm.mappings = [];
        } else {
            vm.mappings = JSON.parse(vm.setting.value);
        }

        umbracoFormsIntegrationsCrmHubspotResource.isAuthorizationConfigured().then(function (response) {
            if (response !== "Unauthenticated") {
                vm.authorizationStatus = response;
                getFieldsForMapping();
            }
            else {
                vm.authorizationStatus = "Unauthenticated";
                umbracoFormsIntegrationsCrmHubspotResource.getAuthenticationUrl().then(function (response) {
                    vm.authenticationUrl = response;
                    vm.loading = false;
                });
            }
        });
    };

    vm.openAuth = function () {
        window.open(vm.authenticationUrl);
    };

    // Setup the post message handler for automatic authentication without having to copy and paste the code from the proxy site.
    window.addEventListener("message", function (event) {
        if (event.data.type === "hubspot:oauth:success") {
            umbracoFormsIntegrationsCrmHubspotResource.authorize(event.data.code).then(function (response) {
                handleAuthorizationCallback(response);
            });
        }
    }, false);

    function handleAuthorizationCallback(response) {
        if (response.success) {
            vm.authorizationStatus = "OAuth";
            vm.authorizationCode = "";
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
    };

    vm.authorize = function () {
        umbracoFormsIntegrationsCrmHubspotResource.authorize(vm.authorizationCode).then(function (response) {
            handleAuthorizationCallback(response);
        });
    }

    vm.deauthorize = function () {

        var overlay = {
            view: "confirm",
            title: "Confirmation",
            content: "Are you sure you wish to disconnect your HubSpot account?",
            closeButtonLabel: "No",
            submitButtonLabel: "Yes",
            submitButtonStyle: "danger",
            close: function () {
                overlayService.close();
            },
            submit: function () {
                umbracoFormsIntegrationsCrmHubspotResource.deauthorize().then(function (response) {
                    if (response.success) {
                        vm.authorizationStatus = "Unauthenticated";
                        notificationsService.showNotification({
                            type: 0,
                            header: "De-authorization succeeded",
                            message: "Your Umbraco Forms installation is no longer connected to your HubSpot account",
                        });
                        getFieldsForMapping();
                    } else {
                        notificationsService.showNotification({
                            type: 2,
                            header: "De-authorization failed",
                            message: response.errorMessage
                        });
                    }
                    overlayService.close();
                });
            }
        };
        overlayService.open(overlay);
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