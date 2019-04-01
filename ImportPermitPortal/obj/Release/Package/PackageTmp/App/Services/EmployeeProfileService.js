﻿define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('employeeProfileService', ['ajaxServices', function (ajaxServices) {
        this.addIssue = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/AddIssue", callbackFunction);
        };

        this.getEmployeeProfile = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/GetEmployeeProfile?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/GetGenericList", callbackFunction);
        };

        this.editEmployeeProfile = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/EditEmployeeProfile", callbackFunction);
        };

        this.deleteEmployeeProfile = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/EmployeeProfile/DeleteEmployeeProfile?id=" + id, callbackFunction);
        };

        this.processAcceptApp = function (employeeProfile, callbackFunction)
        {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/AcceptApp", callbackFunction);
        };

        this.getAppInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet('/EmployeeProfile/GetApplication?id=' + id, callbackFunction);
        };
        
        this.getEligibility = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/GeneralInformation/CheckImporterElligibility?id=" + id, callbackFunction);
        };
    }]);
});