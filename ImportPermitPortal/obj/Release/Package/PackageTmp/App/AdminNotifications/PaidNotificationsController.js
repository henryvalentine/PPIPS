"use strict";

define(['application-configuration', 'ngDialog','userNotificationService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngPNtfs', function ($compile)
    {
        return function ($scope, ngPNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetPaidNotifications";
            tableOptions.itemId = 'NotificationId';
            tableOptions.columnHeaders = ['ImporterName', 'ReferenceCode', 'PermitValue', 'ProductName', 'AmountDue', 'ExpectedQuantityStr', 'ArrivalDateStr', 'DischargeDateStr', 'StatusStr'];
            var ttc = adminNotificationTableManager($scope, $compile, ngPNtfs, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
            $scope.getLists();
        }; 
    });
  
    app.register.controller('myNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$upload', 'fileReader', '$http', '$location','$window',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $upload, fileReader, $http, $location,$window)
    {

        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 2;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDate($scope, miniDate, '');
        setControlDate($scope, miniDate, '');

        $scope.getLists = function ()
        {
            userNotificationService.getGenericList($scope.getListsCompleted);

        };

        $scope.getListsCompleted = function (data)
        {
            $scope.ordinaryproducts = data.Products;
            $scope.countries = data.Countries;
            $scope.localJetties = data.LocalJetties;
            $scope.products = $scope.ordinaryproducts;
        };

        $scope.initializeApp = function ()
        {
            $rootScope.setPageTitle('Notifications|DPR-PPIPS');
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
          
        $scope.getNotificationDetails = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            userNotificationService.getNotificationForEditing(appId, $scope.getNotificationDetailsCompleted);
        };

        $scope.getNotificationDetailsCompleted = function (response)
        {
            if (response == null || response.NotificationId < 1) {
                alert('Notification information could not be retrieved. Please try again later.');
                return;
            }

            $scope.application = response;
            $scope.viewApp = true;
        };

        $scope.continueNotification = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            userNotificationService.getNotificationForEditing(appId, $scope.continueNotificationCompleted);
        };

        $scope.continueNotificationCompleted = function (response)
        {
            if (response == null || response.NotificationId < 1)
            {
                alert('Notification information could not be retrieved. Please try again later.');
                return;
            }
            userNotificationService.setApp(response);
            $location.path('/Notification/EditNotification');
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




