"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.directive('ngHistory', function ($compile)
    {
        return function ($scope, ngHistory)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetBankUserJobHistory";
            tableOptions.itemId = 'ApplicationId';
            tableOptions.columnHeaders = ['ReferenceCode', 'ImporterStr', 'DerivedQuantityStr', 'DerivedValue', 'DateAppliedStr', 'StatusStr'];
            var ttc = bnkAppHistoryTableManager($scope, $compile, ngHistory, tableOptions, 'getAppDetail', 'getAppDocuments');
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });  
   app.register.controller('usrJobHistoryController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $location)
    {
        $scope.getAppDetail = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            bnkAdminService.getAssignedApp(appId, $scope.getAppViewCompleted);
        };

        $scope.getAppViewCompleted = function (data)
        {
            if (data == null || data.ApplicationId < 1)
            {
                alert('Application Information. Please try again');
                return;
            }
            $scope.productDocuments = [];
            $scope.unsuppliedProductDocuments = [];

            $scope.products = [];

            angular.forEach(data.ApplicationItemObjects, function (g, m) {
                $scope.products.push(g.ProductObject);

                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0) {
                    angular.forEach(g.ProductBankerObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0) {
                            y.index = p + 1;
                            y.IsUploaded = true;
                            $scope.productDocuments.push(y);
                        }

                        else {
                            y.IsUploaded = false;
                            $scope.unsuppliedProductDocuments.push(y);
                        }
                    });
                }

            });

            $scope.application = data;
            $scope.viewApp = true;
        };
        
        $scope.getAppDocuments = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            bnkAdminService.setId(appId);
            bnkAdminService.setRoleInfo(appId);
            $location.path('/AppDocs/AppDocs');
        };

    }]);

});




