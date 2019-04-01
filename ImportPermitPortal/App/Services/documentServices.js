define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('documentServices', ['ajaxServices', function (ajaxServices)
    {
        this.addDocument = function (document, callbackFunction)
        {
            return ajaxServices.AjaxPost({ document: document }, "/Document/AddDocument", callbackFunction);
        };

        this.getDocument = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/GetDocument?id=" + id, callbackFunction);
        };
        
        this.getDocuments = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/MyDocuments", callbackFunction);
        };
        
        this.getDocumentTypes = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/GetDocumentTypes", callbackFunction);
        };
        
        this.getApplications = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/GetApplications", callbackFunction);
        };

        this.editDocument = function (document, callbackFunction)
        {
            return ajaxServices.AjaxPost({ document: document }, "/Document/EditDocument", callbackFunction);
        };

        this.deleteDocument = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Document/DeleteDocument?id=" + id, callbackFunction);
        };
    

    }]);
});