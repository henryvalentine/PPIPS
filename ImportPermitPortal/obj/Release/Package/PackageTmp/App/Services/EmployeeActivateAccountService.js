define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('employeeActivateAccountService', ['ajaxServices', function (ajaxServices) {
        this.activateAccount = function (calculator, callbackFunction) {
            return ajaxServices.AjaxPost({ calculator: calculator }, "/Account/ActivateAccount", callbackFunction);
        };

        this.getCalculator = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Calculator/GetCalculator?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Calculator/GetGenericList", callbackFunction);
        };

        this.editCalculator = function (calculator, callbackFunction) {
            return ajaxServices.AjaxPost({ calculator: calculator }, "/Calculator/EditCalculator", callbackFunction);
        };

        this.deleteCalculator = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Calculator/DeleteCalculator?id=" + id, callbackFunction);
        };


    }]);
});