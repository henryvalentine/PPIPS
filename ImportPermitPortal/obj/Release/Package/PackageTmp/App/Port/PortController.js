"use strict";

define(['application-configuration', 'portService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngPort', function ($compile) {
        return function ($scope, ngPort) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Port/GetPortObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'CountryName'];
            var ttc = tableManager($scope, $compile, ngPort, tableOptions, 'Add Port', 'preparePortTemplate', 'getPort', 'deletePort', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('portController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'portService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, portService, $location) {

        $scope.preparePortTemplate = function () {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/Port/ProcessPort.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getPort = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            portService.getPort(impId, $scope.getPortCompleted);
        };

        $scope.getPortCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeReq();

            $scope.port = response;


            $scope.port =
 {
     'Id': $scope.port.Id,
     'Name': $scope.port.Name,

     'Country': { 'Id': response.CountryId, 'CountryName': response.Country }
     


 };
            $scope.edit = true;
            $scope.add = false;

            $scope.port.Header = 'Update Port';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Port/ProcessPort.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processPort = function () {
            if ($scope.port == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req = {
                Id: $scope.port.Id,
                Name: $scope.port.Name,
                CountryId: $scope.port.Country.Id

            };

            if ($scope.add) {
                portService.addPort(req, $scope.processPortCompleted);
            }
            else {
                portService.editPort(req, $scope.processPortCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Ports');
            portService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {
            $scope.countrys = data.Countries;

        };

        $scope.initializeReq = function () {
            $scope.port =
            {
                'Id': '',
                'Name': '',
                'Country': { 'Id': '', 'Name': 'Select Country' }

            };
            $scope.add = true;
            $scope.edit = false;
            $scope.port.Header = 'Add Port';
            $scope.buttonText = "Add";
        };

        $scope.processPortCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Port/ProcessPort.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deletePort = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                portService.deletePort(id, $scope.deletePortCompleted);

            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deletePortCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);

            }
            else {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };


      

    }]);

});




