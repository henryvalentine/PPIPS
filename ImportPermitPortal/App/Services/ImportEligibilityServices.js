define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('importEligibilityService', ['ajaxServices', function (ajaxServices)
    {
        this.addImportEligibility = function (importEligibility, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importEligibility: importEligibility }, "/ImportEligibility/AddImportEligibility", callbackFunction);
        };

        this.getImportEligibility = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportEligibility/GetImportEligibility?id=" + id, callbackFunction);
        };
        
        this.setDocSession = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Document/SetDocSession?id=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ImportEligibility/GetList", callbackFunction);
        };

        this.editImportEligibility = function (importEligibility, callbackFunction)
        {
            return ajaxServices.AjaxPost({ importEligibility: importEligibility }, "/ImportEligibility/EditImportEligibility", callbackFunction);
        };

        this.getImportEligibilityBrandSearch = function (searchCriteria, callbackFunction)
        {
            return ajaxServices.AjaxGet({ searchCriteria: searchCriteria }, "/ImportEligibility/GetImportEligibilityBrandObjects", callbackFunction);
        };

        this.deleteImportEligibility = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ImportEligibility/DeleteImportEligibility?id=" + id, callbackFunction);
        };
    

    }]);
});