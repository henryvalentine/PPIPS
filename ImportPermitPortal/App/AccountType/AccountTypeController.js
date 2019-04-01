"use strict";

define(['application-configuration', 'accountTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngAccType', function ($compile)
    {
        return function ($scope, ngAccType)
        {
            var tableOptions = {sourceUrl : "/AccountType/GetAccountTypeObjects", itemId : 'AccountTypeId'};
          
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngAccType, tableOptions, 'Add Account Type', 'prepareAccTypeTemplate', 'getAccType', 'deleteAccType', 155);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
   
    app.register.controller('accountTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'accountTypeService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, accountTypeService, $upload, fileReader, $location)
    {

        $scope.prepareAccTypeTemplate = function ()
        {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/AccountType/ProcessAccountType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getAccType = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            accountTypeService.getItem("/AccountType/GetAccountType?id=" + impId, $scope.getAccTypeCompleted);
        };

        $scope.getAccTypeCompleted = function (response)
        {
            if (response.AccountTypeId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.accountType = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.accountType.Header = 'Update Account Type';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/AccountType/ProcessAccountType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processAccType = function ()
        {
            if ($scope.accountType == null || $scope.accountType.Name.length < 1)
            {
                alert('Please provide Account Type Name');
                return;
            }
            
            if ($scope.accountType.AccountTypeId < 1)
            {
                accountTypeService.addAccountType($scope.accountType, $scope.processAccTypeCompleted);
            }
            else
            {
                accountTypeService.editAccountType($scope.accountType, $scope.processAccTypeCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Account Types');
            $scope.accountType =
           {
               'AccountTypeId': 0, 'Name': '', 'Description' : ''
           };
            $scope.accountType.Header = "New Account Type";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processAccTypeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/AccountType/ProcessAccountType.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteAccType = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This  Account Type will be deleted permanently. Continue?"))
                {
                    return;
                }
                accountTypeService.deleteAccountType(id, $scope.deleteAccTypeCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteAccTypeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else
            {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };
        
    }]);
  
});




