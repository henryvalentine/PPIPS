define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('documentTypeService', ['ajaxServices', function (ajaxServices)
    {
        this.addDocumentType = function (documentType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ documentType: documentType }, "/DocumentType/AddDocumentType", callbackFunction);
        };

        this.getDocumentType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/DocumentType/GetDocumentType?id=" + id, callbackFunction);
        };
        
        this.editDocumentType = function (documentType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ documentType: documentType }, "/DocumentType/EditDocumentType", callbackFunction);
        };
        
        this.deleteDocumentType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/DocumentType/DeleteDocumentType?id=" + id, callbackFunction);
        };

    }]);
});