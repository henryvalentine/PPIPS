define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('applicationIssueService', ['ajaxServices', function (ajaxServices) {
        this.addIssue11 = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/ApplicationIssue/AddIssue", callbackFunction);
        };

        this.getApplicationIssue = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/ApplicationIssue/GetApplicationIssue?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/ApplicationIssue/GetGenericList", callbackFunction);
        };

        this.editApplicationIssue = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/ApplicationIssue/EditApplicationIssue", callbackFunction);
        };

        this.deleteApplicationIssue = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/ApplicationIssue/DeleteApplicationIssue?id=" + id, callbackFunction);
        };


    }]);
});