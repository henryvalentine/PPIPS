"use strict";

define(['application-configuration'], function (app)
{
    
    app.register.controller('contactController', ['$scope', '$rootScope',
    function ($scope, $rootScope)
    {
        $rootScope.setPageTitle('Contact Us|DPR-PPIPS');

    }]);
    
});


