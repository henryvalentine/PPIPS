define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('processService', ['ajaxServices', function (ajaxServices) {
        this.addProcess = function (process, callbackFunction) {
            return ajaxServices.AjaxPost({ process: process }, "/Process/AddProcess", callbackFunction);
        };

        this.getProcess = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Process/GetProcess?Id=" + id, callbackFunction);
        };

        this.editProcess = function (process, callbackFunction) {
            return ajaxServices.AjaxPost({ process: process }, "/Process/EditProcess", callbackFunction);
        };

        this.deleteProcess = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Process/DeleteProcess?Id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Process/GetGenericList", callbackFunction);
        };

    }]);
});