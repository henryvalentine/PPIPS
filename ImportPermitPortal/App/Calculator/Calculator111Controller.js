"use strict";

define(['application-configuration', 'calculatorService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    //app.register.directive('ngCalculator', function ($compile) {
    //    return function ($scope, ngCalculator) {
    //        var tableOptions = {};
    //        tableOptions.sourceUrl = "/Calculator/GetCalculatorObjects";
    //        tableOptions.itemId = 'Id';
    //        tableOptions.columnHeaders = ['Name'];
    //        var ttc = tableManager($scope, $compile, ngCalculator, tableOptions, 'Add Calculator', 'prepareCalculatorTemplate', 'getCalculator', 'deleteCalculator', 130);
    //        ttc.removeAttr('width').attr('width', '100%');
    //        $scope.jtable = ttc;
    //    };
    //});

    app.register.controller('calculatorController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'calculatorService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, calculatorService, $upload, fileReader, $http, $location){

        $scope.prepareCalculatorTemplate = function () {
            $scope.initializeController();
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

        $scope.processCalculator = function (perm) {
            if ($scope.calculator.Name == null) {
                alert('Please provide Permit Number');
                return;
            }

            $http({ method: 'GET', url: '/Calculator/Calculator?TruckedOut=' + perm }).success(function (response) {
                if (response == null) {
                    alert('Information could not be retrieved.');
                    return;
                }
                else if (response.IsExpired === true) {
                    
                    $scope.success = 'The Permit has expired';
                    $scope.positivefeedback = true;
                }
                else if (response.IsValid === true) {

                    $scope.success = 'The Permit is Valid';
                    $scope.positivefeedback = true;
                }
                else if (response.NoEmployee === true) {
                    $scope.success = 'The Permit is not Valid';
                    $scope.positivefeedback = true;
                }
            });
            

        };

        $scope.initializeController = function () {
            $scope.calculator =
           {
               'Id': '',
               'Name': ''
              
           };
            $scope.calculator.Header = "New Calculator";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processCalculatorCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
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




