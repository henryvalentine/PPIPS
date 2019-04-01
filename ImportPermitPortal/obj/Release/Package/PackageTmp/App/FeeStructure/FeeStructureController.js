"use strict";

define(['application-configuration'], function (app)
{
    app.register.controller('feeStructureController', function ($scope, $http, $window) {
        $window.document.title = 'DPR-PPIPS|Fees Structure';

        $scope.getFeeStructures = function ()
        {
            $scope.busy = true;
            setTimeout(function () {
                $http({ method: 'GET', url: '/Account/GetFeeStructures' }).success(function (response) {
                    $scope.busy = false;
                    $scope.fees = response;
                });
            }, 1000);
        };

     });
});




