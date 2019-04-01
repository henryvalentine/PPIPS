define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('stepActivityTypeService', ['ajaxServices', function (ajaxServices) {
        this.addStepActivityType = function (stepActivityType, callbackFunction) {
            return ajaxServices.AjaxPost({ stepActivityType: stepActivityType }, "/StepActivityType/AddStepActivityType", callbackFunction);
        };

        this.getStepActivityType = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/StepActivityType/GetStepActivityType?id=" + id, callbackFunction);
        };

        this.editStepActivityType = function (stepActivityType, callbackFunction) {
            return ajaxServices.AjaxPost({ stepActivityType: stepActivityType }, "/StepActivityType/EditStepActivityType", callbackFunction);
        };

        this.deleteStepActivityType = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/StepActivityType/DeleteStepActivityType?id=" + id, callbackFunction);
        };



    }]);
});