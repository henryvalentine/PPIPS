"use strict";

define(['application-configuration', 'issueService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngIssue', function ($compile)
    {
        return function ($scope, ngIssue)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Issue/GetIssueObjects";
            tableOptions.itemId = 'IssueId';
            tableOptions.columnHeaders = ['IssueCategoryName', 'AffectedCompanyName', 'ResolvedByName', 'Status'];
            var ttc = issueTableManager($scope, $compile, ngIssue, tableOptions, 'Add Issue', 'prepareIssueTemplate', 'getIssue', 'deleteIssue', 95);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('issueController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'issueService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, issueService, $location)
    {
        $scope.prepareIssueTemplate = function ()
        {
            //$scope.initializeReq();
            ngDialog.open({
                template: '/App/Issues/ProcessIssue.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getIssue = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            issueService.getIssue(impId, $scope.getIssueCompleted);
        };
        
        $scope.getIssueCompleted = function (response)
        {
            if (response.IssueId < 1)
            {
                alert('Invalid selection!');
                return;
            }

            $scope.originalIssue = response;
            $scope.issue = response;
            
            $scope.issue.Header = 'Resolve Issues|DPR-PPIPS';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Issues/ProcessIssue.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.processIssue = function ()
        {
            if ($scope.issue == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

           
            if ($scope.issue.StatusCode !== $scope.originalIssue.Status)
            {
                issueService.resolveIssue($scope.issue, $scope.processIssueCompleted);
            }
            else
            {
                ngDialog.close('/App/Issues/ProcessIssue.html', '');
            }
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Resolve Issues|DPR-PPIPS');
         };

        $scope.processIssueCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Issues/ProcessIssue.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteIssue = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                issueService.deleteIssue(id, $scope.deleteIssueCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteIssueCompleted = function (data)
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




