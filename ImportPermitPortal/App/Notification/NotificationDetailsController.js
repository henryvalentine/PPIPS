"use strict";

define(['application-configuration', 'userNotificationService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    
    app.register.controller('notificationDetailsController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $location, $upload, fileReader, $http)
    {
        $scope.gotoNotifications = function ()
        {
            $location.path('/Notification/MyNotifications');

        };
        //submitVessels
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Notification Details');
           var app = userNotificationService.getNotification();
            if (app == null || app.Id < 1)
            {
                alert('An errror was encountered on the page.  Please reselect the Notification.');
                $scope.goToApps();
            } else {
                $scope.notification = app;
                $scope.bnkDocs = [];
                $scope.suppliedDocs = [];
                $scope.nextDocs = [];
               
                angular.forEach($scope.notification.DocumentTypeObjects, function (n, m)
                {
                    if (n.Uploaded === true)
                    {
                        n.index = m + 1;
                        $scope.suppliedDocs.push(n);
                    }
                    else
                    {
                        if (n.StageId === 1)
                        {
                            $scope.bnkDocs.push(n);
                        }
                        else
                        {
                            $scope.nextDocs.push(n);
                        }
                    }
                });
                
                $scope.permitValue = app.PermitValue;
                $scope.notification.ArrivalDate = app.ArrivalDateStr;
                $scope.notification.DischargeDate = app.DischargeDateStr;
                $scope.notificationType = app.NotificationTypeId;
                $scope.cargoType = app.CargoInformationTypeId;
                $scope.getNotificationVessels($scope.notification.Id);
                if ($scope.notification.Rrr != null )
                {
                    if ($scope.notification.Rrr.length > 0 && $scope.notification.Status < 2 && $scope.notification.PaymentTypeId < 3)
                    {
                        $scope.preparePaymentForm($scope.notification);
                    }
                }
            }
            
        };
        
        $scope.getNotificationVessels = function (notificationId)
        {
            if (notificationId == null || notificationId < 1)
            {
                alert('An errror was encountered on the page.  Please reselect the Notification.');
                $location.path('/Notification/MyNotifications');
            }
            userNotificationService.getNotificationVesssels(notificationId, $scope.getNotificationVesselsCompleted);
        };

        $scope.getNotificationVesselsCompleted = function (vessels)
        {
            $scope.vesselTracker = [];
            if (vessels.length > 0)
            {
               angular.forEach(vessels, function (y, k) 
                {
                    $scope.vesselTracker.push(y);
                });
            }
            
        };

        $scope.updateApp = function ()
        {
            $location.path('Notification/ContinueNotification');
        };


        $scope.preparePaymentForm = function (data) {
            var pType = '';
            angular.forEach($scope.PaymentTypes, function (k, i)
            {
                if ($scope.application.PaymentTypeId === i.Id)
                {
                    pType = k.Identity;
                }
            });

            var frmInfo = '<form action="https://login.remita.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                + '<input name="merchantId" value="' + 442773233 + '" type="hidden">'
                + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                + '<input name="rrr" value="' + data.ReferenceCode + '" id="rrr" type="hidden">'
                + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                + '</form>';

            //var frmInfo = '<form action="http://www.remitademo.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
            //    + '<input name="merchantId" value="' + 2547916 + '" type="hidden">'
            //    + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
            //    + '<input name="rrr" value="' + data.ReferenceCode + '" id="rrr" type="hidden">'
            //    + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
            //    + '</form>';

            $scope.frm = frmInfo;
            $scope.frmUrl = data.RedirectUri;

        };

        $scope.pay = function () {
            var content = $scope.frm;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes,menubar=no,toolbar=no,location=no,status=no');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" />' +
                    '</head><body><div class="reward-body">' + content + '</div></html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }
            else {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" /></head><body>' + content + '</html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }

            $location.path('Application/MyApplications');
        }
    }]);
    
});
