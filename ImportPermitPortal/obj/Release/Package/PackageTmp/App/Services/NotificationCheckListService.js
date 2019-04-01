define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('notificationCheckListService', ['ajaxServices', function (ajaxServices) {
        this.addNotificationCheckList = function (notificationCheckList, callbackFunction) {
            return ajaxServices.AjaxPost({ notificationCheckList: notificationCheckList }, "/NotificationCheckList/AddNotificationCheckList", callbackFunction);
        };

        this.getNotificationCheckList = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/NotificationCheckList/GetNotificationCheckList?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/NotificationCheckList/GetGenericList", callbackFunction);
        };

        this.RefreshAppStage = function (callbackFunction) {
            return ajaxServices.AjaxGet("/NotificationCheckList/RefreshApplicationStage", callbackFunction);
        };

        this.editNotificationCheckList = function (notificationCheckList, callbackFunction) {
            return ajaxServices.AjaxPost({ notificationCheckList: notificationCheckList }, "/NotificationCheckList/EditNotificationCheckList", callbackFunction);
        };

        this.deleteNotificationCheckList = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/NotificationCheckList/DeleteNotificationCheckList?id=" + id, callbackFunction);
        };

        this.getProcessesByStage = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/NotificationCheckList/GetProcessesByStage?id=" + id, callbackFunction);
        };
    }]);
});