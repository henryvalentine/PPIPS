"use strict";

define(['application-configuration', 'processService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngAppProcess', function ($compile) {
        return function ($scope, ngProcess) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Process/GetProcessObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'ImportStageName'];
            var ttc = tableManager($scope, $compile, ngProcess, tableOptions, 'Add Process', 'prepareProcessTemplate', 'getProcess', 'deleteProcess', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('processController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'processService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, processService, $location) {

        $scope.prepareProcessTemplate = function () {
            $scope.initialize();
            ngDialog.open({
                template: '/App/Process/ProcessProcess.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getProcess = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            processService.getProcess(impId, $scope.getProcessCompleted);
        };

        $scope.getProcessCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initialize();

            $scope.process = response;

            $scope.process =
{
    'Id': $scope.process.Id,
    'Name': $scope.process.Name,
    'ImportStage': { 'Id': response.ImportStageId, 'ImportStageName': response.ImportStage }



};

            $scope.edit = true;
            $scope.add = false;

            $scope.process.Header = 'Update Process';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Process/ProcessProcess.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processProcess = function () {
            if ($scope.process == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req =
           {
               'Id': $scope.process.Id,
               'Name': $scope.process.Name,
               'ImportStageId': $scope.process.ImportStage.Id
              
           };

            if ($scope.add) {
                processService.addProcess(req, $scope.processProcessCompleted);
            }
            else {
                processService.editProcess(req, $scope.processProcessCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Processes');
            processService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {
            $scope.ImportStages = data.ImportStages;
           

        };

        $scope.initialize = function () {
            $scope.process =
            {
                'Id': '',
                'Name': '',
                'ImportStage': { 'Id': '', 'Name': 'Select Application Stage' }
                
            };
            $scope.add = true;
            $scope.edit = false;
            $scope.process.Header = 'Add Process';
            $scope.buttonText = "Add";
        };

        $scope.processProcessCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Process/ProcessProcess.html', '');
                $scope.jtable.fnClearTable();
                $scope.initialize();
            }
        };

        $scope.deleteProcess = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                processService.deleteProcess(id, $scope.deleteProcessCompleted);

            }
           
            else {
                alert('Invalid selection.');
            }
        };


      

        $scope.deleteProcessCompleted = function (data) {
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




