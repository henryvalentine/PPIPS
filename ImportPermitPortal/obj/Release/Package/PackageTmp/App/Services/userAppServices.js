define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('userAppService', ['ajaxServices', function (ajaxServices)
    {
        this.getApplication = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplication?id=" + id, callbackFunction);
        };
        this.addCompanyDocument = function (doc, callbackFunction) {
            return ajaxServices.AjaxPost({ model: doc }, "/CompanyDocument/AddCompanyDocument", callbackFunction);
        };

        this.addCompanyDoc = function (doc, callbackFunction) {
            return ajaxServices.AjaxPost({ model: doc }, "/CompanyDocument/AddCompanyDoc", callbackFunction);
        };

        this.getAppForEditing = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppForEdit?id=" + id, callbackFunction);
        };

        this.getAppForPayment = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetAppForPayment?id=" + id, callbackFunction);
        };

        this.getAppForEdit = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetAppForEdit?id=" + id, callbackFunction);
        };

        this.getImportAppDocsX = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppXDocuments?id=" + id, callbackFunction);
        };

        this.getAppResult = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetApplicationDetails?id=" + id, callbackFunction);
        };

        this.checkAppSubmit = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/CheckToAppSubmit?id=" + id, callbackFunction);
        };

        this.submitApp = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/AppSubmit?id=" + id, callbackFunction);
        };
        
        this.addBankerInfo = function (applicationBanker, callbackFunction) {
            return ajaxServices.AjaxPost({ applicationBanker: applicationBanker }, "/Application/AddAppBanker", callbackFunction);
        };

        this.updateBankAccounts = function (banks, callbackFunction) {
            return ajaxServices.AjaxPost({ bankers: banks }, "/Application/UpdateBankAccounts", callbackFunction);
        };

        this.editBankerInfo = function (applicationBanker, callbackFunction) {
            return ajaxServices.AjaxPost({ applicationBanker: applicationBanker }, "/Application/EditAppBanker", callbackFunction);
        };

        var appId = 0;

        this.setAppId = function (id)
        {
            appId = id;
        };

        this.getAppId = function ()
        {
          return appId;
        };

        var app = {};
        var genList = [];
        this.setApp = function (gh)
        {
            app = gh;
        };

        this.setgenList = function (qlist) {
            genList = qlist;
        };

        this.getgenList = function ()
        {
            return genList;
        };


        this.getApp = function ()
        {
            return app;
        };
        
        this.setDocSession = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/SetDocSession?id=" + id, callbackFunction);
        };

        this.getEligibility = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/GeneralInformation/CheckImporterElligibility?id=" + id, callbackFunction);
        };

        this.updateAppPaymentOption = function (ptyid, appid, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/UpdateAppPaymentOption?paymentTypeId=" + ptyid + '&applicationId=' + appid, callbackFunction);
        };

        this.getApplicationDocuments = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppDocuments?id=" + id, callbackFunction);
        };
       
        this.saveDocInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxPost("/Document/SaveStageFile?docTypeId=" + id, callbackFunction);
        };
        
        this.deleteApplication = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Application/DeleteApplication?id=" + id, callbackFunction);
        };
    
        this.getBanks = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Bank/GetBanks", callbackFunction);
        };

        this.getList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetList", callbackFunction);
        };

        this.editApplication = function (importApplication, callbackFunction) {
            return ajaxServices.AjaxPost({ importApplication: importApplication }, "/Application/EditApplication", callbackFunction);
        };

        this.calculateDerivedValue = function (importApplication, callbackFunction) {
            return ajaxServices.AjaxPost({ application: importApplication }, "/Application/ComputeDerivedValue", callbackFunction);
        };

        this.getPortsAndCountries = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetPortsAndCountries", callbackFunction);
        };
        this.getItems = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetList", callbackFunction);
        };

        this.verifyCode = function (codeVerifier, callbackFunction) {
            return ajaxServices.AjaxPost({ verifier: codeVerifier }, "/Application/VerifyCode", callbackFunction);
        };

        this.VerifyDepotLicenseCode = function (codeVerifier, callbackFunction) {
            return ajaxServices.AjaxPost({ verifier: codeVerifier }, "/Application/VerifyDepotLicenseCode", callbackFunction);
        };


        this.verifyAppByRefCode = function (code, callbackFunction) {
            return ajaxServices.AjaxGetWithOutLoader("/Application/GetAppForRenewal?referenceCode=" + code, callbackFunction);
        };

        this.getAppForInclusion = function (code, callbackFunction) {
            return ajaxServices.AjaxGetWithOutLoader("/Application/GetAppForInclusion?referenceCode=" + code, callbackFunction);
        };

        this.getAppStageDocRequirements = function (docData, callbackFunction) {
            return ajaxServices.AjaxPost({ requirementProp: docData }, "/Application/GetDocRequirements", callbackFunction);
        };

        this.setDocSession = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Document/SetDocSession?id=" + id, callbackFunction);
        };

        this.saveDocInfo = function (id, callbackFunction) {
            return ajaxServices.AjaxPost("/Document/SaveStageFile?docTypeId=" + id, callbackFunction);
        };


        this.getRunList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetRunList", callbackFunction);
        };

        this.getPorts = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetPorts", callbackFunction);
        };

        //Company Address

        this.deleteAddress = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/GeneralInformation/DeleteAddress?id=" + id, callbackFunction);
        };

        this.checkAddressAvailability = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/GeneralInformation/CheckAddressAvailability", callbackFunction);
        };

        this.processCompanyAddresses = function (model, callbackFunction) {
            return ajaxServices.AjaxPost({ addresses: model }, "/GeneralInformation/ProcessCompanyAddresses", callbackFunction);
        };
        
        this.addthroughPut = function (throughPutObj, callbackFunction)
        {
            return ajaxServices.AjaxPost({ throughPut: throughPutObj }, "/ThroughPut/AddThroughPutByApplicant", callbackFunction);
        };
        this.updatethroughPut = function (throughPutObj, callbackFunction)
        {
            return ajaxServices.AjaxPost({ throughPut: throughPutObj }, "/ThroughPut/EditThroughPut", callbackFunction);
        };

         
     
    }]);
});