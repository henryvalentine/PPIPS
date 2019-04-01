"use strict";

define(['application-configuration', 'unitOfMeasurementService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngUnitOfMeasurement', function ($compile) {
        return function ($scope, ngUnitOfMeasurement) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/UnitOfMeasurement/GetUnitOfMeasurementObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngUnitOfMeasurement, tableOptions, 'Add Unit Of Measurement', 'prepareUnitOfMeasurementTemplate', 'getUnitOfMeasurement', 'deleteUnitOfMeasurement', 200);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('unitOfMeasurementController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'unitOfMeasurementService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, unitOfMeasurementService, $upload, fileReader, $location) {

        $scope.prepareUnitOfMeasurementTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/UnitOfMeasurement/ProcessUnitOfMeasurement.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getUnitOfMeasurement = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          unitOfMeasurementService.getUnitOfMeasurement(impId, $scope.getUnitOfMeasurementCompleted);
        };

        $scope.getUnitOfMeasurementCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.unitOfMeasurement = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.unitOfMeasurement.Header = 'Update UnitOfMeasurement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/UnitOfMeasurement/ProcessUnitOfMeasurement.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processUnitOfMeasurement = function () {
            if ($scope.unitOfMeasurement == null || $scope.unitOfMeasurement.Name.length < 1) {
                alert('Please provide UnitOfMeasurement Name');
                return;
            }

            var req =
       {
          
           'Name': $scope.unitOfMeasurement.Name
         

       };

            if ($scope.add) {
              unitOfMeasurementService.addUnitOfMeasurement(req, $scope.processUnitOfMeasurementCompleted);
            }
            else {
              unitOfMeasurementService.editUnitOfMeasurement(req, $scope.processUnitOfMeasurementCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Units of Measurement');
            $scope.unitOfMeasurement =
           {
               'Id': '',
               'Name': ''
              
           };
            $scope.unitOfMeasurement.Header = "New Unit Of Measurement";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processUnitOfMeasurementCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/UnitOfMeasurement/ProcessUnitOfMeasurement.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteUnitOfMeasurement = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This unitOfMeasurement will be deleted permanently. Continue?")) {
                    return;
                } else if (confirm("This unitOfMeasurement will be deleted permanently. Continue?")) {
                  unitOfMeasurementService.deleteUnitOfMeasurement(id, $scope.deleteUnitOfMeasurementCompleted);
                   
                }
                
                
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteUnitOfMeasurementCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();
               
            }
        };

    }]);

});




