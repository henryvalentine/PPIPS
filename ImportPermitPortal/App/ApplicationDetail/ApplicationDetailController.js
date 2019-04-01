"use strict";

define(['application-configuration', 'userAppService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.controller('appDetailController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userAppService', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, userAppService, $location, $upload, fileReader, $http)
    {
        $scope.PaymentTypes = [{ 'Id': 1, 'Name': 'Online', 'Identity': 'Online' }, { 'Id': 2, 'Name': 'Bank', 'Identity': 'Bank' }];
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Details|DPR-PPIPS');
            $scope.newUser = false;
            $scope.showx = false;
            $scope.myBankInfo = { 'BankId': '', 'Name': '-- Select Bank --' };
            $scope.myBankInfo.ContactPersonObject =
            {
                'ContactPersonId': '',
                'LastName': '-- Select Contact --',
                'Email': '',
                'PhoneNumber': '',
                'Companyd': ''
            }

            $scope.processing = false;
            $scope.settingBank = false;

            $scope.ldr = '<img src="/Images/load.GIF" style="float: left" id="loder" title="Processing File"/>';
            $scope.scs = '<img src="/Images/success.png" style="float: left" id="saveSuccess" title="File was successfully processed"/>';
            $scope.sEr = '<img src="/Images/error.png" style="float: left" id="saveError" title="File processing failed. Please try again."/>';
            
            $scope.add = true;
            $scope.buttonText = "Add Product";
            $scope.products = [];
            $scope.application =
               {
                   'Id': '', 'ImporterId': '', 'PaymentTypeId': '', 'DerivedTotalQUantity': '', 'BankId': '', 'StorageProviderTypeId': '', 'InvoiceId': '', 'ClassificationId': '',
                   'DerivedValue': '', 'ApplicationStatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '', 'PreviousPermitNo': '', 'ApplicationTypeId': '',
                   'Header': 'New Application', 'ApplicationItemObjects': [], 'BankObject': { 'BankId': '', 'SortCode': '', 'ImporterId': '', 'Name': '-- Select Bank --' },
                   'StorageProviderTypeObject': { 'Id': '', 'Name': '--Select Storage Provider Type --' }, 'ImportClass': { 'Id': '', 'Name': '--Select Import Category--' }
               };

            $scope.application = userAppService.getApp();
            if ($scope.application.Id < 1)
            {
                 $location.path('Application/MyApplications');
            }
            
            $scope.width = $scope.application.PercentageCompletion + '%';

            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            $scope.nextDocs = [];
            $scope.throughPuts = [];
            $scope.unsuppliedthroughPuts = [];
            angular.forEach($scope.application.DocumentTypeObjects, function (n, m) {
                if (n.Uploaded === true) {
                    $scope.suppliedDocs.push(n);
                }
                else {
                    if (n.StageId === 1 || n.IsDepotDoc === true) {
                        $scope.bnkDocs.push(n);
                    }
                    else {
                        $scope.nextDocs.push(n);
                    }
                }
            });

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i) {
                if (g.ThroughPutObjects !== null && g.ThroughPutObjects.length > 0) {

                    angular.forEach(g.ThroughPutObjects, function (y, p)
                    {
                        if (y.DocumentId !== null && y.DocumentId > 0)
                        {
                           y.IsUploaded = true;
                            $scope.throughPuts.push(y);
                        }

                        else
                        {
                            if (g.StorageProviderTypeId !== 1) {
                                y.IsUploaded = false;
                                $scope.unsuppliedthroughPuts.push(y);
                            }
                            
                        }
                    });
                }

            });

            $scope.refLetters = [];
            $scope.unsuppliedRefLetters = [];

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i) {
                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0) {
                    angular.forEach(g.ProductBankerObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0) {
                            y.IsUploaded = true;
                            y.DocumentPath = y.DocumentPath.replace("~", "");
                            $scope.refLetters.push(y);
                        }
                        else {
                            y.IsUploaded = false;
                            $scope.unsuppliedRefLetters.push(y);
                        }
                    });
                }

            });

            if ($scope.unsuppliedthroughPuts > 0) {
                angular.forEach($scope.application.ApplicationItemObjects, function (k, m) {
                    k.hasDocs = 0;
                    angular.forEach(unsuppliedthroughPuts, function (g, i) {
                        if (g.ApplicationItemId === k.Id) {
                            k.hasDocs += 1;
                        }
                    });
                });

            }

            if ($scope.application.ReferenceCode.length > 0 && $scope.application.ApplicationStatusCode < 2)
            {
                $scope.preparePaymentForm($scope.application);
            }

            $scope.getReqs($scope.application.ImporterId);

        };

        $scope.getReqs = function (id) {
            userAppService.getEligibility(id, $scope.getReqsCompleted);
        };

        $scope.getReqsCompleted = function (response) {
            $scope.stadDocs = [];
            if (response != null && response.length > 0)
            {
                angular.forEach(response, function (g, i)
                {
                   $scope.stadDocs.push(g);
                });
            }
        };

        $scope.updateApp = function ()
        {
            //userAppService.setApp($scope.application);
            $location.path('ApplicationDetail/BankerDetail');
        };
       
        $scope.showDoc = function (docPath)
        {
            if (docPath == null || docPath.length < 1)
            {
                return;
            }
            angular.element("#docframe").attr("src", docPath);
            $scope.showx = true;

        };

        $scope.preparePaymentForm = function (data)
        {
            var pType = '';
            angular.forEach($scope.PaymentTypes, function (k, i)
            {
                if ($scope.application.PaymentTypeId === i.Id)
                {
                    pType = k.Identity;
                }
            });

            var frmInfo = '<form action="https://login.remita.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                + '<input name="merchantId" value="' + 442773233 + '" type="hidden">'
                + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                + '<input name="rrr" value="' + data.ReferenceCode + '" id="rrr" type="hidden">'
                + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                + '</form>';


            //var frmInfo = '<form action="http://www.remitademo.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
            //       + '<input name="merchantId" value="' + 2547916 + '" type="hidden">'
            //       + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
            //       + '<input name="rrr" value="' + data.ReferenceCode + '" id="rrr" type="hidden">'
            //       + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
            //       + '</form>';



            $scope.frm = frmInfo;
            $scope.frmUrl = data.RedirectUri;
            
        };

        $scope.pay = function () {
            var content = $scope.frm;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes,menubar=no,toolbar=no,location=no,status=no');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" />' +
                    '</head><body><div class="row"><div class="col-md-12">' + content + '</div></div></html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }
            else {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" /></head><body><div class="row"><div class="col-md-12">' + content + '</div></div></html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }

            $location.path('Application/MyApplications');
        }

    }]);
    
});


