"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngBankUsers', function ($compile)
    {
        return function ($scope, ngBankUsers) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Bank/GetBankUsers";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['LastName', 'FirstName', 'Email', 'PhoneNumber', 'StatusStr'];
            var ttc = bankUserTableManager($scope, $compile, ngBankUsers, tableOptions, 'Single Entry', 'prepareUserTemplate', 'getBankUser', 122, 'changePassword');
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });


    app.register.controller('bnkUsrMgrController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$upload', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $upload, $location)
    {

        $scope.roles = [{ 'Name': 'Bank Admin', 'Id': 1 }, { 'Name': 'Standard User', 'Id': 5 }];
       
        $scope.bulkUpload = function () {
            ngDialog.open({
                template: '/App/Users/ProcessBulkUserBranch.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        /* Manage Bank User*/
        $scope.initializeModel = function ()
        {
            $scope.edit = false;
            $scope.add = true;
            $scope.user =
            {
                'FirstName': '',
                'LastName': '',
                'Email': '',
                'PhoneNumber': '',
                'Password': '',
                'ConfirmPassword': '',
                'BankId': '',
                'Id': '',
                'IsActive': true,
                'RoleId': '',
                'RoleObject' : { 'Id': '', 'Name': '-- Select Role --' },
                'BranchObject' : { 'Id': '', 'Name': '-- Select Branch --' }
            };
        };

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Users|DPR-PPIPS');
            bnkAdminService.getBranches($scope.getBranchesCompleted);
        };

        //	GetBankBranches
        
        $scope.getBranchesCompleted = function (branches)
        {
            $scope.branches = branches;
        };

        $scope.getBankUser = function (userId)
        {
            if (userId == null || userId.length < 1)
            {
                alert('Invalid selection!');
                return;
            }

            bnkAdminService.getUser(userId, $scope.getBankUserCompleted);
        };
  
        $scope.prepareUserTemplate = function ()
        {
            $scope.initializeModel();
            $scope.edit = false;
            $scope.add = true;
             $scope.user.Header = 'Create User';
            $scope.buttonText = "Register";
            ngDialog.open({
                template: '/App/Users/ProcessUsers.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };
        
        $scope.getBankUserCompleted = function (response)
        {
            $scope.initializeModel();
           
            $scope.user = response;
            angular.forEach($scope.roles, function (r, i)
            {
                if (r.Id === $scope.user.RoleId)
                {
                    $scope.user.RoleObject = r;
                }
            });
             
            angular.forEach($scope.branches, function (r, i) {
                if (r.Id === $scope.user.BranchId) {
                    $scope.user.BranchObject = r;
                }
            });

            $scope.edit = true;
            $scope.add = false;
            $scope.user.Header = 'Update User Information';
            $scope.buttonText = "Update";
               
            ngDialog.open({
                template: '/App/Users/ProcessUsers.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };
        
        $scope.processUser = function ()
        {
            if ($scope.user < 1) {
                alert('Error: Please refresh the page and try again!');
                return;
            }

            if ($scope.user.FirstName == null || $scope.user.FirstName.length < 1)
            {
                alert('Please provide Other Names.');
                return;
            }

            if ($scope.user.LastName == null || $scope.user.LastName.length < 1) {
                alert('Please provide Last Name.');
                return;
            }

            if ($scope.user.Email == null || $scope.user.Email.length < 1) {
                alert('Please provide Email.');
                return;
            }

            if ($scope.user.RoleObject == null || $scope.user.RoleObject.Id < 1)
            {
                alert('Please select a role.');
                return;
            }

            if ($scope.user.BranchObject == null || $scope.user.BranchObject.Id < 1)
            {
                alert('Please select a Branch.');
                return;
            }

            var user =
             {
                 'FirstName': $scope.user.FirstName,
                 'LastName': $scope.user.LastName,
                 'Email': $scope.user.Email,
                 'PhoneNumber': $scope.user.PhoneNumber,
                 'BankId': $scope.user.BankId,
                 'Id': $scope.user.Id,
                 'IsActive': $scope.user.IsActive,
                 'RoleId': $scope.user.RoleObject.Id,
                 'BranchId': $scope.user.BranchObject.Id
             };
            

           if (!$scope.validateUser(user))
           {
               return;
           }
           if ($scope.edit)
           {
               bnkAdminService.editUser(user, $scope.processUserCompleted);
               
           }
           else
           {
               bnkAdminService.addUser(user, $scope.processUserCompleted);
           }
           
        }

        $scope.processUserCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
                return;
            }
            
            alert(data.Error);
            ngDialog.close('/App/Users/ProcessUsers.html', '');
            $scope.initializeModel();
            $scope.jtable.fnClearTable();
        }

        $scope.validateUser = function(user) {
            if (user.FirstName == null || user.FirstName.length < 1) {
                alert('Please provide User First Name');
                return false;
            }

            if (user.LastName == null || user.LastName.length < 1) {
                alert('Please provide User Last Name');
                return false;
            }

            if (user.Email == null || user.Email.length < 1) {
                alert('Please provide User Email');
                return false;
            }

            if (user.PhoneNumber == null || user.PhoneNumber.length < 1) {
                alert('Please provide User Phone Number');
                return false;
            }

            return true;
        };

        $scope.validatePassword = function (user)
        {
           
            if (user.Password == null || user.Password.length < 1) {
                alert('Please provide User Password');
                return false;
            }

            if (user.ConfirmPassword == null || user.ConfirmPassword.length < 1) {
                alert('Please confirm User Password');
                return false;
            }

            if (user.ConfirmPassword.length < 8 || user.Password.length < 8) {
                alert('Both passwords should not be less than 8 alphanumeric characters');
                return false;
            }

            if (user.ConfirmPassword !== user.Password) {
                alert('The passwords do not match');
                return false;
            }
            return true;
        };


        //Update User's Password

        $scope.changePassword = function (id)
        {
            if (id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.UserId = id;
            ngDialog.open({
                template: '/App/Users/ChangePassword.html',
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
            ngDialog.close('/App/Users/ChangePassword.html', '');
        };

        //Bulk User Upload

        $scope.ProcessBankDoc = function (e) {
            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];

            $scope.processing = true;
             $rootScope.busy = true;
            $upload.upload({
                url: '/BankUserBulk/BulkUpload',
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt) {

           }).success(function (data) {
               $scope.processing = false;
               $rootScope.busy = false;
               if (data.Code < 1) {
                   if (data.Code === -7) {
                       $scope.bMsg = data.Error;
                       $scope.bulkusers = data.BankUserInfoList;

                       ngDialog.open({
                           template: '/App/Users/BulkUploadFeedback.html',
                           className: 'ngdialog-theme-flat',
                           scope: $scope
                       });
                   } else {
                       alert(data.Error);
                   }
               } else {

                   $scope.bMsg = data.Error;
                   $scope.bulkusers = data.BankUserInfoList;

                   ngDialog.open({
                       template: '/App/Users/BulkUploadFeedback.html',
                       className: 'ngdialog-theme-flat',
                       scope: $scope
                   });
                   $scope.jtable.fnClearTable();
               }
           });
        }

        $scope.cls = function () {
            ngDialog.close('/App/Users/BulkUploadFeedback.html', '');
        };

    }]);

});




