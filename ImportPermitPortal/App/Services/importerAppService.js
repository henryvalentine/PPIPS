define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('importerAppService', ['ajaxServices', function (ajaxServices)
    {
        this.getApplication = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplication?id=" + id, callbackFunction);
        };
       
        this.getAppForEditing = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppForEdit?id=" + id, callbackFunction);
        };

        this.getImportAppDocsX = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppXDocuments?id=" + id, callbackFunction);
        };

        this.getAppResult = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetApplicationDetails?id=" + id, callbackFunction);
        };


        this.setImporterId = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/ImporterId/SetImporterId?id=" + id, callbackFunction);
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

        this.getApp = function ()
        {
            return app;
        };
        
      
        this.getApplicationDocuments = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetAppDocuments?id=" + id, callbackFunction);
        };
       
       
        this.getAppStageDocRequirements = function (docData, callbackFunction) {
            return ajaxServices.AjaxPost({ requirementProp: docData }, "/Application/GetDocRequirements", callbackFunction);
        };
        
    }]);
});