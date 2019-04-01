"use strict";

define(['application-configuration', 'importerAppService'], function (app)
{
    app.register.directive('ngApps', function ($compile)
    {
        return function ($scope, ngApps)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetImporterApplications?id=" + $scope.ImporterId;
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'DerivedQuantityStr', 'DerivedValue', 'DateAppliedStr', 'LastModifiedStr', 'StatusStr'];
            var ttc = importerApplicationtableManager($scope, $compile, ngApps, tableOptions);
            ttc.jTable.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc.jTable;
            $scope.companyName = ttc.companyName;
        };
    });

    app.register.controller('importerApplicationController', ['$scope', '$rootScope', '$routeParams', 'importerAppService', '$location',
    function ($scope, $rootScope, $routeParams, importerAppService, $location)
    {
        $scope.getAppView = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            $scope.getApp(appId);
        };

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Importer Applications|DPR-PPIPS');
            $scope.viewApp = false;

            var importerId = $routeParams.id;
            if (parseInt(importerId) < 1) {
                $location.path('Importers/Importers');
            }
            $scope.ImporterId = importerId;
        };


        $scope.getApp = function (appId) {
            importerAppService.getImportAppDocsX(appId, $scope.getAppCompleted);
        };

        $scope.getAppCompleted = function (application) {
            if (application.Id < 1) {
                alert('An error was encountered. The Application Infromation could not be retrieved.');
                return;
            }

            $scope.application = application;
            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            $scope.nextDocs = [];

            $scope.newDpocList = [];
            $scope.taxClrnc = {};
            angular.forEach($rootScope.documentTypes, function (n, m)
            {
                if (n.DocumentTypeId === 3) {
                    $scope.taxClrnc = n;
                }
                else {
                    n.className = 't' + n.DocumentTypeId;
                    $scope.newDpocList.push(n);
                }
            });

            angular.forEach($scope.application.DocumentTypeObjects, function (n, m) {
                if (n.Uploaded === true) {
                    n.index = m + 1;
                    $scope.suppliedDocs.push(n);
                }
                else {
                    if (n.StageId === 1 || n.IsDepotDoc === true) {
                        $scope.bnkDocs.push(n);
                    }
                    else {
                        $scope.nextDocs.push(n);
                    }
                }
            });

            $scope.viewApp = true;
        };

        $scope.showApps = function () {
            $scope.viewApp = false;
            $scope.application = {};
            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            $scope.nextDocs = [];
            $scope.newDpocList = [];
            $scope.taxClrnc = {};
           
        };
        $scope.goToImporters = function ()
        {
            $location.path('/Importers/Importer/' + $scope.ImporterId);
        };
    }]);

});




