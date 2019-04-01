"use strict";

define(['application-configuration', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.controller('sysStatsController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, $location, $upload, fileReader, $http)
    {

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('System Statistics|DPR-PPIPS');
            $rootScope.getAppCount("/Application/GetAdminCounts", $scope.getStatsCompleted);
        };
       
        $scope.getStatsCompleted = function (data)
        {
            angular.forEach(data, function (a, k)
            {
                a.PaidAppHref = 'ngappr.html#' + a.PaidAppHref;
                a.SubmittedAppCountHref = 'ngappr.html#' + a.SubmittedAppCountHref;
                a.ApprovedAppHref = 'ngappr.html#' + a.ApprovedAppHref;
                a.ProcessingAppHref = 'ngappr.html#' + a.ProcessingAppHref;
                a.RejectedHref = 'ngappr.html#' + a.RejectedHref;
            });
            $scope.reports = data;

        };
        
    }]);
    
});


