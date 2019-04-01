﻿"use strict";

define(['application-configuration', 'bnkNotificationService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{

   app.register.directive('ngBnkNtfs', function ($compile)
    {
        return function ($scope, ngBnkNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Notification/GetBankAssignedNotifications";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'ImporterName', 'ProductCode', 'QuantityToDischargeStr', 'ArrivalDateStr', 'StatusStr'];
            var ttc = bnkUserUnprocessedNotificationTableManager($scope, $compile, ngBnkNtfs, tableOptions);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
    
    app.register.controller('notificationIndexController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkNotificationService', '$location', '$upload', '$timeout', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkNotificationService, $location, $upload, $timeout, fileReader, $http)
    {
        $scope.initializeController = function ()  
        {
            $rootScope.setPageTitle('Notifications|DPR-PPIPS');
            $scope.viewApp = false;
           
        };

        $scope.getNotificationByReference = function (e)
        {
            var el = (e.srcElement || e.target);

            $scope.referenceCode = el.id;

            if ($scope.referenceCode == null || $scope.referenceCode.length < 1)
            {
                alert('Invalid selection.');
                return;
            }

            $location.path('/BnkAppUsr/UploadNotificationDocs/' + $scope.referenceCode.trim());
        };

        $scope.viewNotificationByReference = function (e) {
            var el = (e.srcElement || e.target);

            $scope.referenceCode = el.id;

            if ($scope.referenceCode == null || $scope.referenceCode.length < 1) {
                alert('Invalid selection.');
                return;
            }

            bnkNotificationService.searchNotification($scope.referenceCode, $scope.getNotificationByReferenceCompleted);
        };

        $scope.getNotificationByReferenceCompleted = function (response)
        {
            if (response == null || response.Id < 1) {
                alert('The Notification could not be found.');
                return;
            }

            $scope.notification = response;
            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
          
            if (response.Status !== 3 && response.Status !== 5 && response.Status !== 6 && response.Status !== 7 && response.Status !== 8 && response.Status !== 9) {
                $scope.isLegit = true;
            } else {
                $scope.isLegit = false;
            }

            angular.forEach(response.DocumentTypeObjects, function (n, m)
            {
                if (n.IsFormM === true) {
                    $scope.formM = n;
                }
                else {
                    if (n.Uploaded === true) {
                        n.index = m + 1;
                        $scope.suppliedDocs.push(n);
                    }
                    else {
                        if (n.IsBankDoc === true) {
                            $scope.bnkDocs.push(n);
                        }
                    }
                }
            });

            $scope.initializeFormM();
            if ($scope.notification.FormMDetailObjects != null && $scope.notification.FormMDetailObjects.length > 0) {
                $scope.formMBtnText = 'Update Form M Details';
                $scope.formM = $scope.notification.FormMDetailObjects[0];
                $scope.formM.DateIssued = $scope.formM.DateIssuedStr;
            }

            $scope.initializeBanker();

            if ($scope.notification.NotificationBankerObjects != null && $scope.notification.NotificationBankerObjects.length > 0)
            {
                var appBanker = $scope.notification.NotificationBankerObjects[0];
                $scope.getAppBankerInfoCompleted(appBanker);
            }

            $scope.permitValue = response.PermitValue;
            $scope.notification.ArrivalDate = app.ArrivalDateStr;
            $scope.notification.DischargeDate = app.DischargeDateStr;
            $scope.viewApp = true;
        };

        $scope.initializeBanker = function () {
            $scope.fnLt = {};
            $scope.fnltList = [];
            $scope.appBanker =
            {
                'FinancedQuantity': '',
                'ProductId': '',
                'ActualQuantity': '',
                'TransactionAmount': '',
                'ApplicationId': '',
                'BankId': '',
                'FinLetterPath': ''
            };

        };
    }]);
    
});
