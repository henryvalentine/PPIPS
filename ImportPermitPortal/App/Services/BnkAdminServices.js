define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('bnkAdminService', ['ajaxServices', function (ajaxServices)
    {
        this.getAssignedApp = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetBankAssignedAppDocuments?id=" + id, callbackFunction);
        };

        this.getAppDocs = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetBankAssignedAppDocuments?id=" + id, callbackFunction);
        };

        this.getUser = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Account/GetBankUser?id=" + id, callbackFunction);
        };

        this.getBranches = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Bank/GetBankBranches", callbackFunction);
        };

        this.addUser = function (user, callbackFunction)
        { 
            return ajaxServices.AjaxPost({ model: user }, "/Account/RegisterBankUser", callbackFunction); 
        };
        
        this.editUser = function (user, callbackFunction)
        {
            return ajaxServices.AjaxPost({ model: user }, "/Account/UpdateBankUser", callbackFunction);
        };

        this.deleteBank = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Bank/DeleteBankUser?id=" + id, callbackFunction);
        };
    
        var appId = 0;
        var roleInfo = 0;

        this.setRoleInfo = function (id)
        {
            roleInfo = id;
        };

        this.getRoleInfo = function ()
        {
            return roleInfo;
        };

        this.setId = function (id)
        {
            appId = id;
        };

        this.getId = function () {
            return appId;
        };
        
        this.getFormM = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/GetFormMByProduct?id=" + id, callbackFunction);
        };
        
        this.saveFormM = function (formM, callbackFunction)
        {
            return ajaxServices.AjaxPost({ formM: formM }, "/Document/SaveFormM", callbackFunction);
        };
        
        this.updateFormM = function (formM, callbackFunction)
        {
            return ajaxServices.AjaxPost({ formM: formM }, "/Document/UpdateFormM", callbackFunction);
        };

        this.saveDocSession = function (formM, callbackFunction)
        {
            return ajaxServices.AjaxPost({ formM: formM }, "/Document/CreateFileSession", callbackFunction);
        };
        
        this.getHistoryAppDocuments = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppDocuments?id=" + id, callbackFunction);
        };

        this.searchApp = function (refCode, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppDocsByRef?code=" + refCode, callbackFunction);
        };

        this.searchBankApp = function (refCode, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplicationByRef?referenceCode=" + refCode, callbackFunction);
        };
        
        this.addAppBanker = function (appBanker, callbackFunction)
        {
            return ajaxServices.AjaxPost({ applicationBanker: appBanker }, "/Application/AddAppBanker", callbackFunction);
        };

        this.updateAppBanker = function (appBanker, callbackFunction) {
            return ajaxServices.AjaxPost({ applicationBanker: appBanker }, "/Application/EditAppBanker", callbackFunction);
        };

        this.addBankBranch = function (appBanker, callbackFunction)
        {
            return ajaxServices.AjaxPost({ bankBranch: appBanker }, "/Bank/AddBankBranch", callbackFunction);
        };

        this.editBankBranch = function (appBanker, callbackFunction)
        {
            return ajaxServices.AjaxPost({ bankBranch: appBanker }, "/Bank/UpdateBankBranch", callbackFunction);
        };

        this.getBankBranch = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Bank/GetBankBranch?id=" + id, callbackFunction);
        };

        this.getProducts = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetProductList", callbackFunction);
        };

        this.getBankerss = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetBankers", callbackFunction);
        };
    }]);
});

