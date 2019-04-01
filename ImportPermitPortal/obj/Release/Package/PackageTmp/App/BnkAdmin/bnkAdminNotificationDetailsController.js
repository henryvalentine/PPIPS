"use strict";

define(['application-configuration', 'bnkNotificationService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    
    app.register.controller('bnkNotificationDetailsController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkNotificationService', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkNotificationService, $location, $upload, fileReader, $http)
    {
        $scope.gotoNotifications = function ()
        {
            $location.path('/BnkAppUsr/NotificationHistory');
        };
        //submitVessels
        $scope.initializeController = function ()
        {

            var app = bnkNotificationService.getNotificationX();
            if (app == null || app.NotificationId < 1)
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
                        $scope.suppliedDocs.push(n);
                    }
                    else
                    {
                        if (n.IsBankDoc === true)
                        {
                            $scope.bnkDocs.push(n);
                        }
                    }
                });
                
                $scope.permitValue = app.PermitValue;
                $scope.notification.ArrivalDate = app.ArrivalDateStr;
                $scope.notification.DischargeDate = app.DischargeDateStr;
                $scope.notificationType = app.NotificationTypeId;
                $scope.cargoType = app.CargoInformationTypeId;
            }
            
        };
        
    }]);
    
});
