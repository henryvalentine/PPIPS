"use strict";

define(['application-configuration', 'zoneService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngZone', function ($compile) {
        return function ($scope, ngZone) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Zone/GetZoneObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngZone, tableOptions, 'Add Zone', 'prepareZoneTemplate', 'getZone', 'deleteZone', 130);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('zoneController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'zoneService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, zoneService, $upload, fileReader, $location) {

        $scope.prepareZoneTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/Zone/ProcessZone.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getZone = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          zoneService.getZone(impId, $scope.getZoneCompleted);
        };

        $scope.getZoneCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.zone = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.zone.Header = 'Update Zone';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Zone/ProcessZone.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processZone = function () {
            if ($scope.zone == null || $scope.zone.Name.length < 1) {
                alert('Please provide Zone Name');
                return;
            }

            var req =
               {
                   'Name': $scope.zone.Name
               };

            if ($scope.add) {
              zoneService.addZone(req, $scope.processZoneCompleted);
            }
            else {
              zoneService.editZone(req, $scope.processZoneCompleted);
            }

        };

        $scope.initializeController = function ()
        {
           $rootScope.setPageTitle('Zones');
           $scope.zone =
           {
               'Id': '',
               'Name': ''
              
           };
            $scope.zone.Header = "New Zone";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processZoneCompleted = function (data)
        {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Zone/ProcessZone.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteZone = function (id)
        {
            if (parseInt(id) > 0)
            {
                if (!confirm("This zone will be deleted permanently. Continue?"))
                {
                    return;
                }
                else if (confirm("This zone will be deleted permanently. Continue?"))
                {
                  zoneService.deleteZone(id, $scope.deleteZoneCompleted);
                }
            }
            else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteZoneCompleted = function (data)
        {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();
               
            }
        };

    }]);

});




