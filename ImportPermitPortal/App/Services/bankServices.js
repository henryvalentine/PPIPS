define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('bankService', ['ajaxServices', function (ajaxServices)
    {
        this.addBank = function (bank, callbackFunction)
        {
            return ajaxServices.AjaxPost({ bank: bank }, "/Bank/AddBank", callbackFunction);
        };

        this.getBank = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Bank/GetBank?id=" + id, callbackFunction);
        };

        this.getBankAdmin = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Bank/GetBankAdmin?id=" + id, callbackFunction);
        };

        this.addBankAdmin = function (user, callbackFunction)
        {
            return ajaxServices.AjaxPost({ model: user }, "/Account/RegisterBankAdmin", callbackFunction);
        };

        this.UpdateBankAdmin = function (user, callbackFunction) {
            return ajaxServices.AjaxPost({ model: user }, "/Account/UpdateBankAdmin", callbackFunction);
        };

        
        this.getBanks = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Bank/GetBanks", callbackFunction);
        };
        
        this.editBank = function (bank, callbackFunction)
        {
            return ajaxServices.AjaxPost({ bank: bank }, "/Bank/EditBank", callbackFunction);
        };

        this.deleteBank = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Bank/DeleteBank?id=" + id, callbackFunction);
        };
    

    }]);
});