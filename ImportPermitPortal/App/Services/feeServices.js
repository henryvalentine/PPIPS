define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('feeService', ['ajaxServices', function (ajaxServices)
    {
        this.addFee = function (fee, callbackFunction)
        {
            return ajaxServices.AjaxPost({ fee: fee }, "/Fee/AddFee", callbackFunction);
        };

        this.getFee = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Fee/GetFee?id=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Fee/GetGenericList", callbackFunction);
        };

        this.editFee = function (fee, callbackFunction)
        {
            return ajaxServices.AjaxPost({ fee: fee }, "/Fee/EditFee", callbackFunction);
        };

        this.deleteFee = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Fee/DeleteFee?id=" + id, callbackFunction);
        };
    

    }]);
});