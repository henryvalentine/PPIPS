"use strict";

define(['application-configuration', 'issueCategoryService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngIssCategory', function ($compile)
    {
        return function ($scope, ngIssCategory)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/IssueCategory/GetIssueCategoryObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngIssCategory, tableOptions, 'Add Issue Category', 'prepareIssueCategoryTemplate', 'getIssueCategory', 'deleteIssueCategory', 165);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('issueCategoryController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'issueCategoryService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, issueCategoryService, $location)
    {
        $scope.prepareIssueCategoryTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/IssueCategories/ProcessIssueCategory.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getIssueCategory = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            issueCategoryService.getIssueCategory(impId, $scope.getIssueCategoryCompleted);
        };

        $scope.getIssueCategoryCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.issueCategory = response;

            $scope.issueCategory =
            {
                'Id': $scope.issueCategory.Id,
                'Name': $scope.issueCategory.Name

            };

            $scope.edit = true;
            $scope.add = false;

            $scope.issueCategory.Header = 'Update Issue Category';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/IssueCategories/ProcessIssueCategory.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processIssueCategory = function ()
        {
            if ($scope.issueCategory == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req = {
                Id: $scope.issueCategory.Id,
                Name: $scope.issueCategory.Name
            };
            
            if ($scope.add)
            {
                issueCategoryService.addIssueCategory(req, $scope.processIssueCategoryCompleted);
            }
            else
            {
               issueCategoryService.editIssueCategory(req, $scope.processIssueCategoryCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Issue Categories|DPR-PPIPS');

        };

        $scope.initializeReq = function ()
        {
           $scope.issueCategory =
           {
               'Id': '',
               'Name': ''
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.issueCategory.Header = 'Add Issue Category';
            $scope.buttonText = "Add";
        };

        $scope.processIssueCategoryCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/IssueCategories/ProcessIssueCategory.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteIssueCategory = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                issueCategoryService.deleteIssueCategory(id, $scope.deleteIssueCategoryCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteIssueCategoryCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);

            }
            else
            {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };
     
    }]);

});







