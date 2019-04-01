define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('depotService', ['ajaxServices', function (ajaxServices) {

        this.addDepot = function (depot, callbackFunction)
        {
            return ajaxServices.AjaxPost({ model: depot }, "/Account/RegisterDepotInformation", callbackFunction);
        };
        
        this.getDepot = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Depot/GetDepot?id=" + id, callbackFunction);
        };

        this.getJetties = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Depot/GetJetties", callbackFunction);
        };

        this.RefreshAppStage = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Depot/RefreshImportStage", callbackFunction);
        };

        this.editDepot = function (depot, callbackFunction) {
            return ajaxServices.AjaxPost({ model: depot }, "/Account/UpdateDepotAccount", callbackFunction);
        };

        this.deleteDepot = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Depot/DeleteDepot?id=" + id, callbackFunction);
        };

    }]);
});