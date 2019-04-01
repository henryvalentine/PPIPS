"use strict";

define(['application-configuration', 'employeeProfileService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngEmp', function ($compile) {
        return function ($scope, ngEmp) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/EmployeeProfile/GetProcessTrackingObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'AssignedTimeStr', 'DueTimeStr'];

            var ttc = adminApplicationtableManager($scope, $compile, ngEmp, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.error = 'You have no Job';
            $scope.negativefeedback = true;
            $scope.jtable = ttc;
        };
    });

    app.register.controller('employeeProfileController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'employeeProfileService','$route','$window','$location',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http, employeeProfileService,$route,$window, $location) {
        $scope.initializeApp = function () {
            $scope.viewApp = false;
            $scope.application =
            {

                'ImportApplicationId': '', 'CompanyId': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ImportItemObjects': []
            };



        };

        $scope.getAppView = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeApp();
            $http({ method: 'GET', url: '/EmployeeProfile/GetApplication?id=' + appId }).success(function (response) {
                if (response == null || response.ImportApplicationId < 1) {
                    alert('Application Information could not be retrieved.');
                    return;
                }
                $scope.application = response;

                $scope.viewApp = true;
            });

        };

        $scope.initializeReq = function () {
            $scope.employee =
            {

                'Issuetype': { 'Id': '', 'Name': 'Select Issue Type' },
                'Reason': ''

            };

        };
        
        $scope.rejectApp = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.employee =
            {

                'Name': { 'Id': '', 'Name': 'Select Issue Type' },
                'Reason': ''

            };

            $http({ method: 'GET', url: '/EmployeeProfile/GetGenericList' }).success(function (response) {
                if (response == null) {
                    alert('Issue Types could not be retrieved.');
                    return;
                }
                $scope.issuetypes = response.IssueTypes;

            });

            $scope.employee.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeProfile/ProcessRejection.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };


        $scope.processAppRejection = function () {

            if ($scope.employee == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

             var issue = '';
             if ($scope.employee.IssueType !== null && $scope.employee.IssueType.Id !== undefined) {
                 issue = $scope.employee.IssueType.Id;
             }

             var req =
            {

                'ApplicationId': $scope.employee.Id,
                'IssueTypeId': issue,
                'Description': $scope.employee.Description


            };

             employeeProfileService.addIssue(req, $scope.processRejectionCompleted);


        };


        $scope.processRejectionCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }
            else if (response.IsAccepted === true) {
                alert('Your comment has been noted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            
        };




        $scope.acceptApp = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

           
            $http({ method: 'GET', url: '/EmployeeProfile/AcceptApp?id=' + appId }).success(function (response) {
                if (response == null || response.IsError ===  true) {
                    alert('There is a problem with the network.');
                    return;
                }
                else if (response.IsAccepted === true) {
                    alert('Your application has been accepted, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();
                }
                else if (response.IsFinal === true) {
                    alert('This is the final step and permit has been generated, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();
                }
                else if (response.CantGeneratePermit === true) {
                    alert('This is the final step but permit is not generated, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();
                }
                else if (response.NoEmployee === true) {
                    alert('There is no Employee to assign the task, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();
                }
               
            });

        };


          $scope.historyView = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

              $scope.myHis = false;
            $http({ method: 'GET', url: '/EmployeeProfile/PreviousJobs?id=' + appId }).success(function (response) {
                if (response == null || response.ImportApplicationId < 1) {
                    alert('Application Information could not be retrieved.');
                    return;
                }
                $scope.application = response;

                $scope.myHis = !$scope.myHis;
                $scope.myDetail = !$scope.myDetail;
            });

        };




        $scope.viewDocuments = function (appIdId) {
            if (parseInt(appIdId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $http({ method: 'GET', url: '/Document/SetDocSession?id=' + appIdId }).success(function (response) {
                $scope.viewDocumentsCompleted(response);
            });
        };

        $scope.viewDocumentsCompleted = function (code) {
            if (parseInt(code) < 1) {
                alert('An unknown error was encountered. Please try again');
                return;
            }

            $location.path('/AppDocs/AppDocs');
        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('My Tasks');
            $scope.viewApp = false;
            $scope.application =
            {
                'ImportApplicationId': '', 'CompanyId': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ImportItemObjects': []
            };

        };





    }]);

});




