define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('receiptService', ['ajaxServices', function (ajaxServices) {
     
        this.getReceipt = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Transaction/GetMyReceipt?id=" + id, callbackFunction);
        };
        this.getReceiptInfo = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Transaction/GetReceipt?id=" + id, callbackFunction);
        };
        var info = {};
        this.setInfo = function (app)
        {
            info = app;
        };
        this.getInfo = function ()
        {
            return info;
        };

    }]);
});