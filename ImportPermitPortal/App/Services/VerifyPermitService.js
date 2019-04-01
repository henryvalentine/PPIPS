define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('verifyPermitService', ['ajaxServices', function (ajaxServices) {
        this.addPort = function (port, callbackFunction) {
            return ajaxServices.AjaxPost({ port: port }, "/Port/AddPort", callbackFunction);
        };

        this.getPort = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Port/GetPort?id=" + id, callbackFunction);
        };

        this.editPort = function (port, callbackFunction) {
            return ajaxServices.AjaxPost({ port: port }, "/Port/EditPort", callbackFunction);
        };

        this.deletePort = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Port/DeletePort?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Port/GetGenericList", callbackFunction);
        };
       

    }]);
});