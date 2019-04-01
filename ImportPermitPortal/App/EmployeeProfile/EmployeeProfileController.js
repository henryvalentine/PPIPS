

"use strict";

define(['application-configuration', 'employeeProfileService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngEmp', function ($compile) {
        return function ($scope, ngEmp) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/EmployeeProfile/GetProcessTrackingObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'CompanyName', 'AssignedTimeStr', 'DueTimeStr'];
            var ttc = adminApplicationtableManager($scope, $compile, ngEmp, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');

            $scope.jtable = ttc;
        };
    });

    app.register.controller('employeeProfileController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'employeeProfileService', '$route', '$window', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http, employeeProfileService, $route, $window, $location) {
        $scope.initializeApp = function () {
            $scope.viewApp = false;
            $scope.application =
            {

                'ImportApplicationId': '', 'CompanyId': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ImportItemObjects': []
            };



        };

        
        $scope.openDoc = function (path)
        {
            if (path == null || path.length < 1)
            {
                alert('Invalid selection!');
                return;
            }
            window.open(path);
        };

        $scope.getAppView = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            employeeProfileService.getAppInfo(appId, $scope.getAppViewCompleted);
        };

        $scope.getAppViewCompleted = function (response) {

            if (response == null) {
                alert('Application Information could not be retrieved.');
                return;
            }
            $scope.initializeApp();
            $scope.application = response;

            if (response.Activity === 'Review') {
                $scope.viewR = true;
            }

            else if (response.Activity === 'Approval') {

                $scope.viewA = true;
            }

            $scope.viewApp = true;

            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            $scope.throughPuts = [];
            angular.forEach($scope.application.DocumentTypeObjects, function (n, m) {
                n.index = m + 1;
                $scope.suppliedDocs.push(n);
            });

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i) {
                if (g.ThroughPutObjects !== null && g.ThroughPutObjects.length > 0) {

                    angular.forEach(g.ThroughPutObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0) {
                            y.index = p + 1;
                            p++;
                            y.IsUploaded = true;
                            $scope.throughPuts.push(y);
                        }

                    });
                }

            });
            $scope.refLetters = [];
            angular.forEach($scope.application.ApplicationItemObjects, function (g, i) {
                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0)
                {
                    angular.forEach(g.ProductBankerObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0) {
                            y.index = p + 1;
                            p++;
                            y.IsUploaded = true;
                            y.DocumentPath = y.DocumentPath.replace("~", "");
                            $scope.refLetters.push(y);
                        }
                    });
                }

            });

            $scope.getReqs($scope.application.ImporterId);
        };

        $scope.getReqs = function (id) {
            employeeProfileService.getEligibility(id, $scope.getReqsCompleted);
        };

        $scope.getReqsCompleted = function (response) {
            $scope.stadDocs = [];
            if (response != null && response.length > 0) {
                angular.forEach(response, function (g, i) {
                    g.index = i + 1;
                    $scope.stadDocs.push(g);
                });
            }
        };

        //process review
        $scope.acceptApp = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.employee =
           {
               'Reason': ''
           };

            $scope.employee.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeProfile/ProcessReview.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processAcceptApp = function () {

            if ($scope.employee == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }
            var req =
           {

               'ApplicationId': $scope.employee.Id,

               'Description': $scope.employee.Description


           };

            employeeProfileService.processAcceptApp(req, $scope.processAcceptAppCompleted);


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
                alert('This is the final step and Application is now in Verification stage, Click to continue');
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
            else if (response.NoEmployee === true)
            {
                alert('There is no Employee to assign the task, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
           

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


        $scope.viewDocuments = function (appIdId)
        {
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
            $rootScope.setPageTitle('Application Process Tracking');
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