define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('notificationService', ['ajaxServices', function (ajaxServices)
    {
        this.addNotification = function (notification, callbackFunction)
        {
            return ajaxServices.AjaxPost({ notification: notification }, "/Notification/Notification", callbackFunction);
        };

        this.editNotification = function (importNotification, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importNotification: importNotification }, "/Notification/EditNotification", callbackFunction);
        };

        this.getApplicationByPermitValue = function (permitValue, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetPermitApplicationByPermitValue?permitValue=" + permitValue, callbackFunction);
        };
        this.getAmountDueWithReqs = function (notificationProps, callbackFunction)
        {
            return ajaxServices.AjaxPost({ notificationProps: notificationProps }, "/Notification/ComputeAmountDueWithRequirements", callbackFunction);
        };

        this.refreshSession = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Notification/RefreshSession", callbackFunction);
        };

        this.getNotification = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotification?id=" + id, callbackFunction);
        };
        
        this.saveDocInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxPost("/Document/SaveStageFile?docTypeId=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetGenericList", callbackFunction);
        };

        //Notification/GetApplicantsActivePermits
        
        this.deleteImportNotification = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Notification/DeleteNotification?id=" + id, callbackFunction);
        };
    

    }]);
});