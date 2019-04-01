define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('customCodeService', ['ajaxServices', function (ajaxServices)
    {
        this.addCustomCode = function (customCode, callbackFunction)
        {
            return ajaxServices.AjaxPost({ customCode: customCode }, "/CustomCode/AddCustomCode", callbackFunction);
        };

        this.getCustomCode = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/CustomCode/GetCustomCode?id=" + id, callbackFunction);
        };
       
        this.editCustomCode = function (customCode, callbackFunction)
        {
            return ajaxServices.AjaxPost({ customCode: customCode }, "/CustomCode/EditCustomCode", callbackFunction);
        };

        this.deleteCustomCode = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/CustomCode/DeleteCustomCode?id=" + id, callbackFunction);
        };
    

    }]);
});