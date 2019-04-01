define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('appSettingService', ['ajaxServices', function (ajaxServices)
    {
        this.addApplicationSetting = function (applicationSetting, callbackFunction)
        {
            return ajaxServices.AjaxPost({ applicationSetting: applicationSetting }, "/ApplicationSetting/AddApplicationSetting", callbackFunction);
        };

        this.getApplicationSetting = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ApplicationSetting/GetApplicationSetting", callbackFunction);
        };
        
        this.editApplicationSetting = function (applicationSetting, callbackFunction)
        {
            return ajaxServices.AjaxPost({ applicationSetting: applicationSetting }, "/ApplicationSetting/EditApplicationSetting", callbackFunction);
        };
        
        this.deleteApplicationSetting = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ApplicationSetting/DeleteApplicationSetting?id=" + id, callbackFunction);
        };
    
    }]);
});