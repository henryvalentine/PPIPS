"use strict";

define(['application-configuration', 'regionService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngRegion', function ($compile) {
        return function ($scope, ngRegion) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Region/GetRegionObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngRegion, tableOptions, 'Add Region', 'prepareRegionTemplate', 'getRegion', 'deleteRegion', 130);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('regionController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'regionService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, regionService, $upload, fileReader, $location) {

        $scope.prepareRegionTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/Region/ProcessRegion.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getRegion = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          regionService.getRegion(impId, $scope.getRegionCompleted);
        };

        $scope.getRegionCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.region = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.region.Header = 'Update Region';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Region/ProcessRegion.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processRegion = function () {
            if ($scope.region == null || $scope.region.Name.length < 1) {
                alert('Please provide Region Name');
                return;
            }

            var req =
       {
          
           'Name': $scope.region.Name
         

       };

            if ($scope.add) {
              regionService.addRegion(req, $scope.processRegionCompleted);
            }
            else {
              regionService.editRegion(req, $scope.processRegionCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Regions');
            $scope.region =
           {
               'Id': '',
               'Name': ''
              
           };
            $scope.region.Header = "New Region";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processRegionCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Region/ProcessRegion.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteRegion = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This region will be deleted permanently. Continue?")) {
                    return;
                } else if (confirm("This region will be deleted permanently. Continue?")) {
                  regionService.deleteRegion(id, $scope.deleteRegionCompleted);
                   
                }
                
                
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteRegionCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();
               
            }
        };

    }]);

});




