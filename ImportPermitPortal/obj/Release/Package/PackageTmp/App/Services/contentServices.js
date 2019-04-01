define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('contentService', ['ajaxServices', function (ajaxServices)
    {
        
        this.getContent = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ApplicationContent/GetApplicationContent?id=" + id, callbackFunction);
        };

    }]);
});