define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('generalInformationService', ['ajaxServices', function (ajaxServices)
    {
        this.processGeneralInformation = function (gInformation, callbackFunction)
        {
            return ajaxServices.AjaxPost({ model: gInformation }, "/GeneralInformation/ProcessGeneralInformation", callbackFunction);
        };
        
        this.getGeneralInformation = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/GeneralInformation/GetGeneralInformation", callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/GeneralInformation/GetList", callbackFunction);
        };
        
        this.deleteAddress = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/GeneralInformation/DeleteAddress?id=" + id, callbackFunction);
        };

    }]);
});