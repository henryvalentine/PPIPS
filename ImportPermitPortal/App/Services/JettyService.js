define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('jettyService', ['ajaxServices', function (ajaxServices) {
        this.addJetty = function (jetty, callbackFunction) {
            return ajaxServices.AjaxPost({ jetty: jetty }, "/Jetty/AddJetty", callbackFunction);
        };

        this.getJetty = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Jetty/GetJetty?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Jetty/GetGenericList", callbackFunction);
        };

        this.editJetty = function (jetty, callbackFunction) {
            return ajaxServices.AjaxPost({ jetty: jetty }, "/Jetty/EditJetty", callbackFunction);
        };

        this.deleteJetty = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Jetty/DeleteJetty?id=" + id, callbackFunction);
        };


    }]);
});