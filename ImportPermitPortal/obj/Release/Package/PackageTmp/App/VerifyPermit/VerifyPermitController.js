"use strict";

define(['application-configuration', 'verifyPermitService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    //app.register.directive('ngVerifyPermit', function ($compile) {
    //    return function ($scope, ngVerifyPermit) {
    //        var tableOptions = {};
    //        tableOptions.sourceUrl = "/VerifyPermit/GetVerifyPermitObjects";
    //        tableOptions.itemId = 'Id';
    //        tableOptions.columnHeaders = ['Name'];
    //        var ttc = tableManager($scope, $compile, ngVerifyPermit, tableOptions, 'Add VerifyPermit', 'prepareVerifyPermitTemplate', 'getVerifyPermit', 'deleteVerifyPermit', 130);
    //        ttc.removeAttr('width').attr('width', '100%');
    //        $scope.jtable = ttc;
    //    };
    //});

    app.register.controller('verifyPermitController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'verifyPermitService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, verifyPermitService, $upload, fileReader, $http, $location) {

        $scope.initializeApp = function () {
            $scope.viewApp = false;
            $scope.viewGood = false;
            $scope.viewBad = false;
            $scope.viewNever = false;
          


            $scope.application =
            {

                'NotificationId': '', 'CompanyId': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ImportItemObjects': []


            };



        };


        $scope.prepareVerifyPermitTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/VerifyPermit/ProcessVerifyPermit.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getVerifyPermit = function (impId) {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
          verifyPermitService.getVerifyPermit(impId, $scope.getVerifyPermitCompleted);
        };

        $scope.getVerifyPermitCompleted = function (response) {
            if (response.Id < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.initializeController();

            $scope.verifyPermit = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.verifyPermit.Header = 'Update VerifyPermit';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/VerifyPermit/ProcessVerifyPermit.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processVerifyPermit = function (perm) {
            if ($scope.verifyPermit.Name == null) {
                alert('Please provide Permit Number');
                return;
            }

            $http({ method: 'GET', url: '/VerifyPermit/VerifyPermit?permNo=' + perm }).success(function (response) {
                if (response == null) {
                    alert('Information could not be retrieved.');
                    return;
                }
                else if (response.IsExpired === true) {
                    
                    $scope.appError = 'The Permit has expired';
                    $scope.negativefeedback = true;
                    $scope.application = response;
                    $scope.viewApp = true;
                    $scope.viewBad = true;
                }
                else if (response.IsValid === true) {

                    $scope.success = 'The Permit is Valid';
                    $scope.positivefeedback = true;
                    $scope.application = response;
                    $scope.viewApp = true;
                    $scope.viewGood = true;
                }
                else if (response.NoEmployee === true) {
                    $scope.appError = 'The Permit does not exist';
                    $scope.negativefeedback = true;
                    $scope.viewApp = true;
                    $scope.viewNever = true;
                }
            });
            

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Verify Permit');
            $scope.verifyPermit =
           {
               'Id': '',
               'Name': ''
              
           };
            $scope.verifyPermit.Header = "New VerifyPermit";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processVerifyPermitCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            
        };

        $scope.deleteVerifyPermit = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This verifyPermit will be deleted permanently. Continue?")) {
                    return;
                } else if (confirm("This verifyPermit will be deleted permanently. Continue?")) {
                  verifyPermitService.deleteVerifyPermit(id, $scope.deleteVerifyPermitCompleted);
                   
                }
                
                
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteVerifyPermitCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                $scope.jtable.fnClearTable();
               
            }
        };

    }]);

});




