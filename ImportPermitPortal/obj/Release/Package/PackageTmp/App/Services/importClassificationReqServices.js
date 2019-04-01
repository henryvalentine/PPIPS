define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('importClassReqService', ['ajaxServices', function (ajaxServices)
    {
        this.addImportClassificationRequirement = function (iClassReq, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importClassificationRequirements: iClassReq }, "/ImportClassificationRequirement/AddImportClassificationRequirement", callbackFunction);
        };

        this.getImportClassificationRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportClassificationRequirement/GetImportClassificationRequirement?id=" + id, callbackFunction);
        };
        
        this.editImportClassificationRequirement = function (iClassReq, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importClassificationRequirements: iClassReq }, "/ImportClassificationRequirement/EditImportClassificationRequirement", callbackFunction);
        };

        this.deleteImportClassificationRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ImportClassificationRequirement/DeleteImportClassificationRequirement?id=" + id, callbackFunction);
        };
    
        this.getList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportClassificationRequirement/GetGenericList", callbackFunction);
        };

    }]);
});