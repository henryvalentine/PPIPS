"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    
    app.register.controller('appDocsController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $location)
    {
        $scope.initializeController = function ()
        {
            $scope.gettingApp = false;

            var appId = bnkAdminService.getId();
            if (appId < 1) {
                alert('An error was encountered on the page. Please try again.');
                $location.path('/JobHistory/JobHistory');
            }

            $scope.getAppInfo(appId);
        };
        
        $scope.getAppInfo = function (appId)
        {
            if (appId < 1)
            {
                alert('Invalid selection.');
                return;
            }
            $scope.gettingApp = true;
            bnkAdminService.getHistoryAppDocuments(appId, $scope.getAppDocsCompleted);

        };
        
        $scope.getAppDocsCompleted = function (data)
        {
            $scope.gettingApp = false;
            $scope.suppliedDocs = [];
            $scope.bnkDocs = [];
            $scope.application = data;
            angular.forEach(data.DocumentTypeObjects, function (n, m)
            {
                if (n.Uploaded === true)
                {
                    $scope.suppliedDocs.push(n);
                }
                else
                {
                  $scope.bnkDocs.push(n);
                }
            });

        };

        $scope.backToHistory = function () {
            $location.path('/JobHistory/JobHistory');
        };

     
    }]);

});




