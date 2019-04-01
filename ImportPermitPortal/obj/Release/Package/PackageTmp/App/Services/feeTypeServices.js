define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('feeTypeService', ['ajaxServices', function (ajaxServices)
    {
        this.addFeeType = function (feeType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ feeType: feeType }, "/FeeType/AddFeeType", callbackFunction);
        };

        this.getFeeType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/FeeType/GetFeeType?id=" + id, callbackFunction);
        };
        
        this.editFeeType = function (feeType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ feeType: feeType }, "/FeeType/EditFeeType", callbackFunction);
        };
        
        this.deleteFeeType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/FeeType/DeleteFeeType?id=" + id, callbackFunction);
        };
     

    }]);
});