"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.directive('ngBranches', function ($compile)
    {
        return function ($scope, ngBranches) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Bank/GetBankBranchObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'BranchCode'];
            var ttc = bankBranchTableManager($scope, $compile, ngBranches, tableOptions, 'Single Entry', 'prepareBranchTemplate', 'getBankBranch', 122);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.branchtable = ttc;
        };
    });
    
    
    app.register.controller('bnkBranchesController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$location', '$upload',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $location, $upload)
    {
        
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Branches|DPR-PPIPS');
            $scope.initializeBranch();
        };

        $scope.bulkUpload = function ()
        {
            ngDialog.open({
                template: '/App/BnkAdmin/ProcessBulkBranch.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        //Branch Management
        //
        $scope.getBankBranch = function (branchId)
        {
            if (parseInt(branchId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            bnkAdminService.getBankBranch(branchId, $scope.getBankBranchCompleted);
        };

        $scope.getBankBranchCompleted = function (response)
        {
            if (response.BankId < 1)
            {
                alert('Branch information could not be retrieved!');
                return;
            }

            $scope.initializeBranch();

            $scope.bankBranch = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.bankBranch.Header = 'Update Bank Branch Information';
            $scope.branchButtonText = "Update";
            ngDialog.open({
                template: '/App/BnkAdmin/ProcessBranch.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processBankBranch = function ()
        {
            if ($scope.bankBranch == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.bankBranch.BranchCode == null || $scope.bankBranch.BranchCode.length < 1)
            {
                alert('Please provide Branch Code.');
                return;
            }

            if ($scope.bankBranch.Name == null || $scope.bankBranch.Name.length < 1)
            {
                alert('Please provide Branch Name.');
                return;
            }
            
            var bankBranch =
            {
                'BankId': $scope.bankBranch.BankId,
                'BranchCode': $scope.bankBranch.BranchCode,
                'Name': $scope.bankBranch.Name
            };

            if ($scope.add || $scope.bankBranch.Id < 1)
            {
                bnkAdminService.addBankBranch(bankBranch, $scope.processBankBranchCompleted);
            }
            else {
                bnkAdminService.editBankBranch(bankBranch, $scope.processBankBranchCompleted);
            }

        }; 

        $scope.processBankBranchCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else
            {
                alert(data.Error);
                ngDialog.close('/App/BnkAdmin/ProcessBranch.html', '');
                $scope.branchtable.fnClearTable();
                $scope.initializeBranch();
            }
        };

        $scope.processBankBranches = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];

            $scope.processing = true;
            $rootScope.busy = true;
            $upload.upload({
                url: '/BankBranchBulk/BulkUpload',
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt)
           {

           }).success(function (data)
           {
               $scope.processing = false;
               $rootScope.busy = false;
               if (data.Code < 1)
               {
                   alert(data.Error);
                   return;
               }

               alert(data.Error);
               $scope.branchtable.fnClearTable();
           });
        }

        $scope.prepareBranchTemplate = function ()
        {
            $scope.initializeBranch();
            ngDialog.open({
                template: '/App/BnkAdmin/ProcessBranch.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.initializeBranch = function ()
        {
            $scope.bankBranch = { 'Name': '', 'BranchCode': '' };
            $scope.bankBranch.Header = 'Add Bank Branch';
            $scope.branchButtonText = "Add";
            $scope.edit = false;
            $scope.add = true;
        };
    }]);

});




