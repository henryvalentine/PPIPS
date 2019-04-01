define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('importClassService', ['ajaxServices', function (ajaxServices)
    {
        this.addImportClass = function (importClass, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importClass: importClass }, "/ImportClass/AddImportClass", callbackFunction);
        };

        this.getImportClass = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportClass/GetImportClass?id=" + id, callbackFunction);
        };
        
        this.editImportClass = function (importClass, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importClass: importClass }, "/ImportClass/EditImportClass", callbackFunction);
        };
        
        this.deleteImportClass = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ImportClass/DeleteImportClass?id=" + id, callbackFunction);
        };

    }]);
});