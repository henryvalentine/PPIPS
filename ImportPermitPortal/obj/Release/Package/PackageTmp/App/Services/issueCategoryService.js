define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('issueCategoryService', ['ajaxServices', function (ajaxServices)
    {
        this.addIssueCategory = function (issueCategory, callbackFunction)
        {
            return ajaxServices.AjaxPost({ issueCategory: issueCategory }, "/IssueCategory/AddIssueCategory", callbackFunction);
        };

        this.getIssueCategory = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/IssueCategory/GetIssueCategory?id=" + id, callbackFunction);
        };

        this.editIssueCategory = function (issueCategory, callbackFunction)
        {
            return ajaxServices.AjaxPost({ issueCategory: issueCategory }, "/IssueCategory/EditIssueCategory", callbackFunction);
        };

        this.deleteIssueCategory = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/IssueCategory/DeleteIssueCategory?id=" + id, callbackFunction);
        };

    }]);
});