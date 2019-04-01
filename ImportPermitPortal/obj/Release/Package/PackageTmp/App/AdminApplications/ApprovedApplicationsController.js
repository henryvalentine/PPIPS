"use strict";

define(['application-configuration', 'adminApplicationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngApps', function ($compile)
    {
        return function ($scope, ngApps)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetApprovedApplications";
            tableOptions.itemId = 'ApplicationId';
            tableOptions.columnHeaders = ['ReferenceCode', 'ImporterStr', 'DerivedQuantityStr', 'DerivedValue', 'DateAppliedStr', 'StatusStr'];
            var ttc = adminApplicationtableManager($scope, $compile, ngApps, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('adminApplicationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'adminApplicationService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http,adminApplicationService, $location)
    {
        $scope.initializeApp = function ()
        {
            $scope.viewApp = false;
            $scope.application =
            {
                'ApplicationId': '', 'Id': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ApplicationItemObjects': []
            };

        };
        
     
        $scope.getAppView = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            adminApplicationService.getAppDetails(appId, $scope.getAppViewCompleted);

        };

        $scope.getAppViewCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Application Information could not be retrieved.');
                return;
            }

            $scope.initializeApp();
            
            $scope.application = response;
            if ($scope.application.SignOffDocumentPath !== null && $scope.application.SignOffDocumentPath.length > 0) {
                $scope.virifierDocumentUploaded = true;
            }
            $scope.suppliedDocs = [];
            $scope.bnkDocs = [];
            $scope.throughPuts = [];
            $scope.nextDocs = [];
            $scope.unsuppliedthroughPuts = [];

            angular.forEach($scope.application.DocumentTypeObjects, function (n, m)
            {
                if (n.Uploaded === true)
                {
                    $scope.suppliedDocs.push(n);
                }
                else
                {
                    if (n.StageId === 1 || n.IsDepotDoc === true)
                    {
                        $scope.bnkDocs.push(n);
                    }
                    else {
                        $scope.nextDocs.push(n);
                    }
                }
            });

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
            {
                if (g.ThroughPutObjects !== null && g.ThroughPutObjects.length > 0)
                {

                    angular.forEach(g.ThroughPutObjects, function (y, p)
                    {
                        if (y.DocumentId !== null && y.DocumentId > 0)
                        {
                            y.IsUploaded = true;
                            $scope.throughPuts.push(y);
                        }

                        else {
                            if (g.StorageProviderTypeId !== 1) {
                                y.IsUploaded = false;
                                $scope.unsuppliedthroughPuts.push(y);
                            }

                        }
                    });
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
            $scope.getReqs($scope.application.ImporterId);
            $scope.viewApp = true;
            
        };

        $scope.getReqs = function (id)
        {
            adminApplicationService.getEligibility(id, $scope.getReqsCompleted);

        };

        $scope.getReqsCompleted = function (response)
        {
            $scope.stadDocs = response;
            adminApplicationService.getAppProcesses($scope.appId, $scope.getAppProcessCompleted);
        };
        
        $scope.getAppProcessCompleted = function (data) {
            $scope.hasTrack = false;
            if (data.ProcessTrackingObjects !== null && data.ProcessTrackingObjects.length > 0) {
                $scope.currentTrack = data.ProcessTrackingObjects[0];
                $scope.hasTrack = true;
            }
            $scope.hasHistory = false;
            if (data.ProcessingHistoryObjects !== null && data.ProcessingHistoryObjects.length > 0) {
                $scope.histories = data.ProcessingHistoryObjects;
                $scope.hasHistory = true;
            }

        };
         
        $scope.viewAppIssues = function ()
        {
            ngDialog.open({
                template: '/App/AdminApplications/AppIssues.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
            angular.element('.ngdialog-content').addClass('modalWidth');
    };

        $scope.viewProcessingHistory = function ()
        {
            ngDialog.open({
                template: '/App/AdminApplications/PrecessingHistory.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
            angular.element('.ngdialog-content').addClass('modalWidth');
        };
        
        $scope.closIssueView = function ()
        {
            ngDialog.close('/App/AdminApplications/AppIssues.html', '');
        };

        $scope.closHistoryView = function (){
            ngDialog.close('/App/AdminApplications/PrecessingHistory.html', '');
        };


        $scope.getCompanyInfo = function (id) {
            if (parseInt(id) < 1) {
                alert('An unknown error was encountered. Please try again');
                return;
            }

            $location.path('Importers/Importer/' + id);
        };

        $scope.viewDocuments = function (appIdId)
        {
            if (parseInt(appIdId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            $http({ method: 'GET', url: '/Document/SetDocSession?id=' + appIdId }).success(function (response)
            {
                $scope.viewDocumentsCompleted(response);
            });
        };

        $scope.viewDocumentsCompleted = function (code)
        {
            if (parseInt(code) < 1)
            {
                alert('An unknown error was encountered. Please try again');
                return;
            }

            $location.path('/AppDocs/AppDocs');
        };

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Applications|DPR-PPIPS');
            $scope.viewApp = false;
            $scope.application =
               {
                   'Id': '', 'ImporterId': '', 'PaymentTypeId': '', 'DerivedTotalQUantity': '', 'BankId': '', 'StorageProviderTypeId': '', 'InvoiceId': '', 'ClassificationId': '',
                   'DerivedValue': '', 'ApplicationStatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '', 'PreviousPermitNo': '', 'ApplicationTypeId': '',
                   'Header': 'New Application', 'ApplicationItemObjects': [], 'BankObject': { 'BankId': '', 'SortCode': '', 'ImporterId': '', 'Name': '-- Select Bank --' },
                   'StorageProviderTypeObject': { 'Id': '', 'Name': '--Select Storage Provider Type --' }, 'ImportClass': { 'Id': '', 'Name': '--Select Import Category--' }
               };

        };
        
    }]);

});




