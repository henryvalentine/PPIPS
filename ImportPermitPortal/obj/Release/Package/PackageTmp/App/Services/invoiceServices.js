define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('invoiceService', ['ajaxServices', function (ajaxServices) {
     
        this.getInvoice = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Transaction/GetMyInvoice?id=" + id, callbackFunction);
        };

        this.getadInvoice = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Transaction/GetInvoice?invoiceId=" + id, callbackFunction);
        };

        this.getInvoiceDetails = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Transaction/GetInvoice?invoiceId=" + id, callbackFunction);
        };

        this.generateRrr = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Transaction/GenerateRrr?id=" + id, callbackFunction);
        };

        this.insertPayment = function (rrr, orderId, callbackFunction) 
        {
            return ajaxServices.AjaxGet("/Transaction/InsertPayment?rrr=" + rrr + '&orderId=' + orderId, callbackFunction);
        };

        this.getTransactionDetails = function (rrr, callbackFunction) 
        {
            return ajaxServices.AjaxGet("/Transaction/GetTransactionDetails?rrr=" + rrr, callbackFunction);
        };
   
        this.notify = function (data, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Transaction/Invoice/" + data, callbackFunction);
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