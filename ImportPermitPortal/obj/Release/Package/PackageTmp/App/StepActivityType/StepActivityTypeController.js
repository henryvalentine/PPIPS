"use strict";

define(['application-configuration', 'stepActivityTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngStepActivityType', function ($compile) {
        return function ($scope, ngStepActivityType) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/StepActivityType/GetStepActivityTypeObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngStepActivityType, tableOptions, 'Add Activity Type', 'prepareStepActivityTypeTemplate', 'getStepActivityType', 'deleteStepActivityType', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('stepActivityTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'stepActivityTypeService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, stepActivityTypeService, $upload, fileReader, $location) {

        $scope.prepareStepActivityTypeTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/StepActivityType/ProcessStepActivityType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getStepActivityType = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          stepActivityTypeService.getStepActivityType(impId, $scope.getStepActivityTypeCompleted);
        };

        $scope.getStepActivityTypeCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.stepActivityType = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.stepActivityType.Header = 'Update Step Activity Type';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/StepActivityType/ProcessStepActivityType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processStepActivityType = function () {
            if ($scope.stepActivityType == null || $scope.stepActivityType.Name.length < 1) {
                alert('Please provide StepActivityType Name');
                return;
            }

            var req =
       {
          
           'Name': $scope.stepActivityType.Name
         

       };

            if ($scope.add) {
              stepActivityTypeService.addStepActivityType(req, $scope.processStepActivityTypeCompleted);
            }
            else {
              stepActivityTypeService.editStepActivityType(req, $scope.processStepActivityTypeCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Step Activities');
            $scope.stepActivityType =
           {
               'Id': '',
               'Name': ''
              
           };
            $scope.stepActivityType.Header = "New StepActivityType";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processStepActivityTypeCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/StepActivityType/ProcessStepActivityType.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteStepActivityType = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This stepActivityType will be deleted permanently. Continue?")) {
                    return;
                } else if (confirm("This stepActivityType will be deleted permanently. Continue?")) {
                  stepActivityTypeService.deleteStepActivityType(id, $scope.deleteStepActivityTypeCompleted);
                   
                }
                
                
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteStepActivityTypeCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();
               
            }
        };

    }]);

});




