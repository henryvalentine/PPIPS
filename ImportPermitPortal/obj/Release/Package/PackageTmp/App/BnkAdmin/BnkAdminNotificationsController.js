"use strict";

define(['application-configuration', 'ngDialog','bnkNotificationService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngBnkNtfs', function ($compile)
    {
        return function ($scope, ngBnkNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetBankAssignedNotifications";
            tableOptions.itemId = 'NotificationId';
            tableOptions.columnHeaders = ['ReferenceCode', 'Company', 'ProductCode', 'ExpectedQuantityStr', 'ArrivalDateStr', 'StatusStr'];
            var ttc = bnkAdminNotificationTableManager($scope, $compile, ngBnkNtfs, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });

    app.register.controller('myNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkNotificationService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkNotificationService, $upload, fileReader, $http, $location)
    {
        $scope.getNotificationDetails = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            bnkNotificationService.getBankNotification(appId, $scope.getNotificationDetailsCompleted);
        };

        $scope.getNotificationDetailsCompleted = function (response) {
            if (response == null || response.NotificationId < 1) {
                alert('Notification information could not be retrieved. Please try again later.');
                return;
            }
            bnkNotificationService.setNotificationX(response);
            $location.path('/BnkAdmin/bnkAdminNotificationDetails');
        };
        $scope.initializeController = function () {
            $rootScope.setPageTitle('Notifications|DPR-PPIPS');
            
        };

    }]);

});




