"use strict";

define(['application-configuration', 'ngDialog','userNotificationService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngRNtfs', function ($compile)
    {
        return function ($scope, ngRNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetRejectedNotifications";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ImporterName', 'ReferenceCode', 'PermitValue', 'ProductName', 'AmountDue', 'ExpectedQuantityStr', 'ArrivalDateStr', 'StatusStr'];
            var ttc = adminNotificationTableManager($scope, $compile, ngRNtfs, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
           
        }; 
    });
  
    app.register.controller('rejectedNotificationsController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$upload', 'fileReader', '$http', '$location', '$window',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $upload, fileReader, $http, $location,$window)
    {

      
        $scope.initializeApp = function ()
        {
            $rootScope.setPageTitle('Rejected Notifications|DPR-PPIPS');
            $scope.viewApp = false;
            $scope.notification =
             {
                 'NotificationId': '',
                 'ReferenceCode': '',
                 'NotificationTypeId': '',
                 'ApplicationQuantity': '',
                 'PermitId': '',
                 'PortOfOriginId': '',
                 'DischargeJettyId': '',
                 'ProductId': '',
                 'ExpectedQuantity': '',
                 'CargoInformationTypeId': '',
                 'ArrivalDate': '',
                 'DischargeDate': '',
                 'AmountDue': '',
                 'DateCreated': ''
             };

        };

        $scope.getCompanyInfo = function (id) {
            if (parseInt(id) < 1) {
                alert('An unknown error was encountered. Please try again');
                return;
            }

            $location.path('Importers/Importer/' + id);
        };
          
        $scope.getNotificationDetails = function (appId) {
            if (parseInt(appId) < 1) {
                alert("Invalid selection!");
                return;
            }
            userNotificationService.getNotificationInfo(appId, $scope.getNotificationDetailsCompleted);
        };

        $scope.getNotificationDetailsCompleted = function (response) {
            if (response == null || response.Id < 1) {
                alert("Notification information could not be retrieved. Please try again later.");
                return;
            }

            $scope.notification = response;
            $scope.suppliedDocs = [];
            $scope.bnkDocs = [];

            angular.forEach($scope.notification.DocumentTypeObjects, function (n, m) {
                if (n.Uploaded === true) {
                    $scope.suppliedDocs.push(n);
                }
                else {
                    $scope.bnkDocs.push(n);
                }
            });

            $scope.getReqs($scope.notification.ImporterId);
            $scope.viewApp = true;
        };

        $scope.getReqs = function (id) {
            userNotificationService.getEligibility(id, $scope.getReqsCompleted);
        };

        $scope.getReqsCompleted = function (response) {
            $scope.stadDocs = response;
            userNotificationService.getNotificationProcesses($scope.notification.Id, $scope.getNotificationProcessCompleted);
        };

        $scope.getNotificationProcessCompleted = function (data) {
            if (data.Id < 1) {
                return;
            }

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

        $scope.printNotification = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
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




