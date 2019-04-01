define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('storageTankService', ['ajaxServices', function (ajaxServices) {
        this.addStorageTank = function (storageTank, callbackFunction) {
            return ajaxServices.AjaxPost({ storageTank: storageTank }, "/StorageTank/AddStorageTank", callbackFunction);
        };

        this.getStorageTank = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/StorageTank/GetStorageTank?id=" + id, callbackFunction);
        };

        this.editStorageTank = function (storageTank, callbackFunction) {
            return ajaxServices.AjaxPost({ storageTank: storageTank }, "/StorageTank/EditStorageTank", callbackFunction);
        };

        this.deleteStorageTank = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/StorageTank/DeleteStorageTank?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/StorageTank/GetGenericList", callbackFunction);
        };

    }]);
});