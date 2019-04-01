"use strict";

define(['application-configuration', 'ngDialog','userNotificationService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngNtfs', function ($compile)
    {
        return function ($scope, ngNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetNotifications";
            tableOptions.itemId = 'NotificationId';
            tableOptions.columnHeaders = ['ReferenceCode', 'PermitValue', 'ProductName', 'AmountDue', 'ExpectedQuantityStr', 'ArrivalDateStr', 'StatusStr'];
            var ttc = adminNotificationTableManager($scope, $compile, ngNtfs, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
  
    app.register.controller('importerNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$upload', 'fileReader', '$http', '$location','$window',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $upload, fileReader, $http, $location,$window)
    {
        $scope.initializeController = function ()
        {
            var importerId = $routeParams.id;
            if (parseInt(importerId) < 1) {
                $location.path('Importers/Importers');
            }
            $scope.ImporterId = importerId;
        };

        $scope.goToNotifications = function ()
        {
            $scope.viewApp = false;
            $scope.application = {};
        };

        $scope.getNotificationDetails = function (appId)
        {
            if (parseInt(appId) < 1)
            {
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
    }]);

});




