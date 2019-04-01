"use strict";

define(['application-configuration', 'issueTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngIssueType', function ($compile) {
        return function ($scope, ngIssueType) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/IssueType/GetIssueTypeObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngIssueType, tableOptions, 'Add IssueType', 'prepareIssueTypeTemplate', 'getIssueType', 'deleteIssueType', 130);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('issueTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'issueTypeService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, issueTypeService, $upload, fileReader, $location) {

        $scope.prepareIssueTypeTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/IssueType/ProcessIssueType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getIssueType = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            issueTypeService.getIssueType(impId, $scope.getIssueTypeCompleted);
        };

        $scope.getIssueTypeCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.issueType = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.issueType.Header = 'Update IssueType';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/IssueType/ProcessIssueType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processIssueType = function () {
            if ($scope.issueType == null || $scope.issueType.Name.length < 1) {
                alert('Please provide IssueType Name');
                return;
            }

            var req =
       {

           'Name': $scope.issueType.Name


       };

            if ($scope.add) {
                issueTypeService.addIssueType(req, $scope.processIssueTypeCompleted);
            }
            else {
                issueTypeService.editIssueType(req, $scope.processIssueTypeCompleted);
            }

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Issue Types');
            $scope.issueType =
           {
               'Id': '',
               'Name': ''

           };
            $scope.issueType.Header = "New IssueType";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processIssueTypeCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/IssueType/ProcessIssueType.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteIssueType = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This issueType will be deleted permanently. Continue?")) {
                    return;
                } else if (confirm("This issueType will be deleted permanently. Continue?")) {
                    issueTypeService.deleteIssueType(id, $scope.deleteIssueTypeCompleted);

                }


            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteIssueTypeCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();

            }
        };

    }]);

});




