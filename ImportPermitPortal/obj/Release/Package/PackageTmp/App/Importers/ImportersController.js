"use strict";

define(['application-configuration'], function (app)
{
    app.register.directive('ngImporter', function ($compile) {
        return function ($scope, ngImporter) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Importer/GetImporterObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'TIN', 'RCNumber', 'DateAdded', 'StatusStr'];
            var ttc = importerstableManager($scope, $compile, ngImporter, tableOptions, 'getImporterDetails');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('importerController', ['$scope', '$rootScope', '$routeParams', '$location', '$http',
    function ($scope, $rootScope, $routeParams, $location, $http)
    {
        $rootScope.setPageTitle('Importers|DPR-PPIPS');
        $rootScope.getImporterDetails = function (importerId)
        {
            if (parseInt(importerId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            $rootScope.importerId = importerId;
            $location.path('Importers/Importer/' + $rootScope.importerId);
            
        };

        //$rootScope.getImporterDetailsCompleted = function (response)
        //{
        //    if (response.Code < 1) {
        //        alert('An error was encountered. Please try again.');
        //        return;
        //    }

        //    $rootScope.importerName = response.Error;
        //    $location.path('Importers/Importer/' + $rootScope.importerId);
        //};

    }]);

});




