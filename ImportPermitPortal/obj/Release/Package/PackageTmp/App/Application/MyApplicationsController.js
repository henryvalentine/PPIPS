"use strict";

define(['application-configuration', 'ngDialog','userAppService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngApps', function ($compile)
    {
        return function ($scope, ngApps)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetApplicationObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'DerivedQuantityStr', 'DerivedValue', 'DateAppliedStr', 'StatusStr'];
            var ttc = applicationtableManager($scope, $compile, ngApps, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('myApplicationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userAppService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, userAppService, $upload, fileReader, $http, $location)
    {
        $scope.initializeApp = function ()
        {
            $scope.viewApp = false;
            $scope.application =
            {
                'Id': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ApplicationItemObjects': []
            };

            $scope.application.StatusCode = 0;
        };

        $scope.continueApp = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            $scope.id = appId;
            userAppService.checkAddressAvailability($scope.checkAddressAvailabilityCompleted);
        };

        $scope.checkAddressAvailabilityCompleted = function (response)
        {
            if (response.Code < 1 || response.IsAddressProvided === false)
            {
                $location.path('CompanyAddress/CompanyAddress/' + $scope.id);
            }
            else
            {
                $location.path('ApplicationDetail/BankerDetail/' + $scope.id);
            }
           
        };

        $scope.getAppView = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            userAppService.getAppResult(appId, $scope.getAppViewCompleted);
        };
      
        $scope.getAppViewCompleted = function (data)
        {
            if (data == null || data.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }

            userAppService.setApp(data);
            $location.path('ApplicationDetail/ApplicationDetail');
        };

        $scope.continueAppCompleted = function (data)
        {
            if (data == null || data.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }

            $location.path('ApplicationDetail/BankerDetail/' + data.Id);
        };
        
        $scope.viewDocuments = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            userAppService.setAppId(appId);
            $location.path('/Application/UserDocuments');
        };

        $scope.setAppSuccess = function (msg)
        {
            $scope.isError = false;
            $scope.appError = '';
            $scope.appSuccess = msg;
            $scope.isSuccess = true;
        };

        $scope.setAppError = function (msg)
        {
            $scope.appError = msg;
            $scope.isError = true;
            $scope.isSuccess = false;
            $scope.appSuccess = '';
        };

        $scope.createApp = function()
        {
            $location.path('Application/Application');
        }
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('My Applications|DPR-PPIPS');
            $scope.Apps = [];
            $scope.viewApp = false;
            $scope.application =
            {
                'Id': '',  'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ApplicationItemObjects': []
            };

            $scope.application.StatusCode = 0;
            $rootScope.getReqs();
        };
        
        $scope.getAppEdit = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $location.path('/Application/EditApplication/' + appId);
        };

        $scope.getAppInfo = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            userAppService.getAppForPayment(appId, $scope.getAppInfoCompleted);
        };

        $scope.getAppInfoCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Application information could not be retrieved. Please try again later.');
                return;
            }

            userAppService.setApp(response);
            $location.path('/Application/ApplicationPaymentSummary');
        };
    }]);

});




