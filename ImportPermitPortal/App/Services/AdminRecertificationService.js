define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('adminRecertificationService', ['ajaxServices', function (ajaxServices) {
        this.addRecertification = function (recertification, callbackFunction) {
            return ajaxServices.AjaxPost({ recertification: recertification }, "/Recertification/AddRecertification", callbackFunction);
        };

        this.getRecertification = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Admin/GetRecertification?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Recertification/GetGenericList", callbackFunction);
        };

        this.RefreshAppStage = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Recertification/RefreshImportStage", callbackFunction);
        };

        this.editRecertification = function (recertification, callbackFunction) {
            return ajaxServices.AjaxPost({ recertification: recertification }, "/Recertification/EditRecertification", callbackFunction);
        };

        this.deleteRecertification = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Recertification/DeleteRecertification?id=" + id, callbackFunction);
        };

        this.printRecertification = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Admin/PrintRecertification?id=" + id, callbackFunction);
        };
    }]);
});