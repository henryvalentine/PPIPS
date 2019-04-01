"use strict";

define(['application-configuration', 'notificationCheckListService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngCheckList', function ($compile) {
        return function ($scope, ngCheckList) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/NotificationCheckList/GetNotificationCheckListObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['CheckListItem', 'CriteriaRuleStr'];
            var ttc = tableManager($scope, $compile, ngCheckList, tableOptions, 'Add NotificationCheckList', 'prepareNotificationCheckListTemplate', 'getNotificationCheckList', 'deleteNotificationCheckList', 200);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('notificationCheckListController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'notificationCheckListService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, notificationCheckListService, $location) {

        $rootScope.setPageTitle('Notification Checklist');
        $scope.prepareNotificationCheckListTemplate = function () {
            $scope.initialize();
            ngDialog.open({
                template: '/App/NotificationCheckList/ProcessNotificationCheckList.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getNotificationCheckList = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            notificationCheckListService.getNotificationCheckList(impId, $scope.getNotificationCheckListCompleted);
        };

        $scope.getNotificationCheckListCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initialize();

            $scope.notificationCheckList = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.notificationCheckList.Header = 'Update NotificationCheckList';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/NotificationCheckList/ProcessNotificationCheckList.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processNotificationCheckList = function () {
            if ($scope.notificationCheckList == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }


            var req =
           {
               'CheckListItem': $scope.notificationCheckList.CheckListItem,
               'CriteriaRuleId': $scope.notificationCheckList.CriteriaRule.Id
               //'ExpectedValue1': $scope.notificationCheckList.ExpectedValue1,
               //'ExpectedValue2': $scope.notificationCheckList.ExpectedValue2,
               //'ItemScore': $scope.notificationCheckList.ItemScore
              
           };

            if ($scope.add) {
                notificationCheckListService.addNotificationCheckList(req, $scope.processNotificationCheckListCompleted);
                $scope.initializeController();
            }

            else {
                notificationCheckListService.editNotificationCheckList(req, $scope.processNotificationCheckListCompleted);
            }

        };

        $scope.initializeController = function () {
            notificationCheckListService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {

            $scope.CriteriaRules = data.CriteriaRules;
          

        };

        $scope.initialize = function () {
            $scope.notificationCheckList =
            {
                'Id': '',
                'Name': '',
                'Group': { 'Id': '', 'Name': '-- Select Group --' },
                'ActivityTypeName': { 'Id': '', 'Name': '-- Select Activity Type --' },
                'PreviousNotificationCheckListName': { 'Id': '', 'Name': '-- Select Previous NotificationCheckList --' },
                'ExpectedDeliveryDuration': '',
                'Stage': {
                    'Id': '', 'Name': '-- Select Application Stage --', 'ProcesseObjects': {
                        'ApplicationStageName': '', 'StatusStr': '', 'Id': '', 'Name': '-- Select Process --', 'ApplicationStageId': '',
                        'SequenceNumber': '', 'Description': '', 'StatusId': '', 'ExpectedDeliveryDuration': '', 'ApplicationStageObject': '', 'NotificationCheckListObjects': ''
                    }
                }

            };
            $scope.add = true;
            $scope.edit = false;
            $scope.notificationCheckList.Header = 'Add NotificationCheckList';
            $scope.buttonText = "Add";
        };

        $scope.processNotificationCheckListCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {

                alert(data.Error);
                ngDialog.close('/App/NotificationCheckList/ProcessNotificationCheckList.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();

                //notificationCheckListService.RefreshAppStage($scope.RefreshAppStageCompleted);
            }
        };

        $scope.RefreshAppStageCompleted = function (response) {
            if (response.length > 0) {
                $scope.applicationstages = response;
                notificationCheckList.Stage.NotificationCheckListObjects = [];
            } else {
                alert('empty');
            }
        };



        $scope.deleteNotificationCheckList = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                notificationCheckListService.deleteNotificationCheckList(id, $scope.deleteNotificationCheckListCompleted);

            }

            else {
                alert('Invalid selection.');
            }
        };


        $scope.deleteNotificationCheckListCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);

            }
            else {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };


        $scope.getProcesses = function () {
            if ($scope.notificationCheckList.Stage.Id == null) {
                alert('An error was encountered. Please select an Application Stage and try again.');
                return;
            }

            var req =
           {
               'ApplicationStageId': $scope.Stage.Id

           };

            notificationCheckListService.getProcessesByStage(req, $scope.processProcessesByStageCompleted);



        };



    }]);

});




