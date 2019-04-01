define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('bnkNotificationService', ['ajaxServices', function (ajaxServices)
    {
        this.getNotification = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotification?id=" + id, callbackFunction);
        };
      
        this.searchNotification = function (code, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/SearchBankNotifications?referenceCode=" + code, callbackFunction);
        };
        
        this.getAppBanker = function (productId,permitValue, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppBankerInfo?productId=" + productId + "&permitValue=" + permitValue, callbackFunction);
        };

        this.getBankNotification = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetBankNotification?id=" + id, callbackFunction);
        };

        this.addAppBanker = function (appBanker, callbackFunction) {
            return ajaxServices.AjaxPost({ applicationBanker: appBanker }, "/Application/AddAppBanker", callbackFunction);
        };

        this.updateAppBanker = function (appBanker, callbackFunction) {
            return ajaxServices.AjaxPost({ applicationBanker: appBanker }, "/Application/EditAppBanker", callbackFunction);
        };

         this.saveFormM = function (formM, callbackFunction)
        {
            return ajaxServices.AjaxPost({ formM: formM }, "/Document/SaveFormM", callbackFunction);
        };
        
        this.updateFormM = function (formM, callbackFunction)
        {
            return ajaxServices.AjaxPost({ formM: formM }, "/Document/UpdateFormM", callbackFunction);
        };
        
        var app = {};

        this.setNotificationX = function (gh)
        {
            app = gh;
        };

        this.getNotificationX = function ()
        {
            return app;
        };
        
        this.getNotificationDocuments = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotificationDocuments?id=" + id, callbackFunction);
        };
       
        this.saveDocInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxPost("/Document/SaveStageFile?docTypeId=" + id, callbackFunction);
        };
        
    }]);
});