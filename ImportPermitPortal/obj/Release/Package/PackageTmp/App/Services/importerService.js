define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('importerService', ['ajaxServices', function (ajaxServices) {
       
        this.getImporterDetails = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Importer/GetImporter?id=" + id, callbackFunction);
        };

        this.processGeneralInformation = function (gInformation, callbackFunction) {
            return ajaxServices.AjaxPost({ model: gInformation }, "/GeneralInformation/ProcessGeneralInformation", callbackFunction);
        };

        this.getGeneralInformation = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/GeneralInformation/GetImporterInformation?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/GeneralInformation/GetList", callbackFunction);
        };

        this.deleteAddress = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/GeneralInformation/DeleteAddress?id=" + id, callbackFunction);
        };
    }]);
});