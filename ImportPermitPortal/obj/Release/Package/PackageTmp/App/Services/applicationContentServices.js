define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('applicationContentService', ['ajaxServices', function (ajaxServices)
    {
        this.addApplicationContent = function (applicationContent, callbackFunction)
        {
            return ajaxServices.AjaxPost({ applicationContent: applicationContent }, "/ApplicationContent/AddApplicationContent", callbackFunction);
        };

        this.getApplicationContent = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ApplicationContent/GetApplicationContentForEdit?id=" + id, callbackFunction);
        };
        
        this.getApplicationContents = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ApplicationContent/GetApplicationContents", callbackFunction);
        };

        this.editApplicationContent = function (applicationContent, callbackFunction)
        {
            return ajaxServices.AjaxPost({ applicationContent: applicationContent }, "/ApplicationContent/EditApplicationContent", callbackFunction);
        };

        this.deleteApplicationContent = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ApplicationContent/DeleteApplicationContent?id=" + id, callbackFunction);
        };

    }]);
});