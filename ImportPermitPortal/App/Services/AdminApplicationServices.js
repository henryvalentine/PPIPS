define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('adminApplicationService', ['ajaxServices', function (ajaxServices)
    {
        this.getAppDetails = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplicationAdmin?id=" + id, callbackFunction);
        };
        
        this.getPaidAppDetails = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Application/GetPaidApplicationAdmin?id=" + id, callbackFunction);
        };

        this.getAppProcesses = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplicationProcesses?applicationId=" + id, callbackFunction);
        };

        this.getEligibility = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/GeneralInformation/CheckImporterElligibility?id=" + id, callbackFunction);
        };

        this.getAppEmployees = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Application/GetApplicationEmployees?applicationId=" + id, callbackFunction);
        };

        this.signOff = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/EmployeeProfile/SignOff?applicationId=" + id, callbackFunction);
        };

       
    }]);
});