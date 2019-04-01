define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('notificationTypeService', ['ajaxServices', function (ajaxServices) {
        this.addNotificationType = function (notificationType, callbackFunction) {
            return ajaxServices.AjaxPost({ notificationType: notificationType }, "/NotificationType/AddNotificationType", callbackFunction);
        };

        this.getNotificationType = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/NotificationType/GetNotificationType?id=" + id, callbackFunction);
        };

        this.editNotificationType = function (notificationType, callbackFunction) {
            return ajaxServices.AjaxPost({ notificationType: notificationType }, "/NotificationType/EditNotificationType", callbackFunction);
        };

        this.deleteNotificationType = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/NotificationType/DeleteNotificationType?id=" + id, callbackFunction);
        };



    }]);
});