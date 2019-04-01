define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('regionService', ['ajaxServices', function (ajaxServices) {
        this.addRegion = function (region, callbackFunction) {
            return ajaxServices.AjaxPost({ region: region }, "/Region/AddRegion", callbackFunction);
        };

        this.getRegion = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Region/GetRegion?id=" + id, callbackFunction);
        };

        this.editRegion = function (region, callbackFunction) {
            return ajaxServices.AjaxPost({ region: region }, "/Region/EditRegion", callbackFunction);
        };

        this.deleteRegion = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Region/DeleteRegion?id=" + id, callbackFunction);
        };



    }]);
});