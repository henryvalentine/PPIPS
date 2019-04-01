define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('importStageService', ['ajaxServices', function (ajaxServices)
    {
        this.addImportStage = function (importStage, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importStage: importStage }, "/ImportStage/AddImportStage", callbackFunction);
        };

        this.getImportStage = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportStage/GetImportStage?id=" + id, callbackFunction);
        };
        
        this.editImportStage = function (importStage, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importStage: importStage }, "/ImportStage/EditImportStage", callbackFunction);
        };
        
        this.deleteImportStage = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ImportStage/DeleteImportStage?id=" + id, callbackFunction);
        };
    
        this.getDocumentTypes = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Document/GetDocumentTypes", callbackFunction);
        };

    }]);
});