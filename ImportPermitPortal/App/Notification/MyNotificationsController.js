"use strict";

define(['application-configuration', 'ngDialog','userNotificationService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngNtfs', function ($compile)
    {
        return function ($scope, ngNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetNotificationObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'PermitValue', 'ProductName', 'ExpectedQuantityStr', 'AmountDueStr', 'ArrivalDateStr', 'StatusStr'];
            var ttc = userNotificationTableManager($scope, $compile, ngNtfs, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
  
    app.register.controller('myNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $upload, fileReader, $http, $location)
    {
        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 2;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDate($scope, miniDate, '');
        setControlDate($scope, miniDate, '');
        
        $scope.getShuttleVessels = function ()
        {
            userNotificationService.getShuttleVessels($scope.getShuttleVesselsCompleted);
        };

        $scope.getShuttleVesselsCompleted = function (data)
        {
            $rootScope.vessels = data;
        };

        $scope.initializeApp = function ()
        {
            $scope.viewApp = false;
            $scope.notification =
             {
                 'Id': '',
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
           
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('My Notifications');
            $scope.viewApp = false;
            $scope.notification =
             {
                 'Id': '',
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
            $scope.getShuttleVessels();
        };
        
        $scope.editNotification = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            userNotificationService.getNotificationForEdit(appId, $scope.editNotificationCompleted);
        };

        $scope.recertify = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            userNotificationService.getNotificationForEditing(appId, $scope.editNotificationCompleted);
        };

        $scope.editNotificationCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Notification information could not be retrieved. Please try again later.');
                return;
            }

            userNotificationService.setNotification(response);
            $location.path('/Notification/EditNotification');
        };

        $scope.getNotificationDetails = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            userNotificationService.getNotificationForEdit(appId, $scope.getNotificationDetailsCompleted);
        };

        $scope.getNotificationDetailsCompleted = function (response)
        {
            if (response == null || response.Id < 1) {
                alert('Notification information could not be retrieved. Please try again later.');
                return;
            }
            userNotificationService.setNotification(response);
            $location.path('/Notification/NotificationDetails');
        };

        $scope.continueNotification = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            $location.path('/Notification/ContinueNotification/' + appId);
            //userNotificationService.getNotificationForEdit(appId, $scope.continueNotificationCompleted);
        };

        //$scope.continueNotificationCompleted = function (response)
        //{
        //    if (response == null || response.Id < 1)
        //    {
        //        alert('Notification information could not be retrieved. Please try again later.');
        //        return;
        //    }
        //    userNotificationService.setNotification(response);
        //    $location.path('/Notification/ContinueNotification/');
        //};
    }]);

});




