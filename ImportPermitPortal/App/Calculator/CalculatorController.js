"use strict";

define(['application-configuration', 'calculatorService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngCalculator', function ($compile) {
        return function ($scope, ngCalculator) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/DepotTrunkedOut/GetDepotTrunkedOutObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngCalculator, tableOptions, 'Add Calculator', 'prepareCalculatorTemplate', 'getCalculator', 'deleteCalculator', 130);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('calculatorController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'calculatorService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, calculatorService, $upload, fileReader, $http, $location) {


        $scope.today = function () {
            $scope.dt = new Date();
        };
        $scope.today();

        $scope.clear = function () {
            $scope.dt = null;
        };

        // Disable weekend selection
        $scope.disabled = function (date, mode) {
            return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
        };

        $scope.toggleMin = function () {
            $scope.minDate = $scope.minDate ? null : new Date();
        };
        $scope.toggleMin();

        $scope.openInspectionDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened1 = true;
        };

        $scope.openDischargeCommencementDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened2 = true;
        };

        $scope.openDischargeCompletionDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened3 = true;
        };

        $scope.openVesselArrivalDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened4 = true;
        };




        $scope.dateOptions = {
            formatYear: 'yy',
            startingDay: 1
        };

        $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];

        $scope.initializeApp = function () {
            $scope.viewApp = false;
            $scope.viewGood = false;
            $scope.viewBad = false;
          


            $scope.application =
            {

                'NotificationId': '', 'CompanyId': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ImportItemObjects': []


            };



        };


        $scope.prepareCalculatorTemplate = function () {
            $scope.calculator =
         


            $http({ method: 'GET', url: '/DepotTrunkedOut/GetAllDepotList' }).success(function (response) {
                if (response == null) {
                    alert('Depots could not be retrieved.');
                    return;
                }
                $scope.Depots = response.Depots;

            });

            ngDialog.open({
                template: '/App/Calculator/ProcessCalculator.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getCalculator = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          calculatorService.getCalculator(impId, $scope.getCalculatorCompleted);
        };

        $scope.getCalculatorCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.calculator = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.calculator.Header = 'Update Calculator';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Calculator/ProcessCalculator.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processCalculator = function () {
            if ($scope.calculator == null) {
                alert('Please provide the information');
                return;
            }

            var req = {
              
                QuantityTrunkedOutInDepot: $scope.calculator.Out,
                TrunkedOutDate: $scope.calculator.OutDate,
                DepotId: $scope.calculator.Depot.Id

            };


            calculatorService.addCalculator(req, $scope.processCalculatorCompleted);
            

        };

        $scope.initializeController = function () {


            $http({ method: 'GET', url: '/DepotTrunkedOut/GetAllDepotList' }).success(function (response) {
                if (response == null) {
                    alert('Depots could not be retrieved.');
                    return;
                }
                $scope.Depots = response.Depots;

            });
           
        };

        $scope.processCalculatorCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }
            else if (response.Good === true) {

                $scope.success = 'The Calculator have been updated';
                $scope.positivefeedback = true;
                $scope.application = response;
                $scope.viewApp = false;
                $scope.viewGood = true;
            }
            else if (response.Bad === true) {

                $scope.appError = 'There is an issue with the network, Please try again later';
                $scope.negativefeedback = true;
                $scope.application = response;
                $scope.viewApp = false;
                $scope.viewBad = true;
            }
            
        };

        $scope.deleteCalculator = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This calculator will be deleted permanently. Continue?")) {
                    return;
                } else if (confirm("This calculator will be deleted permanently. Continue?")) {
                  calculatorService.deleteCalculator(id, $scope.deleteCalculatorCompleted);
                   
                }
                
                
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteCalculatorCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();
               
            }
        };

    }]);

});




