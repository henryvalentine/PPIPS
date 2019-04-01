"use strict";

define(['application-configuration', 'applicationIssueService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngAppIssue', function ($compile) {
        return function ($scope, ngAppIssue) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ApplicationIssue/GetApplicationIssueObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'CompanyName', 'IssueTypeName', 'Description', 'IssueDateStr'];

           
            var ttc = adminApplicationtableManager($scope, $compile, ngAppIssue, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('applicationIssueController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'applicationIssueService','$route','$window','$location',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http, applicationIssueService,$route,$window, $location) {
        $scope.initializeApp = function () {
            $scope.viewApp = false;
            $scope.application =
            {

                'ApplicationId': '', 'Id': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ApplicationItemObjects': []
            };



        };

        $scope.getAppView = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeApp();
            $http({ method: 'GET', url: '/ApplicationIssue/GetApplicationFromIssue?id=' + appId }).success(function (response)
            {
                if (response == null || response.ApplicationId < 1)
                {
                    alert('Application Information could not be retrieved.');
                    return;
                }
                
                $scope.application = response;
                $scope.viewApp = true;
            });

        };

        $scope.initializeReq = function () {
            $scope.process =
            {

                'Issuetype': { 'Id': '', 'Name': 'Select Issue Type' },
                'Reason': ''

            };

        };







       


          $scope.historyView = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

              $scope.myHis = false;
            $http({ method: 'GET', url: '/ProcessProfile/PreviousJobs?id=' + appId }).success(function (response) {
                if (response == null || response.ApplicationId < 1) {
                    alert('Application Information could not be retrieved.');
                    return;
                }
                $scope.application = response;

                $scope.myHis = !$scope.myHis;
                $scope.myDetail = !$scope.myDetail;
            });

        };

     
          $scope.initializeController = function () {
              $rootScope.setPageTitle('Application Issues');
            $scope.viewApp = false;
            $scope.application =
            {
                'ApplicationId': '', 'Id': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ApplicationItemObjects': []
            };

        };

    }]);

});




