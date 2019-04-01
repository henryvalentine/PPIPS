define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('employeeDeskService', ['ajaxServices', function (ajaxServices) {
        this.addEmployeeDesk = function (employeeDesk, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeDesk: employeeDesk }, "/Account/AddEmployeeDesk", callbackFunction);
        };

        this.getEmployeeDesk = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeDesk/GetEmployeeDesk?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeDesk/GetGenericList", callbackFunction);
        };

        this.editEmployeeDesk = function (employeeDesk, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeDesk: employeeDesk }, "/EmployeeDesk/EditEmployeeDesk", callbackFunction);
        };

        this.deleteEmployeeDesk = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/EmployeeDesk/DeleteEmployeeDesk?id=" + id, callbackFunction);
        };

      
    }]);
});