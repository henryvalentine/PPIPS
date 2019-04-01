"use strict";

define(['application-configuration', 'depotOwnerService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{

    app.register.directive("ngBnkDocs", function ($compile)
    {
        return function ($scope, ngBnkDocs)
        {
            $scope.ckxv = $compile;
            $scope.bnkUsrDoc = ngBnkDocs;
            
        }
    });
    
    app.register.controller('processDocumentController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'depotOwnerService', '$upload', '$location', '$timeout', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, depotOwnerService, $upload, $location, $timeout, fileReader)
    {
        $scope.initializeController = function () {
            $rootScope.setPageTitle('Throughput|DPR-PPIPS');
            $scope.applicationItem = depotOwnerService.getApp();
            if ($scope.applicationItem.ID < 1) {
                alert('The selected product information is empty. Please try again.');
                $scope.goToItemList();
            }
            if ($scope.applicationItem.ThroughPutObjects != null && $scope.applicationItem.ThroughPutObjects.length > 0)
            {
                var item = $scope.applicationItem.ThroughPutObjects[0];
                if (item.Status === 0 || item.Status === 6)
                {
                    $scope.disableControls = false;
                    $scope.hideSubmit = false;
                } else {
                    $scope.disableControls = true;
                    $scope.hideSubmit = true;
                }

                $scope.throughput = $scope.applicationItem.ThroughPutObjects[0];
            }
            else {
                $scope.disableControls = true;
                $scope.hideSubmit = true;
                var doc =
                   {
                       'ImporterId': '',
                       'DateUploaded': '',
                       'UploadedById': '',
                       'DocumentPath': '',
                       'DocumentTypeId': '',
                       'Status': '',
                       'IpAddress': ''
                   };

                $scope.throughput =
                    {
                        'Id': '',
                        'DocumentObject': doc,
                        'ProductId': '',
                        'Quantity': '',
                        'ApplicationItemId': '',
                        'DepotId': '',
                        'DocumentId': '',
                        'Comment': '',
                        'IPAddress': '',
                        'TempPath' : ''
                    };
            }

        };

        $scope.goToItemList = function ()
        {
            $location.path('DepotOwner/DepotOwner');
        };

        $scope.setAppSuccess = function (msg)
        {
            $scope.isError = false;
            $scope.appError = '';
            $scope.appSuccess = msg;
            $scope.isSuccess = true;
        };

        $scope.setAppError = function (msg)
        {
            $scope.appError = msg;
            $scope.isError = true;
            $scope.isSuccess = false;
            $scope.appSuccess = '';
        };

        $scope.hideFeedBack = function ()
        {
            $timeout(function () {
                if ($scope.isError === true || $scope.isSuccess === true)
                {
                    $scope.isError = false;
                    $scope.isSuccess = false;
                    $scope.appSuccess = '';
                    $scope.appError = '';
                }

            }, 7000);
        };
        
        $scope.processThroughput = function ()
        {
            if ($scope.finDocSession == null || $scope.finDocSession < 1)
            {
                $scope.setAppError("ERROR: Please attach Throughput Document first.");
                $scope.hideFeedBack();
                return;
            }

            if ($scope.throughput == null)
            {
                $scope.setAppError("ERROR: Please refresh the page and try again.");
                $scope.hideFeedBack();
                return;
            }

            if ($scope.throughput.Quantity == null || $scope.throughput.Quantity < 1)
            {
                $scope.setAppError("ERROR: Please provide Quantity.");
                $scope.hideFeedBack();
                return;
            }

            $scope.throughput.ApplicationItemId = $scope.applicationItem.Id;
            $scope.throughput.DepotId = $scope.applicationItem.DishargeDepotId;
            $scope.throughput.ProductId = $scope.applicationItem.ProductId;
            
            if ($scope.throughput.Id == null || $scope.throughput.Id == undefined || $scope.throughput.Id === NaN || $scope.throughput.Id < 1)
            {
                depotOwnerService.addthroughPut($scope.throughput, $scope.processThroughputCompleted);
            } else {
                depotOwnerService.updatethroughPut($scope.throughput, $scope.processThroughputCompleted);
            }
        };
        
        $scope.processThroughputCompleted = function (response)
        {
            if (response.Code < 1)
            {
                $scope.setAppError(response.Error);
                $scope.hideFeedBack();
                return;
            }
            
            $scope.setAppSuccess(response.Error);
            $timeout(function () {
                $scope.goToItemList();
            }, 2000);
        };
        
        $scope.saveDocToSession = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null && el.files.length < 1)
            {
                return;
            }
            $scope.processinLt = true;
           var finDoc = el.files[0];
           $scope.genClass = "verifying";
            $upload.upload({
                url: "/ThroughPut/SaveTempFile",
                method: "POST",
                data: { file: finDoc }
            })
             .progress(function (evt)
             {

             }).success(function (data)
             {
                 $scope.processinLt = false;
                 if (data.Code < 1)
                 {
                     $scope.finDocSession = 0;
                     $scope.genClass = "notVerified";
                     $scope.setAppError('Document could not be processed. Please try again.');
                     $scope.hideFeedBack();
                     return;
                 }
                 $scope.throughput.TempPath = data.Path;
                 $scope.finDocSession = 5;
                 $scope.genClass = "verified";
             });
        };
      
    }]);

});




