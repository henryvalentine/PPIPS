"use strict";

define(['application-configuration', 'contentService'], function (app)
{
    
    app.register.controller('genInfoController', ['$scope', '$rootScope', '$routeParams', 'contentService', '$location',
    function ($scope, $rootScope, $routeParams, contentService, $location) {
        $scope.initializeController = function () {
            $rootScope.setPageTitle('General Information|DPR-PPIPS');

            var contentId = $routeParams.id;
            if (parseInt(contentId) < 1) {
                $location.path('Dashboard/Dashboard');
            }
            contentService.getContent(contentId, $scope.getContentCompleted);
        };

        $scope.getContentCompleted = function (response) {
            if (response.Id < 1) {

                $location.path('Dashboard/Dashboard');
            }

            angular.element('#contentDiv').html(response.BodyContent);
            angular.element('#contentHeader').html(response.Title);
        };
    }]);
    
});


