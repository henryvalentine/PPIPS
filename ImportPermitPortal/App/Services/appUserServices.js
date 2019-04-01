define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('appUserService', ['ajaxServices', function (ajaxServices) {
        this.addAppUser = function (employeeDesk, callbackFunction) {
            return ajaxServices.AjaxPost({ model: employeeDesk }, "/Account/RegisterUser", callbackFunction);
        };

        this.getAppUser = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Account/GetAppUser?id=" + id, callbackFunction);
        };

        this.getRoles = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Account/GetRoles", callbackFunction);
        };

        this.editAppUser = function (user, callbackFunction) {
            return ajaxServices.AjaxPost({ model: user }, "/Account/UpdateUser", callbackFunction);
        };
      
    }]);
});