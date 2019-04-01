define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('depotOwnerService', ['ajaxServices', function (ajaxServices) {
        this.addDepotOwner = function (depotOwner, callbackFunction) {
            return ajaxServices.AjaxPost({ depotOwner: depotOwner }, "/DepotOwner/AddDepotOwner", callbackFunction);
        };

        this.getDepotOwner = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/DepotOwner/GetDepotOwner?id=" + id, callbackFunction);
        };

        this.editDepotOwner = function (depotOwner, callbackFunction) {
            return ajaxServices.AjaxPost({ depotOwner: depotOwner }, "/DepotOwner/EditDepotOwner", callbackFunction);
        };

        this.deleteDepotOwner = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/DepotOwner/DeleteDepotOwner?id=" + id, callbackFunction);
        };



    }]);
});