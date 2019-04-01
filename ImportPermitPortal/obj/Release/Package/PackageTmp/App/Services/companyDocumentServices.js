define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('companyDocumentService', ['ajaxServices', function (ajaxServices)
    {
        this.addCompanyDocument = function (doc, callbackFunction)
        {
            return ajaxServices.AjaxPost({ model: doc }, "/CompanyDocument/AddCompanyDocument", callbackFunction);
        };

        this.getCompanyDocument = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/CompanyDocument/GetDocument?id=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/CompanyDocument/GetStandardReqTypes", callbackFunction);
        };

        this.editCompanyDocument = function (doc, callbackFunction)
        {
            return ajaxServices.AjaxPost({ model: doc }, "/CompanyDocument/EditCompanyDocument", callbackFunction);
        };

        this.deleteCompanyDocument = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/CompanyDocument/DeleteDocument?id=" + id, callbackFunction);
        };
    

    }]);
});