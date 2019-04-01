"use strict";

define(['application-configuration', 'employeeDeskService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngEmployee', function ($compile)
    {
        return function ($scope, ngEmployee)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/EmployeeDesk/GetEmployeeDeskObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['EmployeeName', 'GroupName', 'ActivityTypeName', 'ZoneName', 'JobCountStr'];
            var ttc = employeeDeskTableManager($scope, $compile, ngEmployee, tableOptions, 'Add Employee', 'prepareEmployeeDeskTemplate', 'getEmployeeDesk', 'deleteEmployeeDesk', 130, 'changePassword');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('employeeDeskController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'employeeDeskService', '$route', '$window', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, employeeDeskService, $route, $window, $location) {

        $scope.prepareEmployeeDeskTemplate = function () {
            $scope.initialize();
            ngDialog.open({
                template: '/App/EmployeeDesk/ProcessEmployeeDesk.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getEmployeeDesk = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            employeeDeskService.getEmployeeDesk(impId, $scope.getEmployeeDeskCompleted);
        };

        $scope.getEmployeeDeskCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initialize();

            $scope.employeeDesk = response;

            $scope.employeeDesk =
{
    'Id': response.Id,
    'FirstName': response.FirstName,
    'LastName': response.LastName,
    'Email': response.Email,
    'Phone': response.Phone,
    'Group': { 'Id': response.GroupId, 'Name': response.Group },
    'StepActivityType': { 'Id': response.ActivityTypeId, 'Name': response.ActivityTypeName, 'Description': response.StepDescription },
    'Zone': { 'Id': response.ZoneId, 'Name': response.Zone }

};
            $scope.edit = true;
            $scope.add = false;

            $scope.employeeDesk.Header = 'Update Employee';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/EmployeeDesk/ProcessEmployeeDesk.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processEmployeeDesk = function () {
            if ($scope.employeeDesk == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

         
          var req =
         {
             'FirstName': $scope.employeeDesk.FirstName,
             'LastName': $scope.employeeDesk.LastName,
             'Email': $scope.employeeDesk.Email,
             'GroupId': $scope.employeeDesk.Group.Id,
             'ActivityTypeId': $scope.employeeDesk.Type.Id,
             'ZoneId': $scope.employeeDesk.Zone.Id
         };

            if ($scope.add) {
                employeeDeskService.addEmployeeDesk(req, $scope.processEmployeeDeskCompleted);
            }
           
            else {
                employeeDeskService.editEmployeeDesk(req, $scope.processEmployeeDeskCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Employees');
            employeeDeskService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {
            
            $scope.groups = data.Groups;
            $scope.zones = data.Zones;
            $scope.activitytypes = data.ActivityTypes;
           

        };

        $scope.initialize = function () {
            $scope.employeeDesk =
            {
                'Id': '',
                'FirstName': '',
                'LastName': '',
                'Email': '',
                'Phone': '',
                'Group': { 'Id': '', 'Name': '-- Select Group --' },
                'ActivityTypeName': { 'Id': '', 'Name': '-- Select Activity Type --' },
                'ZoneName': { 'Id': '', 'Name': '-- Select Zone --' }
               
                
            };
            $scope.add = true;
            $scope.edit = false;
            $scope.employeeDesk.Header = 'Add Employee';
            $scope.buttonText = "Add";
        };

        $scope.processEmployeeDeskCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }
            else if (response.IsMailSent === true) {
               
                //$scope.initializeApp();
                $scope.initialize();
                alert('User was successfully Registered. An email has been sent to notify the user.');
                ngDialog.close('/App/EmployeeDesk /ProcessEmployeeDesk.html', '');         
                $route.reload();
                $window.location.reload();
                           
            }

            else if (response.IsMailSent === false) {

                //$scope.initializeApp();

                alert('User Registeration was successfully completed, but a notification email could not be sent to the user. Please pass the following information to the user');
                ngDialog.close('/App/EmployeeDesk /ProcessEmployeeDesk.html', '');
                $route.reload();
                $window.location.reload();              
            }

            else if (response.IsUserRegistered === false) {

                //$scope.initializeApp();

                alert('User Registeration failed');
                ngDialog.close('/App/EmployeeDesk /ProcessEmployeeDesk.html', '');
                $route.reload();
                $window.location.reload();
            }
        };


        $scope.deleteEmployeeDesk = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                employeeDeskService.deleteEmployeeDesk(id, $scope.deleteEmployeeDeskCompleted);

            }
           
            else {
                alert('Invalid selection.');
            }
        };


        $scope.deleteEmployeeDeskCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);

            }
            else {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };
        

        //Update User's Password

        $scope.changePassword = function (id)
        {
            if (id < 1)
            {
                alert('Invalid selection!');
                return;
            }

            $scope.UserId = id;
            ngDialog.open({
                template: '/App/EmployeeDesk/ChangePassword.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.changePss = function ()
        {
            if ($scope.UserId  < 1)
            {
                alert('Error: Please refresh the page and try again!');
                return;
            }

            if ($scope.pss.NewPassword == null || $scope.pss.NewPassword.length < 1)
            {
                alert('Please provide new Password.');
                return;
            }

            if ($scope.pss.ConfirmPassword == null || $scope.pss.ConfirmPassword.length < 1) {
                alert('Please confirm new Password.');
                return;
            }

            if ($scope.pss.ConfirmPassword.length < 8) {
                alert('The password should be at least 8 characters in length.');
                return;
            }

            if ($scope.pss.NewPassword.length !== $scope.pss.ConfirmPassword.length) {
                alert('The Passwords do not match.');
                return;
            }

            $scope.pss.UserId = $scope.UserId;
            $rootScope.AjaxPost2({ model: $scope.pss }, "/Account/ChangePasswordFromProfile", $scope.changePssCompleted);
        };

        $scope.changePssCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
                return;
            }
            alert(data.Error);
            ngDialog.close('/App/EmployeeDesk/ChangePassword.html', '');
        };

    }]);

});




