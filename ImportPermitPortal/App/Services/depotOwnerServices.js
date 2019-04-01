define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('depotOwnerService', ['ajaxServices', function (ajaxServices)
    {
        this.addthroughPut = function (throughPutObj, callbackFunction)
        {
            return ajaxServices.AjaxPost({ throughPut: throughPutObj }, "/ThroughPut/AddThroughPutByDepotOwner", callbackFunction);
        };
        var appItem = {};
        this.setApp = function (app)
        {
            appItem = app;
        };

        this.getApp = function ()
        {
            return appItem;
        };

        this.getAssignedApp = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetDepotOwnerApplicationItem?id=" + id, callbackFunction);
        };

        this.updatethroughPut = function (throughPutObj, callbackFunction)
        {
            return ajaxServices.AjaxPost({ throughPut: throughPutObj }, "/ThroughPut/EditThroughPut", callbackFunction);
        };
        
    }]);
});