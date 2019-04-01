define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('documentTypeRightService', ['ajaxServices', function (ajaxServices)
    {
        this.addDocumentTypeRight = function (documentTypeRight, callbackFunction)
        {
            return ajaxServices.AjaxPost({ documentTypeRight: documentTypeRight }, "/DocumentTypeRight/AddDocumentTypeRight", callbackFunction);
        };

        this.getDocumentTypeRight = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/DocumentTypeRight/GetDocumentTypeRight?roleId=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/DocumentTypeRight/GetGenericList", callbackFunction);
        };

        this.editDocumentTypeRight = function (documentTypeRight, callbackFunction)
        {
            return ajaxServices.AjaxPost({ newReqs: documentTypeRight }, "/DocumentTypeRight/EditDocumentTypeRight", callbackFunction);
        };

        this.deleteDocumentTypeRight = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/DocumentTypeRight/DeleteDocumentTypeRight?id=" + id, callbackFunction);
        };
    

    }]);
});