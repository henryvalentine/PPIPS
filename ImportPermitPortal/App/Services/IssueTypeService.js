define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('issueTypeService', ['ajaxServices', function (ajaxServices) {
        this.addIssueType = function (issueType, callbackFunction) {
            return ajaxServices.AjaxPost({ issueType: issueType }, "/IssueType/AddIssueType", callbackFunction);
        };

        this.getIssueType = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/IssueType/GetIssueType?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/IssueType/GetGenericList", callbackFunction);
        };

        this.editIssueType = function (issueType, callbackFunction) {
            return ajaxServices.AjaxPost({ issueType: issueType }, "/IssueType/EditIssueType", callbackFunction);
        };

        this.deleteIssueType = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/IssueType/DeleteIssueType?id=" + id, callbackFunction);
        };


    }]);
});