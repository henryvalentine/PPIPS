define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('workFlowService', ['ajaxServices', function (ajaxServices) {
        this.addWorkFlow = function (workFlow, callbackFunction) {
            return ajaxServices.AjaxPost({ workFlow: workFlow }, "/WorkFlow/AddWorkFlow", callbackFunction);
        };

        this.getWorkFlow = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/WorkFlow/GetWorkFlow?id=" + id, callbackFunction);
        };

        this.editWorkFlow = function (workFlow, callbackFunction) {
            return ajaxServices.AjaxPost({ workFlow: workFlow }, "/WorkFlow/EditWorkFlow", callbackFunction);
        };

        this.deleteWorkFlow = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/WorkFlow/DeleteWorkFlow?id=" + id, callbackFunction);
        };

        

    }]);
});