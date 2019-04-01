"use strict";

define(['application-configuration', 'depotOwnerService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.directive('ngThrHistory', function ($compile)
    {
        return function ($scope, ngThrHistory)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetDepotOwnerHistory";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ImporterName', 'ProductName', 'CountryOfOriginName', 'EstimatedQuantityStr', 'EstimatedValueStr', 'StatusStr'];
            var ttc = bnkAppHistoryTableManager($scope, $compile, ngThrHistory, tableOptions, 'getAppDetail');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
   

    app.register.controller('throughputController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'depotOwnerService', '$location', '$upload',
    function (ngDialog, $scope, $rootScope, $routeParams, depotOwnerService, $location, $upload)
    {
        $scope.getAppDetail = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            depotOwnerService.getAssignedApp(appId, $scope.getAppDetailCompleted);
        };

        $scope.getAppDetailCompleted = function (data)
        {
            if (data == null || data.Id < 1)
            {
                alert('Product information could not be retrieved. Please try again');
                return;
            }
           
            depotOwnerService.setApp(data);
            $location.path('/DepotOwner/ProcessDocument');
        };
        $scope.initializeController = function() {
            $rootScope.setPageTitle('Throughput History');
        };
    }]);

});




