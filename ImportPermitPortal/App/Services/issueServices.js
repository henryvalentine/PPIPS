define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('issueService', ['ajaxServices', function (ajaxServices)
    {
        this.addIssue = function (issue, callbackFunction)
        {
            return ajaxServices.AjaxPost({ issue: issue }, "/Issue/AddIssue", callbackFunction);
        };

        this.getIssue = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Issue/GetIssue?id=" + id, callbackFunction);
        };
        
        this.resolveIssue = function (issue, callbackFunction)
        {
            return ajaxServices.AjaxPost({ issue: issue }, "/Issue/ResolveIssue", callbackFunction);
        };

        this.deleteIssue = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Issue/DeleteIssue?id=" + id, callbackFunction);
        };
    

    }]);
});