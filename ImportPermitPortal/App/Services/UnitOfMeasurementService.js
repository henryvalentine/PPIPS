define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('unitOfMeasurementService', ['ajaxServices', function (ajaxServices) {
        this.addUnitOfMeasurement = function (unitOfMeasurement, callbackFunction) {
            return ajaxServices.AjaxPost({ unitOfMeasurement: unitOfMeasurement }, "/UnitOfMeasurement/AddUnitOfMeasurement", callbackFunction);
        };

        this.getUnitOfMeasurement = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/UnitOfMeasurement/GetUnitOfMeasurement?id=" + id, callbackFunction);
        };

        this.editUnitOfMeasurement = function (unitOfMeasurement, callbackFunction) {
            return ajaxServices.AjaxPost({ unitOfMeasurement: unitOfMeasurement }, "/UnitOfMeasurement/EditUnitOfMeasurement", callbackFunction);
        };

        this.deleteUnitOfMeasurement = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/UnitOfMeasurement/DeleteUnitOfMeasurement?id=" + id, callbackFunction);
        };



    }]);
});