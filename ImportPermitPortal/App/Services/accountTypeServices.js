define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('accountTypeService', ['ajaxServices', function (ajaxServices)
    {
        this.addAccountType = function (accountType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ accountType: accountType }, "/AccountType/AddAccountType", callbackFunction);
        };

        this.getAccountType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/AccountType/GetAccountType?id=" + id, callbackFunction);
        };
        
        this.editAccountType = function (accountType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ accountType: accountType }, "/AccountType/EditAccountType", callbackFunction);
        };
        
        this.deleteAccountType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/AccountType/DeleteAccountType?id=" + id, callbackFunction);
        };
     
        this.getItem = function (url, callbackFunction)
        {
            return ajaxServices.AjaxGet(url, callbackFunction);
        };

    }]);
});