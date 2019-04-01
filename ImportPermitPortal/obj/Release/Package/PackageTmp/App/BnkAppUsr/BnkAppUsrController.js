"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{

    app.register.directive("ngBnkDocs", function ($compile)
    {
        return function ($scope, ngBnkDocs)
        {
            $scope.ckxv = $compile;
            $scope.bnkUsrDoc = ngBnkDocs;
            
        }
    });

    app.register.directive('ngBankUserApps', function ($compile)
    {
        return function ($scope, ngBankUserApps)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Application/GetBankAssignedApplications";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'ImporterStr', 'AppTypeStr', 'ImportClassName', 'DerivedQuantityStr', 'LastModifiedStr', 'StatusStr'];
            var ttc = bankUserApplicationtableManager($scope, $compile, ngBankUserApps, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    
    app.register.controller('bnkAppUsrController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$upload', '$location', '$timeout', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $upload, $location, $timeout, fileReader)
    {
        $scope.initializeController = function ()
        {
          $scope.viewApp = false;
            $rootScope.setPageTitle('Application Support|DPR-PPIPS');
            $scope.selectedDoc = null;
            $scope.gettingApp = false;
            $scope.ldr = '<img src="/Images/load.GIF" style="float: left" id="loder" title="Processing File"/>';
            $scope.scs = '<img src="/Images/success.png" style="float: left" id="saveSuccess" title="File was successfully processed"/>';
            $scope.sEr = '<img src="/Images/error.png" style="float: left" id="saveError" title="File processing failed. Please try again."/>';
          
        };

        $scope.initializeApp = function () 
        {
            $scope.viewApp = false;
            $scope.application = {};
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
       
        $scope.hideFeedBack = function () {
            $timeout(function () {
                if ($scope.isError === true || $scope.isSuccess === true) {
                    $scope.isError = false;
                    $scope.isSuccess = false;
                    $scope.appSuccess = '';
                    $scope.appError = '';
                }

            }, 7000);
        };

        $scope.getAppByReference = function (e)
        {
            var el = (e.srcElement || e.target);

            $scope.referenceCode = el.id;

            if ($scope.referenceCode == null || $scope.referenceCode.length < 1)
            {
                alert('Invalid selection.');
                return;
            }

            $scope.gettingApp = true;

            bnkAdminService.searchApp($scope.referenceCode.trim(), $scope.getAppDocsCompleted);
        };

        $scope.getAppInfo = function (appId)
        {
            if (appId < 1)
            {
                alert('Invalid selection.');
                return;
            }
            $scope.gettingApp = true;
            bnkAdminService.getHistoryAppDocuments(appId, $scope.getAppDocsCompleted);

        };

        $scope.viewDocuments = function (appId)
        {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            bnkAdminService.setId(appId);
            $location.path('/Application/UserDocuments');
        };

        $scope.numberCollection = function ()
        {
            angular.forEach($scope.productDocuments, function (y, m)
            {
                y.index = m + 1;
            });
        };
        
        $scope.getAppDocsCompleted = function (data)
        {
            if (data.Id == null || data.Id < 1)
            {
                alert("ERROR: The Application information could not be retrieved.");
                return;
            }

            $scope.gettingApp = false;
            $scope.productDocuments = [];
            $scope.unsuppliedProductDocuments = [];

            $scope.application = data;
            $scope.products = [];

            angular.forEach(data.ApplicationItemObjects, function (g, m)
            {
                $scope.products.push(g.ProductObject);

                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0)
                {
                    angular.forEach(g.ProductBankerObjects, function (y, p)
                    {
                        if (y.DocumentId !== null && y.DocumentId > 0)
                        {
                            y.index = p + 1;
                            y.IsUploaded = true;
                            $scope.productDocuments.push(y);
                        }

                        else {
                            y.IsUploaded = false;
                            $scope.unsuppliedProductDocuments.push(y);
                        } 
                    });
                }

            });

            //$scope.ckxv(angular.element('#tng').html())($scope);
            $scope.viewApp = true;
        };
        
        $scope.getTlx = function (id)
        {
            if (id == null || id < 1)
            {
                alert('Invalid selection!');
                return;
            }

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
            {
                angular.forEach(g.ProductBankerObjects, function (y, p)
                {
                    if (y.Id === id && y.DocumentId > 0)
                    {
                        d.IsUploaded = false;
                        $scope.unsuppliedProductDocuments.push(y);
                        $scope.productDocuments.splice(p, 1);
                    }
                });
            });

            $scope.numberCollection();
        };

        $scope.ProcessDocument = function (e, d)
        {

            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                return;
            }

            if (d == null || d.Id < 1)
            {
                alert('An error was encountered on the page. Please refresh the page and try again.');
                return;
            }

            var file = el.files[0];
            if (file.size > 4096000) {
                alert('File size must not exceed 4MB');
                return;
            }
            var url = '';
            if (d.DocumentId === null)
            {
                url = "/Document/SaveRefLetter?applicationItemId=" + d.ApplicationItemId + '&applicationId=' + $scope.application.Id + '&bankId=' + d.BankId;
            }
            else
            {
                url = "/Document/UpdateRefLetter?documentId=" + d.DocumentId;
            }

            $upload.upload({
                url: url,
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt) {
               $scope.processing = true;
               d.status = 'verifying';
           }).success(function (data)
           {
               $scope.processing = false;
               if (data.Code < 1)
               {
                   d.status = 'notVerified';
               }

               else {
                   
                   d.status = 'verified';
                   d.DocumentPath = data.Path;
                   d.DocumentId = data.Code;

                   angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
                   {
                       angular.forEach(g.ProductBankerObjects, function (y, p)
                       {
                           if (d.Id === y.Id)
                           {
                               d.IsUploaded = true;
                               d.DocumentStatus = 'Pending';
                               d.Status = 1;
                               $scope.productDocuments.push(y);
                               $scope.unsuppliedProductDocuments.splice(p, 1);
                           }
                       });
                   });

                   $scope.numberCollection();
               }

           });


        };
        
    }]);

});




