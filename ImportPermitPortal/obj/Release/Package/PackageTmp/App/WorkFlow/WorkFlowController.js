"use strict";

define(['application-configuration', 'workFlowService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngWorkFlow', function ($compile) {
        return function ($scope, ngWorkFlow) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/WorkFlow/GetWorkFlowObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngWorkFlow, tableOptions, 'Add WorkFlow', 'prepareWorkFlowTemplate', 'getWorkFlow', 'deleteWorkFlow', 130);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('workFlowController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'workFlowService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, workFlowService, $upload, fileReader, $location) {

        $scope.prepareWorkFlowTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/WorkFlow/ProcessWorkFlow.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getWorkFlow = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
           workFlowService.getWorkFlow(impId, $scope.getWorkFlowCompleted);
        };

        $scope.getWorkFlowCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.appStage = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.appStage.Header = 'Update Application Stage';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/WorkFlow/ProcessWorkFlow.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processWorkFlow = function () {
            if ($scope.appStage == null || $scope.appStage.Name.length < 1) {
                alert('Please provide WorkFlow Name');
                return;
            }

            if ($scope.add) {
               workFlowService.addWorkFlow($scope.appStage, $scope.processWorkFlowCompleted);
            }
            else {
               workFlowService.editWorkFlow($scope.appStage, $scope.processWorkFlowCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Workflows');
            $scope.appStage =
           {
               'Id': '', 'Name': ''
           };
            $scope.appStage.Header = "New Workflow";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processWorkFlowCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/WorkFlow/ProcessWorkFlow.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteWorkFlow = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This  WorkFlow will be deleted permanently. Continue?")) {
                    return;
                }
               workFlowService.deleteWorkFlow(id, $scope.deleteWorkFlowCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteWorkFlowCompleted = function (data) {
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




