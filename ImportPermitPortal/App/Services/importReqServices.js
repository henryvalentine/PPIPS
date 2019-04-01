define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('importReqService', ['ajaxServices', function (ajaxServices)
    {
        this.addImportRequirement = function (iClassReq, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importRequirements: iClassReq }, "/ImportRequirement/AddImportRequirement", callbackFunction);
        };

        this.getImportRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportRequirement/GetImportRequirement?id=" + id, callbackFunction);
        };
        
        this.editImportRequirement = function (iClassReq, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importRequirements: iClassReq }, "/ImportRequirement/EditImportRequirement", callbackFunction);
        };

        this.deleteImportRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ImportRequirement/DeleteImportRequirement?id=" + id, callbackFunction);
        };
    
        this.getList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportRequirement/GetGenericList", callbackFunction);
        };

    }]);
});