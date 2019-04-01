define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('stepService', ['ajaxServices', function (ajaxServices) {
        this.addStep = function (step, callbackFunction) {
            return ajaxServices.AjaxPost({ step: step }, "/Step/AddStep", callbackFunction);
        };

        this.getStep = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Step/GetStep?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Step/GetGenericList", callbackFunction);
        };

        this.RefreshAppStage = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Step/RefreshImportStage", callbackFunction);
        };

        this.editStep = function (step, callbackFunction) {
            return ajaxServices.AjaxPost({ step: step }, "/Step/EditStep", callbackFunction);
        };

        this.deleteStep = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Step/DeleteStep?id=" + id, callbackFunction);
        };

        this.getProcessesByStage = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Step/GetProcessesByStage?id=" + id, callbackFunction);
        };
    }]);
});