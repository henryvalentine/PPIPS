"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngBankApps', function ($compile)
    {
        return function ($scope, ngBankApps)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetBankAssignedApplications";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'ImporterStr', 'AppTypeStr', 'ImportClassName', 'DerivedQuantityStr', 'LastModifiedStr', 'StatusStr'];
            var ttc = bankAdminApplicationtableManager($scope, $compile, ngBankApps, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });
    
    app.register.directive('ngBranch', function ($compile)
    {
        return function ($scope, ngBranch) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Bank/GetBankBranchObjects";
            tableOptions.itemId = 'BankId';
            tableOptions.columnHeaders = ['Name', 'BranchCode'];
            var ttc = bankBranchTableManager($scope, $compile, ngBranch, tableOptions, 'Add Branch', 'prepareBranchTemplate', 'getBankBranch', 100);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.branchtable = ttc;
        };
    });
    
    
    app.register.controller('bnkAdminController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$location', '$upload',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $location, $upload)
    {
        $scope.getAppDetail = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            bnkAdminService.getAssignedApp(appId, $scope.getAppViewCompleted);
        };
        
        $scope.getAppViewCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Application Information could not be retrieved.');
                return;
            }

            $scope.initializeController();
            $scope.application = response;
            $scope.suppliedDocs = [];
            $scope.bnkDocs = [];
            $scope.throughPuts = [];
            $scope.nextDocs = [];
            $scope.unsuppliedthroughPuts = [];

            angular.forEach($scope.application.DocumentTypeObjects, function (n, m)
            {
                if (n.IsBankDoc === true)
                {
                    if (n.Uploaded === true)
                    {
                        $scope.suppliedDocs.push(n);
                    }
                    else
                    {
                        $scope.bnkDocs.push(n);
                    }
                }
            });
           
            $scope.refLetters = [];
            $scope.unsuppliedRefLetters = [];

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
            {
                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0)
                {
                    angular.forEach(g.ProductBankerObjects, function (y, p)
                    {
                        if (y.DocumentId !== null && y.DocumentId > 0)
                        {
                            y.IsUploaded = true;
                            y.DocumentPath = y.DocumentPath.replace("~", "");
                            $scope.refLetters.push(y);
                        }
                        else
                        {
                            y.IsUploaded = false;
                            $scope.unsuppliedRefLetters.push(y);
                        }
                    });
                }

            });

            $scope.appId = $scope.application.Id;
            $scope.viewApp = true;
            
        };
        

        $scope.getAppDocuments = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            bnkAdminService.setRoleInfo(1);
            bnkAdminService.setId(appId);
            $location.path('/Application/UserDocuments');
        };

        $scope.viewDocumentsCompleted = function (code)
        {
            if (parseInt(code) < 1)
            {
                alert('An unknown error was encountered. Please try again');
                return;
            }
            $location.path('/Application/UserDocuments');
        };

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Applications|DPR-PPIPS');
            $scope.viewApp = false;
            $scope.viewAppDetails = false;
            $scope.application =
            {
                'Id': '', 'ImporterId': '', 'PaymentTypeId': '', 'PermitId': 0, 'DerivedTotalQUantity': '', 'BankId': '', 'InvoiceId': '', 'ClassificationId': '',
                'DerivedValue': '', 'ApplicationStatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '', 'PreviousPermitNo': '', 'ApplicationTypeId': '',
                'Header': 'New Application', 'ApplicationItemObjects': [], 'BankObject': { 'BankId': '', 'SortCode': '', 'ImporterId': '', 'Name': '-- Select Bank --' },
                'ApplicationType': { 'Id': '', 'Name': '--Select Application Type --' }, 'ImportClass': { 'Id': '', 'Name': '--Select Import Category--' }
            };
           
        };

        
    }]);

});




