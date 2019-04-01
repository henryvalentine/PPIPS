define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('permitService', ['ajaxServices', function (ajaxServices) {
        this.addPermit = function (permit, callbackFunction) {
            return ajaxServices.AjaxPost({ permit: permit }, "/Permit/AddPermit", callbackFunction);
        };

        this.getPermit = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Permit/GetPermit?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Permit/GetGenericList", callbackFunction);
        };

        this.editPermit = function (permit, callbackFunction) {
            return ajaxServices.AjaxPost({ permit: permit }, "/Permit/EditPermit", callbackFunction);
        };

        this.deletePermit = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Permit/DeletePermit?id=" + id, callbackFunction);
        };

        this.getPermitInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Permit/GetPermitInfo?id=" + id, callbackFunction);
        };

        this.generatePermit = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/GeneratePermit?permitId=" + id, callbackFunction);
        };
    }]);
});