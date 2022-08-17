function emerchantpayService($routeParams, pickerResource) {
    return {
        getFormFields: function (callback) {
            var formId = $routeParams.id;

            if (formId !== -1) {
                pickerResource.getAllFields(formId).then(function (response) {
                    callback(response.data);
                });
            } else callback([]);
        }
    };
}

angular.module("umbraco.services")
    .factory("emerchantpayService", emerchantpayService)