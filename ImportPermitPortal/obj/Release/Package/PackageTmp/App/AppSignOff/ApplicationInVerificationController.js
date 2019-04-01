"use strict";

define(['application-configuration', 'adminApplicationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngApps', function ($compile)
    {
        return function ($scope, ngApps)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetApplicationInVerification";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'ImporterStr', 'DerivedQuantityStr', 'DerivedValue', 'DateAppliedStr', 'StatusStr'];
            var ttc = applicationsInVerificationTableManager($scope, $compile, ngApps, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('applicationsInVerificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'adminApplicationService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http,adminApplicationService, $location)
    {
        $scope.getAppVerifiers = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            $scope.selId = appId;
            adminApplicationService.getAppEmployees(appId, $scope.getAppVerifiersCompleted);
        };

        $scope.signOff = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            adminApplicationService.signOff(appId, $scope.signOffCompleted);
        };

        $scope.signOffCompleted = function (response)
        {
            alert(response.Error);
            if (response.Code < 1)
            {
                return;
            }

            $scope.jtable.fnClearTable();
        };

        $scope.getAppVerifiersCompleted = function (response)
        {
            if (response.Code < 1)
            {
                alert('Application information could not be retrieved.');
                return;
            }
            
            $scope.appInfoObj = response;
            $scope.viewVerifiers = true;
        };

        $scope.closeVerifierView = function ()
        {
            $scope.appInfoObj = null;
            $scope.viewVerifiers = false;
        };
        
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Applications in Verification|DPR-PPIPS');
            $scope.viewVerifiers = false;
            $scope.application =
               {
                   'Id': '', 'ImporterId': '', 'PaymentTypeId': '', 'DerivedTotalQUantity': '', 'BankId': '', 'StorageProviderTypeId': '', 'InvoiceId': '', 'ClassificationId': '',
                   'DerivedValue': '', 'ApplicationStatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '', 'PreviousPermitNo': '', 'ApplicationTypeId': '',
                   'Header': 'New Application', 'ApplicationItemObjects': [], 'BankObject': { 'BankId': '', 'SortCode': '', 'ImporterId': '', 'Name': '-- Select Bank --' },
                   'StorageProviderTypeObject': { 'Id': '', 'Name': '--Select Storage Provider Type --' }, 'ImportClass': { 'Id': '', 'Name': '--Select Import Category--' }
               };

        };
        
        $scope.ProcessVerifierFile = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                alert('Please select a file');
                return;
            }
            if ($scope.selId === null || $scope.selId == undefined || $scope.selId < 1)
            {
                alert('Processed failed. Please try again later.');
                return;
            }

            var file = el.files[0];
           
            $upload.upload({
                url: "/Document/UploadSignOffDocument?applicationId=" + $scope.selId,
                method: "POST",
                data: { file: file}
            })
           .progress(function (evt)
           {
               $rootScope.busy = true;
           }).success(function (data)
           {
               $rootScope.busy = false;
               alert(data.Error);
               if (data.Code < 1)
               {
                   return;
               }
               
               $scope.viewVerifiers = false;
               $scope.jtable.fnClearTable();
               $scope.appInfoObj = null;

           });
        };
         
         $scope.printVerifiers = function () {

             var printContents = document.getElementById('verifiersView').innerHTML;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('', '_blank', 'width=500,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" />' +
                    '</head><body onload="window.print()"><div class="row" style="width:95%; margin-left:3%; margin-right:2%; margin-top:5%; margin-bottom:2%"><div class="col-md-12">' + printContents + '</div></div></html>');
                popupWin.onbeforeunload = function (event) {
                    popupWin.close();
                    return '.\n';
                };
                popupWin.onabort = function (event) {
                    popupWin.document.close();
                    popupWin.close();
                }
            } else {
                popupWin = window.open('', '_blank', 'width=800,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" /></head><body onload="window.print()"><div class="row" style="width:95%; margin-left:3%; margin-right:2%; margin-top:5%; margin-bottom:2%"><div class="col-md-12">' + printContents + '</div></div></html>');
                popupWin.document.close();
            }
            popupWin.document.close();

            return true;
        };
    }]);

});




