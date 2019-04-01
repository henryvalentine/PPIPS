"use strict";

define(['application-configuration', 'stepService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngAppStep', function ($compile) {
        return function ($scope, ngStep) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Step/GetStepObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'SequenceNumber', 'ProcessName', 'ImportStageName'];
            var ttc = tableManager($scope, $compile, ngStep, tableOptions, 'Add Step', 'prepareStepTemplate', 'getStep', 'deleteStep', 100);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('stepController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'stepService', '$route', '$window', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, stepService, $route, $window, $location) {

        $scope.prepareStepTemplate = function () {
            $scope.initialize();
            ngDialog.open({
                template: '/App/Step/ProcessStep.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getStep = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            stepService.getStep(impId, $scope.getStepCompleted);
        };

        $scope.getStepCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initialize();
            $scope.step = response;
            $scope.step =
            {
                'Name': response.Name,
                'Id': response.Id,
                'Duration': response.ExpectedDeliveryDurationStr,
                'LocationDependent': response.IsLocationRequired

            }

            angular.forEach($scope.groups, function (o, i) {
                if (o.Id === response.GroupId) {
                    $scope.step.Group = o;
                }
            });

            angular.forEach($scope.stepactivitytypes, function (o, i) {
                if (o.Id === response.ActivityTypeId) {
                    $scope.step.Type = o;
                }
            });

            angular.forEach($scope.ImportStages, function (o, i) {
                if (o.Id === response.ImportStageId) {
                    $scope.step.Stage = o;
                }
            });
            

            angular.forEach($scope.step.Stage.StepObjects, function (o, i)
            {
                if (o.I === response.PreviousStepId)
                {
                    $scope.step.Previous = o;
                }
            });


            angular.forEach($scope.step.Stage.ProcessObjects, function (o, i) {
                if (o.Id === response.ProcessId) {
                    $scope.step.Process = o;
                } 
            });

            $scope.edit = true;
            $scope.add = false;

            $scope.step.Header = 'Update Step';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Step/ProcessStep.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processStep = function () {
            if ($scope.step == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var previous = '';
            if ($scope.step.Previous !== null && $scope.step.Previous !== undefined) {
                previous = $scope.step.Previous.SequenceNumber;
            }

            var req =
           {
               'Id': $scope.step.Id,
               'Name': $scope.step.Name,
               'ImportStageId': $scope.step.Stage.Id,
               'ProcessId': $scope.step.Process.Id,
               'GroupId': $scope.step.Group.Id,
               'ActivityTypeId': $scope.step.Type.Id,
               'PreviousStepSequence': previous,
               'ExpectedDeliveryDuration': $scope.step.Duration,
               'IsLocationRequired': $scope.step.LocationDependent
           };

            if ($scope.add) {
                stepService.addStep(req, $scope.processStepCompleted);             
                alert('Step Successfully added');
                $scope.initializeController();
                $route.reload();
                $window.location.reload();
            }

            else {
                stepService.editStep(req, $scope.processStepCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Steps');
            stepService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {

            $scope.groups = data.Groups;
            $scope.steps = data.Steps;
            $scope.stepactivitytypes = data.StepActivityTypes;
            $scope.ImportStages = data.ImportStages;

        };

        $scope.initialize = function () {
            $scope.step =
            {
                'Id': '',
                'Name': '',
                'Group': { 'Id': '', 'Name': '-- Select Group --' },
                'ActivityTypeName': { 'Id': '', 'Name': '-- Select Activity Type --' },
                'PreviousStepName': { 'Id': '', 'Name': '-- Select Previous Step --' },
                'ExpectedDeliveryDuration': '',
                'Stage': {
                    'Id': '', 'Name': '-- Select Application Stage --', 'ProcesseObjects': {
                        'ImportStageName': '', 'StatusStr': '', 'Id': '', 'Name': '-- Select Process --', 'ImportStageId': '',
                        'SequenceNumber': '', 'Description': '', 'StatusId': '', 'ExpectedDeliveryDuration': '', 'ImportStageObject': '', 'StepObjects': ''
                    }
                }

            };
            $scope.add = true;
            $scope.edit = false;
            $scope.step.Header = 'Add Step';
            $scope.buttonText = "Add";
        };

        $scope.processStepCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {

                alert(data.Error);
                ngDialog.close('/App/Step/ProcessStep.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();

                //stepService.RefreshAppStage($scope.RefreshAppStageCompleted);
            }
        };

        $scope.RefreshAppStageCompleted = function (response) {
            if (response.length > 0) {
                $scope.ImportStages = response;
                step.Stage.StepObjects = [];
            } else {
                alert('empty');
            }
        };



        $scope.deleteStep = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                stepService.deleteStep(id, $scope.deleteStepCompleted);

            }

            else {
                alert('Invalid selection.');
            }
        };


        $scope.deleteStepCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);

            }
            else {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };


        $scope.getProcesses = function () {
            if ($scope.step.Stage.Id == null) {
                alert('An error was encountered. Please select an Application Stage and try again.');
                return;
            }

            var req =
           {
               'ImportStageId': $scope.Stage.Id

           };

            stepService.getProcessesByStage(req, $scope.processProcessesByStageCompleted);



        };



    }]);

});




