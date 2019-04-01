define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('jettyMappingService', ['ajaxServices', function (ajaxServices) {
        this.addJettyMapping = function (jettyMapping, callbackFunction) {
            return ajaxServices.AjaxPost({ jettyMapping: jettyMapping }, "/JettyMapping/AddJettyMapping", callbackFunction);
        };

        this.getJettyMapping = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/JettyMapping/GetJettyMapping?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/JettyMapping/GetGenericList", callbackFunction);
        };

        this.editJettyMapping = function (jettyMapping, callbackFunction) {
            return ajaxServices.AjaxPost({ jettyMapping: jettyMapping }, "/JettyMapping/EditJettyMapping", callbackFunction);
        };

        this.deleteJettyMapping = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/JettyMapping/DeleteJettyMapping?id=" + id, callbackFunction);
        };


    }]);
});