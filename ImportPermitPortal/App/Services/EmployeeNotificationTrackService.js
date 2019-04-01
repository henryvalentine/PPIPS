define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('notificationTrackService', ['ajaxServices', function (ajaxServices) {
        this.addIssue = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/AddIssue", callbackFunction);
        };
        this.addReport = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/AddNotificationReport", callbackFunction);
        };

        this.addReportCheckList = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/AddNotificationReportCheckList", callbackFunction);
        };

        this.continueCheckListLater = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/ContinueCheckListLater", callbackFunction);
        };

        this.storeRecertification = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/StoreRecertification", callbackFunction);
        };
        this.saveDischargeData = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/SaveDischargeData", callbackFunction);
        };

        this.updateDischargeData = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/UpdateDischargeData", callbackFunction);
        };

       

        this.submitRecertification = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/SubmitRecertification", callbackFunction);
        };
        this.getNotification = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/GetNotification?id=" + id, callbackFunction);
        };

        this.getDoc = function (docId, callbackFunction) {
            return ajaxServices.AjaxGet('/EmployeeProfile/GetDoc?id=' + docId, callbackFunction);
        };

        this.validDocument = function (docId, callbackFunction) {
            return ajaxServices.AjaxGet('/EmployeeProfile/ValidDocument?id=' + docId, callbackFunction);
        };
        
        this.getNotificationInfo = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet('/EmployeeProfile/GetNotification?id=' + id, callbackFunction);
        };

        this.retrieveStoredRecertification = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/RetrieveStoredRecertification?id=" + id, callbackFunction);
        };

        this.printRecertification = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/PrintRecertification?id=" + id, callbackFunction);
        };

        this.printCheckList = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/PrintCheckList?id=" + id, callbackFunction);
        };

        this.assignJob = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/AssignInspectorJob", callbackFunction);
        };

        this.editTank = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/EditTank", callbackFunction);
        };

        this.retrieveDischargeData = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/RetrieveDischargeData", callbackFunction);
        };

        this.submitDischargeData = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/SubmitDischargeData", callbackFunction);
        };

        this.printCheckList = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/PrintCheckList?id=" + id, callbackFunction);
        };

        this.editEmployeeProfile = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/EditEmployeeProfile", callbackFunction);
        };


        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/GetGenericList", callbackFunction);
        };

        this.printDischargeData = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/PrintDischargeData", callbackFunction);
        };

        this.deleteEmployeeProfile = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/EmployeeProfile/DeleteEmployeeProfile?id=" + id, callbackFunction);
        };

        this.processDialogReview = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/ProcessDialogReview", callbackFunction);
        };

        this.processNotificationDialogApprove = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/ProcessNotificationDialogApprove", callbackFunction);
        };


        this.getHistory = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/GetHistory?id=" + id, callbackFunction);
        };

        this.saveVesselReport = function (employeeProfile, callbackFunction) {
            return ajaxServices.AjaxPost({ employeeProfile: employeeProfile }, "/EmployeeProfile/SaveVesselReport", callbackFunction);
        };
      

        this.retrieveSavedVesselReport = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/EmployeeProfile/RetrieveSavedVesselReport?id=" + id, callbackFunction);
        };
    }]);
});