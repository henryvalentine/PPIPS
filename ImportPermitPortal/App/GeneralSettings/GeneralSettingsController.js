"use strict";

define(['application-configuration'], function (app)
{
    
    app.register.controller('generalSettingsController', ['$scope', '$rootScope', '$routeParams',
    function ($scope, $rootScope, $routeParams)
    {
        $rootScope.setPageTitle('General Settings');
    }]);

});