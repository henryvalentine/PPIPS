"use strict";

define(['application-configuration', 'appUserService', 'ngDialog'], function (app) {
    app.register.directive('ngUser', function ($compile) {
        return function ($scope, ngUser) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Account/GetAppUsers";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'Email', 'PhoneNumber', 'Role', 'StatusStr'];
            var ttc = appUsertableManager($scope, $compile, ngUser, tableOptions, 'Add User', 'prepareUserTemplate', 'getUser', 95, 'changePassword');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('appUserController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'appUserService',
    function (ngDialog, $scope, $rootScope, $routeParams, appUserService)
    {
        $scope.prepareUserTemplate = function ()
        {
            $scope.initialize();
            ngDialog.open({
                template: '/App/AppUsers/ProcessAppUser.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getUser = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            appUserService.getAppUser(impId, $scope.getAppUserCompleted);
        };

        $scope.getAppUserCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert(response.Error);
                return;
            }

            $scope.initialize();

            $scope.appUser = response;

            $scope.appUser.RoleObject = { 'Id': '', 'Name': '-- Select Role --' };
            
            angular.forEach($scope.roles, function (r, i)
            {
                if (r.Name ===  $scope.appUser.Role)
                {
                    $scope.appUser.RoleObject = r;
                }
            });

            $scope.edit = true;
            $scope.add = false;

            $scope.appUser.Header = 'Update User';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/AppUsers/ProcessAppUser.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processAppUser = function ()
        {
            if ($scope.appUser == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.appUser.FirstName == null || $scope.appUser.FirstName.length < 1)
            {
                alert('Please provide First Name.');
                return;
            }

            if ($scope.appUser.LastName == null || $scope.appUser.LastName.length < 1)
            {
                alert('Please provide Last Name.');
                return;
            }

            if ($scope.appUser.PhoneNumber == null || $scope.appUser.PhoneNumber.length < 1) {
                alert('Please provide Phone Number.');
                return;
            }

            if ($scope.appUser.RoleObject.Id == null || $scope.appUser.RoleObject.Id.length < 1) {
                alert('Please select user role.');
                return;
            }

            if ($scope.appUser.Email == null || $scope.appUser.Email.length < 1)
            {
                alert('Please provide Email.');
                return;
            }

            var  appUser =
            {
                'Id': $scope.appUser.Id,
                'FirstName':  $scope.appUser.FirstName,
                'LastName':  $scope.appUser.LastName,
                'Email': $scope.appUser.Email,
                'IsActive' : $scope.appUser.IsActive,
                'PhoneNumber':  $scope.appUser.PhoneNumber,
                'Role' :  $scope.appUser.RoleObject.Name
            };

          if ($scope.add) {
              appUser.IsActive = true;
              appUserService.addAppUser(appUser, $scope.processAppUserCompleted);
          }
           
          else
          {
              appUserService.editAppUser(appUser, $scope.processAppUserCompleted);
          }

        };

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Users|DPR-PPIPS');
            $scope.initialize();
            appUserService.getRoles($scope.getRolesCompleted);
        };

        $scope.getRolesCompleted = function (data)
        {
            $scope.roles = data;
        };

        $scope.initialize = function ()
        {
            $scope.appUser =
            {
                'Id': '',
                'FirstName': '',
                'LastName': '',
                'Email': '',
                'PhoneNumber': '',
                'RoleObject': { 'Id': '', 'Name': '-- Select Role --' }
            };

            $scope.add = true;
            $scope.edit = false;
            $scope.appUser.Header = 'Add User';
            $scope.buttonText = "Add";
        };

        $scope.processAppUserCompleted = function (response)
        {
            if (response.Code < 1)
            {
                alert(response.Error);
                return;
            }
            alert(response.Error);
            ngDialog.close('/App/AppUsers/ProcessAppUser.html', '');
            $scope.initializeController();
            $scope.jtable.fnClearTable();
        };
       

        //Update User's Password

        $scope.changePassword = function (id) {
            if (id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.UserId = id;
            ngDialog.open({
                template: '/App/AppUsers/ChangePassword.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.changePss = function () {
            if ($scope.UserId < 1) {
                alert('Error: Please refresh the page and try again!');
                return;
            }

            if ($scope.pss.NewPassword == null || $scope.pss.NewPassword.length < 1) {
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
            ngDialog.close('/App/AppUsers/ChangePassword.html', '');
        };
    }]);

});




