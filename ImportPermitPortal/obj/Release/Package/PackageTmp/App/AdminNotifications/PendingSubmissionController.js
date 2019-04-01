"use strict";

define(["application-configuration", "ngDialog","userNotificationService", "angularFileUpload", "fileReader"], function (app)
{
    app.register.directive("ngPNtfs", function ($compile)
    {
        return function ($scope, ngPNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetPaidNotifications";
            tableOptions.itemId = "Id";
            tableOptions.columnHeaders = ["ReferenceCode", "ImporterName", "PermitValue", "ProductName", "AmountDue", "ExpectedQuantityStr", "ArrivalDateStr", "StatusStr"];
            var ttc = adminNotificationTableManager($scope, $compile, ngPNtfs, tableOptions);
            ttc.removeAttr("width").attr("width", "100%");
            $scope.jtable = ttc;
        }; 
    });
  
    app.register.controller("paidNotificationController", ["ngDialog", "$scope", "$rootScope", "$routeParams", "userNotificationService", "$upload", "fileReader", "$http", "$location", "$window",
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $upload, fileReader, $http, $location,$window)
    {
        $scope.initializeApp = function ()
        {
            $rootScope.setPageTitle("Notifications|DPR-PPIPS");
            $scope.viewApp = false;
            $scope.notification =
             {
                 'NotificationId': "",
                 'ReferenceCode': "",
                 'NotificationTypeId': "",
                 'ApplicationQuantity': "",
                 'PermitId': "",
                 'PortOfOriginId': "",
                 'DischargeJettyId': "",
                 'ProductId': "",
                 'ExpectedQuantity': "",
                 'CargoInformationTypeId': "",
                 'ArrivalDate': "",
                 'DischargeDate': "",
                 'AmountDue': "",
                 'DateCreated': ""
             };

        };
          
        $scope.getNotificationDetails = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert("Invalid selection!");
                return;
            }
            userNotificationService.getNotificationInfo(appId, $scope.getNotificationDetailsCompleted);
        };

        $scope.getCompanyInfo = function (id) {
            if (parseInt(id) < 1) {
                alert('An unknown error was encountered. Please try again');
                return;
            }

            $location.path('Importers/Importer/' + id);
        };

        $scope.getNotificationDetailsCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert("Notification information could not be retrieved. Please try again later.");
                return;
            }

            $scope.notification = response;
            $scope.suppliedDocs = [];
            $scope.bnkDocs = [];

            angular.forEach($scope.notification.DocumentTypeObjects, function (n, m)
            {
                if (n.Uploaded === true)
                {
                    $scope.suppliedDocs.push(n);
                }
                else
                {
                   $scope.bnkDocs.push(n);
                }
            });

            $scope.getReqs($scope.notification.ImporterId);
            $scope.viewApp = true;
        };

        $scope.getReqs = function (id)
        {
            userNotificationService.getEligibility(id, $scope.getReqsCompleted);
        };

        $scope.getReqsCompleted = function (response)
        {
            $scope.stadDocs = response;
        };
        
        $scope.printNotification = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert("Invalid selection!");
                return;
            }
            userNotificationService.printNotification(appId, $scope.printNotificationCompleted);
        };

        $scope.printNotificationCompleted = function (response)
        {
            window.open(response.SmallPath);

        };

    }]);

});




