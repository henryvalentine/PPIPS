"use strict";

define(['application-configuration', 'bankService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngBank', function ($compile)
    {
        return function ($scope, ngBank)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Bank/GetBankObjects";
            tableOptions.itemId = 'BankId';
            tableOptions.columnHeaders = ['Name', 'SortCode', 'NotificationEmail','LastName'];
            var ttc = bankTableManager($scope, $compile, ngBank, tableOptions, 'Add Bank', 'prepareBankTemplate', 'getBank', 'getBankUser', 100);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
     
    app.register.controller('bankController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bankService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, bankService, $location)
    {
        $scope.prepareBankTemplate = function ()
        {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/Banks/ProcessBank.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getBank = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            bankService.getBank(impId, $scope.getBankCompleted);
        };

        $scope.getBankCompleted = function (response)
        {
            if (response.BankId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.bank = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.bank.Header = 'Update Bank Information';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Banks/ProcessBank.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processBank = function ()
        {
            if ($scope.bank == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.bank.SortCode == null || $scope.bank.SortCode.length < 1)
            {
                alert('Please provide Bank Sort Code.');
                return;
            }

            if ($scope.bank.Name == null || $scope.bank.Name.length < 1)
            {
                alert('Please provide Bank Name.');
                return;
            }

           var bank =
              {
                  'BankId': $scope.bank.BankId,
                  'SortCode': $scope.bank.SortCode,
                  'Name': $scope.bank.Name,
                  'StructureId': 3,
                  'DateRegistered': '',
                  'RCNumber': $scope.bank.RCNumber,
                  'NotificationEmail': $scope.bank.NotificationEmail,
                  'TIN': $scope.bank.TIN,
                  'Id': $scope.bank.Id,
                  'ContactPersonId': $scope.bank.ContactPersonId
              };
            
            if ($scope.add)
            {
                bankService.addBank(bank, $scope.processBankCompleted);
            }
            else
            {
                bankService.editBank(bank, $scope.processBankCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Banks');
            $scope.bank =
               {
                   'BankId': '',
                   'SortCode': '',
                   'Name': '',
                   'StructureId': 3,
                   'DateRegistered': '',
                   'RCNumber': '',
                   'TIN': '',
                   'Id': '',
                   'ContactPersonId': ''
               };
            
              $scope.user =
              {
                  'BankId': '',
                  'ImporterId': '',
                  'Id' : '',
                  'FirstName': '',
                  'LastName': '',
                  'Email': '',
                  'PhoneNumber': '',
                  'IsActive': true,
                  'BankBranchName': '',
                  'BranchCode' : ''
              };

            $scope.add = true;
            $scope.edit = false;
            $scope.bank.Header = 'Add Bank';
            $scope.buttonText = "Add";

        };
        
        $scope.processBankCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Banks/ProcessBank.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteBank = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                bankService.deleteBank(id, $scope.deleteBankCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteBankCompleted = function (data)
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

        /* Manage Bank User*/

        $scope.getBankUser = function (bankId)
        {
            if (parseInt(bankId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            bankService.getBankAdmin(bankId, $scope.getBankUserCompleted);
        };

        $scope.getBankUserCompleted = function (response)
        {
            $scope.initializeController();
            
            if (response.UserId == null || response.UserId.length < 1 || response.Id === 0)
            {
                $scope.edit = false;
                $scope.add = true;
                $scope.user.Header = 'Add Bank Admin/Contact';
                $scope.buttonText = "Add";

                $scope.user.BankId = response.BankId;
                $scope.user.ImporterId = response.CompanyId;
                $scope.user.Id = response.Id;

            } else {
                $scope.user = response;
                $scope.user.BankId = response.BankId;
                $scope.user.ImporterId = response.CompanyId;
                $scope.edit = true;
                $scope.add = false;
                $scope.user.Header = 'Update Bank Admin/Contact Information';
                $scope.buttonText = "Update";
               
            }
            ngDialog.open({
                template: '/App/Banks/ProcessBankUsers.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };
        
        $scope.processBankUser = function ()
        {
            var user =
            {
                'FirstName': $scope.user.FirstName,
                'LastName': $scope.user.LastName,
                'Email': $scope.user.Email,
                'PhoneNumber': $scope.user.PhoneNumber,
                'BankId': $scope.user.BankId,
                'ImporterId': $scope.user.ImporterId,
                'Id': $scope.user.Id,
                'IsActive': $scope.user.IsActive,
                'BankBranchName': $scope.user.BankBranchName,
                'BranchCode': $scope.user.BranchCode
            };

           if (!$scope.validateBankUser(user))
           {
               return;
           }

           if ($scope.edit)
           {
               bankService.UpdateBankAdmin(user, $scope.processBankUserCompleted);
           }
           else
           {
               bankService.addBankAdmin(user, $scope.processBankUserCompleted);
           }
           
        }

        $scope.processBankUserCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
                return;
            }
            
            alert(data.Error);
            ngDialog.close('/App/Banks/ProcessBankUsers.html', '');
            $scope.initializeController();
            $scope.jtable.fnClearTable();
        }

        $scope.validateBankUser = function (user)
        {
            if (user.FirstName == null || user.FirstName.length < 1)
            {
                alert('Please provide User First Name');
                return false;
            }

            if (user.LastName == null || user.LastName.length < 1) {
                alert('Please provide User Last Name');
                return false;
            }
            if ($scope.user.BankId == null || $scope.user.BankId < 1) {
                alert('An error was encountered on the page. Please refresh the page and try again.');
                return false;
            }
            if (user.Email == null || user.Email.length < 1) {
                alert('Please provide User Email');
                return false;
            }
            if (user.BankBranchName == null || user.BankBranchName.length < 1) {
                alert('Please provide Bank Branch Name');
                return false;
            }
            if (user.BranchCode == null || user.BranchCode.length < 1) {
                alert('Please provide Bank Branch Code');
                return false;
            }
           
            if (user.PhoneNumber == null || user.PhoneNumber.length < 1) {
                alert('Please provide User Phone Number');
                return false;
            }

            return true;
        };

        $scope.validatePassword = function (user) {

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

    }]);

});




