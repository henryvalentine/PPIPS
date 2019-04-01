"use strict";

define(['application-configuration', 'appSettingService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.controller('appSettingController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'appSettingService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, appSettingService, $location)
    {
        $scope.getAppSetting = function ()
        {
            appSettingService.getApplicationSetting($scope.getAppSettingCompleted);
        };

        $scope.getAppSettingCompleted = function (response)
        {
            $scope.initializeController();
            if (response.ApplicationSettingId > 0)
            {
                $scope.appSetting = response;
                $scope.edit = true;
                $scope.add = false;
                $scope.startEdit = false;
            }
        };

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Settings');
           $scope.appSetting =
           {
               'ApplicationSettingId': '', 'ApplicationExpiry': '', 'ApplicationLifeCycle': '', 'VesselArrivalLeadTime': '', 'VesselDischargeLeadTime': '', 'DischargeQuantityTolerance': '',
               'PermitExpiryTolerance' : '', 'PermitValidity': ''
           };
            
           $scope.add = true;
           $scope.startEdit = true; 
           $scope.edit = false;
        };
        
        $scope.processAppSetting = function ()
        {
            if ($scope.appSetting.ApplicationExpiry < 1)
            {
                alert('Please provide Application Expiry');
                return;
            }

            if ($scope.appSetting.ApplicationLifeCycle < 1)
            {
                alert('Please provide Application Life Cycle');
                return;
            }
            if ($scope.appSetting.PriceVolumeThreshold < 1) {
                alert('Please provide Price-Volume Threshold');
                return;
            }
            if ($scope.appSetting.VesselArrivalLeadTime < 1) {
                alert('Please provide Vessel Arrival Lead Time');
                return;
            }
            if ($scope.appSetting.VesselDischargeLeadTime < 1) {
                alert('Please provide Vessel Discharge Lead Time');
                return;
            }
            //if ($scope.appSetting.DischargeQuantityTolerance < 1)
            //{
            //    alert('Please provide Discharge Quantity Tolerance');
            //    return;
            //}

            if ($scope.appSetting.PermitValidity < 1)
            {
                alert('Please provide Permit Validity');
                return;
            }
            
            if ($scope.add)
            {
                appSettingService.addApplicationSetting($scope.appSetting, $scope.processAppSettingCompleted);
            }
            else
            {
                appSettingService.editApplicationSetting($scope.appSetting, $scope.processAppSettingCompleted);
            }
            
        };

        $scope.processAppSettingCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else
            {
                alert(data.Error);
                $scope.edit = true;
                $scope.add = false;
                $scope.startEdit = false;
            }
        };
        
    }]);

});




