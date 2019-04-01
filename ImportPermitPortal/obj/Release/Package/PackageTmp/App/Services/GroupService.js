define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('groupService', ['ajaxServices', function (ajaxServices) {
        this.addGroup = function (group, callbackFunction) {
            return ajaxServices.AjaxPost({ group: group }, "/Group/AddGroup", callbackFunction);
        };

        this.getGroup = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Group/GetGroup?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Group/GetGenericList", callbackFunction);
        };

        this.editGroup = function (group, callbackFunction) {
            return ajaxServices.AjaxPost({ group: group }, "/Group/EditGroup", callbackFunction);
        };

        this.deleteGroup = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Group/DeleteGroup?id=" + id, callbackFunction);
        };


    }]);
});