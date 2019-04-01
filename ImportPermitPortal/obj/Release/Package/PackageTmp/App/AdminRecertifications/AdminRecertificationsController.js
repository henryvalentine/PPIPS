"use strict";

define(['application-configuration', 'ngDialog', 'adminRecertificationService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngAdcert', function ($compile)
    {
        return function ($scope, ngAdcert)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Admin/GetRecertificationObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'DateApplied', 'StatusStr'];
            var ttc = adminRecertificationTableManager($scope, $compile, ngAdcert, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
            $scope.getLists();
        }; 
    });
  
    app.register.controller('RecertificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'adminRecertificationService', '$upload', 'fileReader', '$http', '$location', '$window',
    function (ngDialog, $scope, $rootScope, $routeParams, adminRecertificationService, $upload, fileReader, $http, $location, $window)
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
            adminRecertificationService.getGenericList($scope.getListsCompleted);

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
            $rootScope.setPageTitle('Recertifications|DPR-PPIPS');
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
          
        $scope.getNotificationDetails = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            adminRecertificationService.getRecertification(appId, $scope.getNotificationDetailsCompleted);
        };

        $scope.getNotificationDetailsCompleted = function (response)
        {
            if (response == null || response.NotificationId < 1) {
                alert('Recertification information could not be retrieved. Please try again later.');
                return;
            }

            $scope.application = response;
            $scope.viewApp = true;
        };

       

        $scope.printNotification = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            adminRecertificationService.printRecertification(appId, $scope.printRecertificationCompleted);
        };

        $scope.printRecertificationCompleted = function (response)
        {
            window.open(response);

        };
    }]);

});




