define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('documentRequirementService', ['ajaxServices', function (ajaxServices)
    {
        this.addDocumentsRequirement = function (documentRequirement, callbackFunction)
        {
            return ajaxServices.AjaxPost({ documentsRequirement: documentRequirement }, "/DocumentsRequirement/AddDocumentsRequirement", callbackFunction);
        };

        this.getDocumentsRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/DocumentsRequirement/GetDocumentsRequirement?id=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/DocumentsRequirement/GetGenericList", callbackFunction);
        };

        this.editDocumentsRequirement = function (documentRequirement, callbackFunction)
        {
            return ajaxServices.AjaxPost({ documentsRequirement: documentRequirement }, "/DocumentsRequirement/EditDocumentsRequirement", callbackFunction);
        };

        this.deleteDocumentsRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/DocumentsRequirement/DeleteDocumentsRequirement?id=" + id, callbackFunction);
        };
    

    }]);
});