define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('applicationService', ['ajaxServices', function (ajaxServices)
    {
        this.addApplication = function (importApplication, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importApplication: importApplication }, "/Application/Application", callbackFunction);
        };

        this.calculateDerivedValue = function (importApplication, callbackFunction)
        {
            return ajaxServices.AjaxPost({ application: importApplication }, "/Application/ComputeDerivedValue", callbackFunction);
        };

        this.verifyCode = function (codeVerifier, callbackFunction)
        {
            return ajaxServices.AjaxPost({ verifier: codeVerifier }, "/Application/VerifyCode", callbackFunction);
        };

        this.VerifyDepotLicenseCode = function (codeVerifier, callbackFunction) {
            return ajaxServices.AjaxPost({ verifier: codeVerifier }, "/Application/VerifyDepotLicenseCode", callbackFunction);
        };

        this.verifyAppByRefCode = function (code, callbackFunction)
        {
            return ajaxServices.AjaxGetWithOutLoader("/Application/GetAppForRenewal?referenceCode=" + code, callbackFunction);
        };

        this.getAppForInclusion = function (code, callbackFunction)
        {
            return ajaxServices.AjaxGetWithOutLoader("/Application/GetAppForInclusion?referenceCode=" + code, callbackFunction);
        };

        this.getAppStageDocRequirements = function (docData, callbackFunction)
        {
            return ajaxServices.AjaxPost({ requirementProp: docData }, "/Application/GetDocRequirements", callbackFunction);
        };

        this.getApplication = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplication?id=" + id, callbackFunction);
        };
        
        this.setDocSession = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/SetDocSession?id=" + id, callbackFunction);
        };

        this.saveDocInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxPost("/Document/SaveStageFile?docTypeId=" + id, callbackFunction);
        };

        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetList", callbackFunction);
        };
        
        this.getPortsAndCountries = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetPortsAndCountries", callbackFunction);
        };

        this.addBankerInfo = function (applicationBanker, callbackFunction)
        {
             return ajaxServices.AjaxPost({ applicationBanker: applicationBanker }, "/Application/AddAppBanker", callbackFunction);
        };
        this.editApplication = function (importApplication, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importApplication: importApplication }, "/Application/EditApplication", callbackFunction);
        };
        
        this.getApplicationBrandSearch = function (searchCriteria, callbackFunction)
        {
            return ajaxServices.AjaxGet({ searchCriteria: searchCriteria }, "/Application/GetApplicationBrandObjects", callbackFunction);
        };

        this.deleteApplication = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Application/DeleteApplication?id=" + id, callbackFunction);
        };
    

    }]);
});