define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('zoneService', ['ajaxServices', function (ajaxServices) {
        this.addZone = function (zone, callbackFunction) {
            return ajaxServices.AjaxPost({ zone: zone }, "/Zone/AddZone", callbackFunction);
        };

        this.getZone = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Zone/GetZone?id=" + id, callbackFunction);
        };

        this.editZone = function (zone, callbackFunction) {
            return ajaxServices.AjaxPost({ zone: zone }, "/Zone/EditZone", callbackFunction);
        };

        this.deleteZone = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Zone/DeleteZone?id=" + id, callbackFunction);
        };



    }]);
});