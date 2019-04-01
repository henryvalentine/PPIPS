"use strict";

define(['application-configuration', 'permitService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngPermit', function ($compile) {
        return function ($scope, ngPermit) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Permit/GetApplicantPermitObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['PermitValue','CompanyName', 'PermitStatusStr', 'IssueDateStr', 'ExpiryDateStr'];
         
            var ttc = permitTableManager($scope, $compile, ngPermit, tableOptions, 'getPermit');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('permitController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'permitService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, permitService, $upload, fileReader, $location) {

       
        $scope.getPermit = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            permitService.getPermitInfo(impId, $scope.getPermitCompleted);
        };

        $scope.getPermitCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Permit information could not be retrieved.');
                return;
            }
            
            $scope.viewApp = true;
            $scope.application = response;
        };
        
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('My Permits|DPR-PPIPS');
            $scope.viewApp = false;
            $scope.application = {};
        };

        $scope.generatePermit = function ()
        {
           
            if (parseInt($scope.application.PermitId) < 1)
            {
                alert('An error was encountered on the page. Please refresh the page and try again!');
                return;
            }
            permitService.generatePermit($scope.application.PermitId, $scope.generatePermitCompleted);
        };

        $scope.generatePermitCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert(response.Error);
                return;
            }

            $scope.permitPath = response.Path;

            window.open($scope.permitPath);
        };

    }]);

});




