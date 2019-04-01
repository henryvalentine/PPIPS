"use strict";

define(['application-configuration', 'storageTankService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngStorageTank', function ($compile) {
        return function ($scope, ngStorageTank) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/StorageTank/GetStorageTankObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'TankNo', 'Capacity', 'DepotName','ProductName'];
            var ttc = tableManager($scope, $compile, ngStorageTank, tableOptions, 'Add StorageTank', 'prepareStorageTankTemplate', 'getStorageTank', 'deleteStorageTank', 170);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('storageTankController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'storageTankService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, storageTankService, $location) {

        $scope.prepareStorageTankTemplate = function () {
            $scope.initialize();
            ngDialog.open({
                template: '/App/StorageTank/ProcessStorageTank.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getStorageTank = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            storageTankService.getStorageTank(impId, $scope.getStorageTankCompleted);
        };

        $scope.getStorageTankCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initialize();

            $scope.storageTank = response;

            $scope.storageTank =
        {
            'Id': $scope.storageTank.Id,
            'TankNo': $scope.storageTank.TankNo,
           
            'Depot': { 'Id': response.DepotId, 'Name': response.DepotName },
            'Product': { 'ProductId': response.ProductId, 'Name': response.ProductName },
            
            'Capacity': $scope.storageTank.Capacity,
            'UoMId': { 'Id': response.UoMId, 'Name': response.Measurement }
          

        };

          

            $scope.edit = true;
            $scope.add = false;

            $scope.storageTank.Header = 'Update StorageTank';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/StorageTank/ProcessStorageTank.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processStorageTank = function () {
            if ($scope.storageTank == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var previous = '';
            if ($scope.storageTank.Previous !== null && $scope.storageTank.Previous !== undefined) {
                previous = $scope.storageTank.Previous.SequenceNumber;
            }

            var req =
           {
               'Id': $scope.storageTank.Id,
               'TankNo': $scope.storageTank.TankNo,
               'DepotId': $scope.storageTank.Depot.Id,
               'ProductId': $scope.storageTank.Product.ProductId,
               'Capacity': $scope.storageTank.Capacity,
               'UoMId': $scope.storageTank.UoMId.Id
             
           };

            if ($scope.add) {
                storageTankService.addStorageTank(req, $scope.processStorageTankCompleted);
                $scope.initializeController();
            }

            else {
                storageTankService.editStorageTank(req, $scope.processStorageTankCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Storage Tanks');
            storageTankService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {

            $scope.Products = data.Products;
            $scope.Depots = data.Depots;
            $scope.Measurements = data.Measurements;
           
        };

        $scope.initialize = function () {
            $scope.storageTank =
            {
                'Id': '',
                'Name': '',
                'Group': { 'Id': '', 'Name': '-- Select Group --' },
                'ActivityTypeName': { 'Id': '', 'Name': '-- Select Activity Type --' },
                'PreviousStorageTankName': { 'Id': '', 'Name': '-- Select Previous StorageTank --' },
                'ExpectedDeliveryDuration': '',
                'Stage': {
                    'Id': '', 'Name': '-- Select Application Stage --', 'ProcesseObjects': {
                        'ImportStageName': '', 'StatusStr': '', 'Id': '', 'Name': '-- Select Process --', 'ImportStageId': '',
                        'SequenceNumber': '', 'Description': '', 'StatusId': '', 'ExpectedDeliveryDuration': '', 'ImportStageObject': '', 'StorageTankObjects': ''
                    }
                }

            };
            $scope.add = true;
            $scope.edit = false;
            $scope.storageTank.Header = 'Add StorageTank';
            $scope.buttonText = "Add";
        };

        $scope.processStorageTankCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {

                alert(data.Error);
                ngDialog.close('/App/StorageTank/ProcessStorageTank.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();

                //storageTankService.RefreshAppStage($scope.RefreshAppStageCompleted);
            }
        };

        $scope.RefreshAppStageCompleted = function (response) {
            if (response.length > 0) {
                $scope.ImportStages = response;
                storageTank.Stage.StorageTankObjects = [];
            } else {
                alert('empty');
            }
        };



        $scope.deleteStorageTank = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                storageTankService.deleteStorageTank(id, $scope.deleteStorageTankCompleted);

            }

            else {
                alert('Invalid selection.');
            }
        };


        $scope.deleteStorageTankCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);

            }
            else {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };


        $scope.getProcesses = function () {
            if ($scope.storageTank.Stage.Id == null) {
                alert('An error was encountered. Please select an Application Stage and try again.');
                return;
            }

            var req =
           {
               'ImportStageId': $scope.Stage.Id

           };

            storageTankService.getProcessesByStage(req, $scope.processProcessesByStageCompleted);



        };



    }]);

});




