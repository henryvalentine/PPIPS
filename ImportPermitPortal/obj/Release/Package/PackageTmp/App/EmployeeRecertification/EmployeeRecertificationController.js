"use strict";

define(['application-configuration', 'employeeRecertificationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngRecert', function ($compile) {
        return function ($scope, ngRecert) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/EmployeeProfile/GetEmployeeRecertificationObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'CompanyName', 'AssignedTimeStr', 'DueTimeStr'];
            var ttc = adminApplicationtableManager($scope, $compile, ngRecert, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
           
            $scope.jtable = ttc;
        };
    });

    app.register.controller('employeeRecertificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'employeeRecertificationService','$route','$window','$location',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http, employeeRecertificationService,$route,$window, $location) {
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
            $http({ method: 'GET', url: '/EmployeeProfile/GetRecertification?id=' + appId }).success(function (response) {
                if (response == null) {
                    alert('Recertification Information could not be retrieved.');
                    return;
                }
                $scope.application = response;

                if (response.Activity === 'Review') {

                    $scope.viewR = true;

                }


                else if (response.Activity === 'Approval') {

                    $scope.viewA = true;


                }

                $scope.viewApp = true;
            });

        };


        //process review
        $scope.acceptApp = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $scope.employeeRecertification =
          {


              'Reason': ''

          };

            $scope.employeeRecertification.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeRecertification/ProcessReview.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };


        $scope.processAcceptApp = function () {



            if ($scope.employeeRecertification == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var req =
           {

               'ApplicationId': $scope.employeeRecertification.Id,

               'Description': $scope.employeeRecertification.Description


           };

            employeeRecertificationService.processAcceptApp(req, $scope.processAcceptAppCompleted);


        };


        $scope.processAcceptAppCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }
            else if (response.IsAccepted === true) {
                alert('Your application has been accepted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
                //$scope.success = 'The application has been submitted';
                //$scope.positivefeedback = true;
            }
            else if (response.IsFinal === true) {
                alert('This is the final step and the Recertification has been approved, Click to continue');
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

        };

        $scope.initializeReq = function () {
            $scope.employeeRecertification =
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

            $scope.employeeRecertification =
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

            $scope.employeeRecertification.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeRecertification/ProcessRejection.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };


        $scope.processAppRejection = function () {



            if ($scope.employeeRecertification == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

           

             var issue = '';
             if ($scope.employeeRecertification.IssueType !== null && $scope.employeeRecertification.IssueType.Id !== undefined) {
                 issue = $scope.employeeRecertification.IssueType.Id;
             }

             var req =
            {

                'ApplicationId': $scope.employeeRecertification.Id,
                'IssueTypeId': issue,
                'Description': $scope.employeeRecertification.Description


            };

             employeeRecertificationService.addIssue(req, $scope.processRejectionCompleted);


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


        //process cancel
        $scope.cancel = function () {

            ngDialog.close('/App/EmployeeProfile/ProcessReview.html', '');
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
            $rootScope.setPageTitle('Recertifications');
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




