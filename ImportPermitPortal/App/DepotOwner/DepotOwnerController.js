"use strict";

define(['application-configuration', 'depotOwnerService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.directive('ngDepotOwner', function ($compile)
    {
        return function ($scope, ngDepotOwner)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetDepotOwnerItems";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ImporterName', 'ProductName', 'CountryOfOriginName', 'EstimatedQuantityStr', 'EstimatedValueStr', 'StatusStr'];
            var ttc = bnkAppHistoryTableManager($scope, $compile, ngDepotOwner, tableOptions, 'getAppDetail');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
   

    app.register.controller('depotOwnerController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'depotOwnerService', '$location', '$upload',
    function (ngDialog, $scope, $rootScope, $routeParams, depotOwnerService, $location, $upload)
    {
        $scope.getAppDetail = function (appId)
        {
            $rootScope.setPageTitle('Applications');
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
        

    }]);

});




