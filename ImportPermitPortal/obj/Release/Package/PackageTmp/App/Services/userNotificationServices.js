define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('userNotificationService', ['ajaxServices', function (ajaxServices)
    {
        this.getNotificationInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotification?id=" + id, callbackFunction);
        };

        this.getNotificationForEditing = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Admin/GetNotificationAdmin?id=" + id, callbackFunction);
        };

        this.getNotificationForEdit = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Notification/GetNotificationForEdit?id=" + id, callbackFunction);
        };


        this.printNotification = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Admin/PrintDischargeDataNow?id=" + id, callbackFunction);
        };

        this.getShuttleVessels = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Vessel/GetShuttleVessels", callbackFunction);
        };
        
        this.checkNotificationSubmit = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/CheckNotificationForSubmit?id=" + id, callbackFunction);
        };

        this.getDepotCollection = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetDepotCollection", callbackFunction);
        };

        this.submitNotification = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/SubmitNotification?id=" + id, callbackFunction);
        };
        
        this.getNotificationVesssels = function (notificationId, callbackFunction)
        {
            return ajaxServices.AjaxGet("/NotificationVessel/GetNotificationVesselsByNotification?notificationId=" + notificationId, callbackFunction);
        };

        this.VerifyVesselLicense = function (codeVerifier, callbackFunction) {
            return ajaxServices.AjaxPost({ verifier: codeVerifier }, "/Application/VerifyVesselLicense", callbackFunction);
        };
       
        this.addNotificationVessels = function (notificationVessels, callbackFunction)
        {
            return ajaxServices.AjaxPost({ notificationVessels: notificationVessels }, "/NotificationVessel/AddNotificationVessels", callbackFunction);
        };

        this.getRRR = function (ntfDetails, callbackFunction)
        {
            return ajaxServices.AjaxPost({ details: ntfDetails }, "/Notification/GetRrr", callbackFunction);
        };
      
        this.updateNotificationVessels = function (notificationVessels, callbackFunction)
        {
            return ajaxServices.AjaxPost({ notificationVessels: notificationVessels }, "/NotificationVessel/UpdateNotificationVessels", callbackFunction);
        };

        this.refreshSession = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Notification/RefreshSession", callbackFunction);
        };

        this.getNotificationResult = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotificationDetails?id=" + id, callbackFunction);
        };

        var appId = 0;

        this.setNotificationId = function (id)
        {
            appId = id;
        };

        this.getNotificationId = function ()
        {
          return appId;
        };

        var app = {};

        this.setNotification = function (gh)
        {
            app = gh;
        };

        this.getNotification = function ()
        {
            return app;
        };
        
        this.setDocSession = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/SetDocSession?id=" + id, callbackFunction);
        };
       
        this.updateDepot = function (notificationId, depotId, callbackFunction) {
            return ajaxServices.AjaxPostNoData("/Document/UpdateDepot?notificationId=" + notificationId + '&depotId=' + depotId, callbackFunction);
        };
        
        this.getNotificationDocuments = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotificationDocuments?id=" + id, callbackFunction);
        };
       
        this.saveDocInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxPost("/Document/SaveStageFile?docTypeId=" + id, callbackFunction);
        };
        
        this.deleteNotification = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Notification/DeleteNotification?id=" + id, callbackFunction);
        };
    
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetGenericList", callbackFunction);
        };

        this.editNotification = function (notification, callbackFunction)
        {
            return ajaxServices.AjaxPost({ iNotification: notification }, "/Notification/EditNotification", callbackFunction);
        };
        this.getAmountDueWithReqs = function (notificationProps, callbackFunction) {
            return ajaxServices.AjaxPost({ notificationProps: notificationProps }, "/Notification/ComputeAmountDueWithRequirements", callbackFunction);
        };

        this.getEligibility = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/GeneralInformation/CheckImporterElligibility?id=" + id, callbackFunction);
        };
        
        this.getNotificationProcesses = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Notification/GetNotificationProcesses?notificationId=" + id, callbackFunction);
        };
    }]);
}); 