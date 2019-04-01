define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('vesselService', ['ajaxServices', function (ajaxServices) {

        this.addVessel = function (model, callbackFunction)
        {
            return ajaxServices.AjaxPost({ vessel: model }, "/Vessel/AddVessel", callbackFunction);
        };
        
        this.getVessel = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Vessel/GetVessel?id=" + id, callbackFunction);
        };
        
        this.editVessel = function (model, callbackFunction)
        {
            return ajaxServices.AjaxPost({ vessel: model }, "/Vessel/EditVessel", callbackFunction);
        };

        this.deleteVessel = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Vessel/DeleteVessel?id=" + id, callbackFunction);
        };

    }]);
});