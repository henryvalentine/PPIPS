"use strict";

define(['application-configuration', 'permitService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngPermit', function ($compile) {
        return function ($scope, ngPermit) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Permit/GetImporterPermits";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['PermitValue', 'PermitStatusStr', 'IssueDateStr', 'ExpiryDateStr'];
         
            var ttc = permitTableManager($scope, $compile, ngPermit, tableOptions, 'getPermit');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('importerPermitController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'permitService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, permitService, $upload, fileReader, $location) {

       
        $scope.getPermit = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          permitService.getPermitFile(impId, $scope.getPermitCompleted);
        };

        $scope.getPermitCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

          

            $scope.permit.file = response;
            
            //var file = new Blob([response], { type: 'application/pdf' });
            //var fileURL = URL.createObjectURL(file);
            window.open(response);

            $scope.permit.Header = 'Update Permit';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Permit/ProcessPermit.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };


        $scope.initializeController = function () {
            $rootScope.setPageTitle('Importer Permits|DPR-PPIPS');
         
        };

    }]);

});




