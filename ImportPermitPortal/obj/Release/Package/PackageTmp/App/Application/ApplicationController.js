"use strict";

define(['application-configuration', 'applicationService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    
    app.register.controller('applicationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'applicationService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, applicationService, $location)
    {
        $scope.getProducts = function ()
        {
            applicationService.getGenericList($scope.getProductsCompleted);

        };

        $scope.PaymentTypes = [{ 'Id': 1, 'Name': 'Online', 'Identity': 'Online' }, { 'Id': 2, 'Name': 'Bank', 'Identity': 'Bank' }];;

        $scope.checkAppType = function (appType)
        {
            if (appType == null || appType.Id < 1)
            {
                return;
            }
            $scope.appType = appType;
            $scope.initializeApp();
            $scope.application.ApplicationType = $scope.appType;
            if (appType.Id === 2 || appType.Id === 3)
            {
                $scope.showRefCode = true;
            } else {
                $scope.showRefCode = false;
         }
           
            if (($scope.application.ImportClass.Id > 0 && appType.Id < 1) || ($scope.application.ImportClass.Id < 1 && appType.Id > 0))
            {
                $scope.showProductControl = false;
                return;
            }

            if ($scope.application.ImportClass.Id > 0 && appType.Id > 0)
            {
                $scope.showProductControl = true;
                return;
            }
            
        }; 

        $scope.showAddPr = function (clss)
        {
            if (clss == null || clss.Id < 1)
            {
                return;
            }

            if ((clss.Id > 0 && $scope.application.ApplicationType.Id < 1) || (clss.Id < 1 && $scope.application.ApplicationType.Id > 0))
            {
                $scope.showProductControl = false;
                return;
            }
            if (clss.Id > 0 && $scope.application.ApplicationType.Id > 0)
            {
                $scope.showProductControl = true;
                return;
            }
        };

        $scope.getProductsCompleted = function (data)
        {
            $scope.products = data.Products;
            $scope.providerTypes = data.StorageProviderTypes;
            $scope.banks = data.Banks;
            $scope.classes = data.Classes;
            $scope.appTypes = data.ApplicationTypes;
            $scope.getPorts();
        };

        $scope.getPorts = function ()
        {
            applicationService.getPortsAndCountries($scope.getPortsCompleted);
        };

        $scope.getPortsCompleted = function (response)
        {
            $scope.depotList = response.DepotList;
            $scope.countries = response.Countries;
        };


        // depotList  appDepotList countries appCountries

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

        $scope.setSummaryError = function (msg)
        {
            $scope.summError = msg;
            $scope.isSummError = true; 
        };


        $scope.getBanksCompleted = function (data)
        {
            $scope.banks = data;
        };
        
        $scope.initializeController = function () {
           
            $rootScope.setPageTitle('Create Application|DPR-PPIPS');
            $scope.edit = false;
           
            $scope.newUser = false;
            $scope.myBankInfo = { 'BankId': '', 'Name': '-- Select Bank --' };
          
            $scope.next = false;
            $scope.processing = false;
            $scope.settingBank = false;
            $scope.pstage = false;
            $scope.bankDet = false;

            $scope.add = true;
            $scope.buttonText = "Add Product";
            $scope.products = [];
            $scope.initializeApp();
            $scope.application.Header = 'Add New Product'; 
            $scope.application.ApplicationStatusCode = 0;
            $scope.getProducts();
        };

        $scope.initializeApp = function () {
            $scope.showProductControl = false;
            $scope.application =
            {
                'Id': '', 'ImporterId': '', 'PaymentTypeId': '', 'PermitId': 0, 'DerivedTotalQUantity': '', 'InvoiceId': '', 'ClassificationId': '',
                'DerivedValue': '', 'ApplicationStatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '', 'PreviousPermitNo': '', 'ApplicationTypeId': '',
                'Header': 'New Application', 'ApplicationItemObjects': [], 
                'ApplicationType': { 'Id': '', 'Name': '--Select Application Type --' }, 'ImportClass': { 'Id': '', 'Name': '--Select Import Category--' }
            };
           
            $scope.application.Header = 'Add Product';
        };
        
        $scope.InitializeModel = function ()
        {
            $scope.ApplicationItem =
            {
                'Id': '',
                'ApplicationId': '',
                'ProductId': '',
                'DepotLicense' : '',
                'EstimatedQuantity': '',
                'StorageProviderTypeId': '',
                'EstimatedValue': '',
                'CountryOfOriginName': '',
                "DischargeDepotName": '',
                'ProductBankerName' : '',
                'PSFNumber': '',
                'ReferenceLicenseCode': '',
                'OutstandingQuantity': '',
                'ImportedQuantityValue': '',
                'DateImported': '',
                'ThroughPutObjects': [],
                'ProductBankerObjects' : [],
                'ApplicationCountryObjects': [],
                'StorageProviderTypeObject': { 'Id': '', 'Name': '-- Select Storage Option --' },
                'ProductObject': { 'ProductId': '', 'Name': '-- Select Product --' }
            };
            
            $scope.appDepotList = [];
            $scope.appCountries = [];
            $scope.appBankers = [];
        };  
       
        $scope.addNewProduct = function ()
        {
            $scope.InitializeModel();
            $scope.edit = false;
            $scope.add = true;
            $scope.buttonText = "Add";
            ngDialog.open({
                template: '/App/Application/AddProduct.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };
        
        $scope.verifyCode = function (code)
        {
            
            if ($scope.ApplicationItem.StorageProviderTypeObject.Name === 'Own Depot' || $scope.ApplicationItem.StorageProviderTypeObject.Id === 1)
            {
                if (code == null || code.length < 1)
                {
                    alert("ERROR: Please your Depot License Number.");
                    return;
                }

                $scope.depotClass = "verifying";
                var codeVrifier = { 'RefCode': code, 'LicenseType': 4, 'ImporterId': 0 }
                applicationService.VerifyDepotLicenseCode(codeVrifier, $scope.verifyCodeCompleted);
            }
            
        };

        $scope.verifyCodeCompleted = function (response)
        {
            if ($scope.ApplicationItem.StorageProviderTypeObject.Name === 'Own Depot' || $scope.ApplicationItem.StorageProviderTypeObject.Id === 1)
            {
                if (response.Code < 1)
                {
                    $scope.depotClass = "notVerified";
                    alert(response.Error);
                    return;
                }

                $scope.depotClass = "verified";
                $scope.appDepotList = [];
                angular.forEach($scope.depotList, function (c, i) 
                {
                    if (c.Id === response.Code)
                    {
                       c.ticked = true;
                        
                    }
                });
                
            }
        };
        
        $scope.verifyRefCode = function (code)
        {
            if (code == null || code.length < 1)
            {
                alert("ERROR: Please provide Application Reference Code.");
                return;
            }

            if ($scope.application.ApplicationType == null || $scope.application.ApplicationType == undefined || $scope.application.ApplicationType.Id < 1) {
                $scope.setAppError("Please select Application Type");
                return;
            }

            $scope.genClass = "verifying";

            if ($scope.application.ApplicationType.Id === 2)
            {
                applicationService.verifyAppByRefCode(code, $scope.verifyRefCodeCompleted);
            }
            
            if ($scope.application.ApplicationType.Id === 3)
            {
                applicationService.getAppForInclusion(code, $scope.verifyRefCodeCompleted);
            }
        };

        $scope.verifyRefCodeCompleted = function (response)
        {
            if (response.Code < 1)
            {
                $scope.setAppError(response.Error);
                $scope.genClass = "notVerified";
                $scope.initializeApp();
                return;
            }
            
            $scope.genClass = "verified";
            $scope.application.PermitId = response.Code;
        };

        $scope.getAppDerivedValueAndReqs = function ()
        {
            if ($scope.application.ApplicationItemObjects.length < 1)
            {
                alert("ERROR: Please add at least one Product.");
                return;
            }

            if ($scope.application.PaymentTypeId == null || $scope.application.PaymentTypeId == undefined || $scope.application.PaymentTypeId < 1) {
                alert("Please select a payment option.");
                return;
            }

            if ($scope.application.ImportClass == null || $scope.application.ImportClass == undefined || $scope.application.ImportClass.Id < 1) {
                alert("Please select Import Category");
                return;
            }

            if ($scope.application.ApplicationType == null || $scope.application.ApplicationType == undefined || $scope.application.ApplicationType.Id < 1) {
                alert("Please select Application Type");
                return;
            }
            
            $scope.totalQuantity = 0;
            angular.forEach($scope.application.ApplicationItemObjects, function (item, i)
            {
                $scope.totalQuantity = parseFloat(item.EstimatedQuantity) + $scope.totalQuantity;
            });
            
            if ($scope.totalQuantity < 1 || $scope.totalQuantity === NaN || $scope.totalQuantity == null || $scope.totalQuantity === undefined) {
                alert("ERROR: The Application process failed. Please try again.");
                return;
            }
            
            $scope.application.DerivedValue = $scope.totalQuantity;
            $scope.application.ApplicationTypeId = $scope.application.ApplicationType.Id;
            $scope.application.ClassificationId = $scope.application.ImportClass.Id;
            applicationService.calculateDerivedValue($scope.application, $scope.getAppDerivedValueAndReqsCompleted);
        };

        $scope.getAppDerivedValueAndReqsCompleted = function (response)
        {
            if (response.DerivedValue < 1 || response.FeeObjects.length < 1)
            {
                alert(response.Error);
                return;
            }

            angular.forEach($scope.PaymentTypes, function (g, i)
            {
                if ($scope.application.PaymentTypeId === g.Id)
                {
                    $scope.pOpt = g.Name;
                }
            });
            
            $scope.application.PaymentOption = $scope.pOpt;
            $scope.stageDocs = response.DocumentTypeObjects;
            $scope.application.fees = response.FeeObjects;
            $scope.application.FeeObjects = response.FeeObjects;
            $scope.application.DerivedTotalQUantity = $scope.totalQuantity;
            $scope.application.DerivedValue = response.DerivedValue;
            $scope.application.CompanyName = response.CompanyName;
            $scope.stage2();
        };

        $scope.addApp = function ()
        {
            if ($scope.ApplicationItem == null || $scope.ApplicationItem === undefined)
            {
                alert("ERROR: Please select a Product.");
                return;
            }
            

            if ($scope.ApplicationItem.ProductObject == null || parseInt($scope.ApplicationItem.ProductObject.ProductId) < 1)
            {
                alert("ERROR: Please select a Product.");
                return;
            }

            if ($scope.add)
            {
                var hit = false;
                angular.forEach($scope.application.ApplicationItemObjects, function (v, p)
                {
                    if (v.ProductId === $scope.ApplicationItem.ProductObject.ProductId)
                    {
                        hit = true;
                    }

                });

                if (hit === true) {
                    alert("ERROR: You have already added this product. Please modify as required.");
                    return;
                }
            }

            if (parseInt($scope.ApplicationItem.EstimatedQuantity) < 1)
            {
                alert("ERROR: Please provide Estimated Quantity.");
                return;
            }

            if (parseInt($scope.ApplicationItem.EstimatedValue) < 1)
            {
                alert("ERROR: Please provide Estimated Value.");
                return;
            }
           
            if ($scope.ApplicationItem.StorageProviderTypeObject == null || $scope.ApplicationItem.StorageProviderTypeObject.Id < 1) {
                alert("ERROR: Please Select Storage Option.");
                return;
            }

            if ($scope.ApplicationItem.StorageProviderTypeObject.Id === 1 || $scope.ApplicationItem.StorageProviderTypeObject.Name === 'Own Depot')
            {
                if ($scope.ApplicationItem.DepotLicense == null || $scope.ApplicationItem.DepotLicense === undefined || $scope.ApplicationItem.DepotLicense.length < 1)
                {
                    alert("ERROR: Please Provide your Depot License number.");
                    return;
                }
            }
            
            if ($scope.ApplicationItem.ProductObject.RequiresPsf !== true)
            {
                $scope.ApplicationItem.PSFNumber = "Not Applicable";
            }
            if ($scope.ApplicationItem.ProductObject.RequireReferenceCode !== true)
            {
              $scope.ApplicationItem.ReferenceLicenseCode = "Not Applicable";
            }

            if ($scope.appDepotList == null || $scope.appDepotList.length < 1)
            {
                alert("ERROR: Please select at least one Disharge Depot.");
                return;
            }

            if ($scope.appBankers == null || $scope.appBankers.length < 1) {
                alert("ERROR: Please select at least one Sposoring Bank.");
                return;
            }

            if ($scope.appCountries == null || $scope.appCountries.length < 1)
            {
                alert("ERROR: Please select Country of Origin.");
                return;
            }
            
            
            if ($scope.edit)
            {
                $scope.editProduct($scope.ApplicationItem.ProductObject.ProductId);
            }
            
            else 
            {
                var importItem =
                {
                    'Id': $scope.ApplicationItem.Id,
                    'ApplicationId': $scope.ApplicationItem.ApplicationId,
                    'ProductId': $scope.ApplicationItem.ProductObject.ProductId,
                    'EstimatedQuantity': $scope.ApplicationItem.EstimatedQuantity,
                    'PSFNumber': $scope.ApplicationItem.PSFNumber,
                    'DepotLicense' :  $scope.ApplicationItem.DepotLicense,
                    'ReferenceLicenseCode': $scope.ApplicationItem.ReferenceLicenseCode,
                    'OutstandingQuantity': $scope.ApplicationItem.EstimatedQuantity,
                    'ImportedQuantityValue': $scope.ApplicationItem.ImportedQuantityValue,
                    'DateImported': $scope.ApplicationItem.DateImported,
                    'EstimatedValue': $scope.ApplicationItem.EstimatedValue,
                    'StorageProviderTypeId': $scope.ApplicationItem.StorageProviderTypeObject.Id,
                    'CountryOfOriginName': '',
                    'DischargeDepotName': '',
                    'ThroughPutObjects': [],
                    'ProductBankerObjects' : [],
                    'ApplicationCountryObjects': [],
                    'StorageProviderTypeObject': $scope.ApplicationItem.StorageProviderTypeObject,
                    'ProductObject': $scope.ApplicationItem.ProductObject
                };
               
                angular.forEach($scope.appDepotList, function (x, y)
                {
                    var throughput = { 'Id': '', 'ApplicationItemId': '', 'DepotId': x.Id, 'ProductId': $scope.ApplicationItem.ProductObject.ProductId, 'Quantity' : 0, 'Comment' : '','DocumentId' :  '', 'IPAddress' : ''};

                    if (importItem.DischargeDepotName == null || importItem.DischargeDepotName.length < 1)
                    {
                        importItem.DischargeDepotName += x.Name;
                    }
                    else
                    {
                        importItem.DischargeDepotName += ', ' + x.Name;
                    }

                    importItem.ThroughPutObjects.push(throughput);
                });

                angular.forEach($scope.appCountries, function (u, y)
                {
                    var country = { 'Id': '', 'ApplicationItemId': '', 'CountryId': u.Id};

                    if (importItem.CountryOfOriginName == null || importItem.CountryOfOriginName.length < 1)
                    {
                        importItem.CountryOfOriginName = u.Name; 
                    }
                    else
                    {
                        importItem.CountryOfOriginName += ', ' + u.Name ;
                    }

                    importItem.ApplicationCountryObjects.push(country);
                });


                angular.forEach($scope.appBankers, function (u, y)
                {
                    var banker = { 'Id': '', 'ApplicationItemId': '', 'BankId': u.BankId, 'DocumentId': '', 'BankName2': u.Name };
                    
                    if (importItem.ProductBankerName == null || importItem.ProductBankerName.length < 1)
                    {
                        importItem.ProductBankerName = u.Name;
                    }
                    else {
                        importItem.ProductBankerName += ', ' + u.Name;
                    }

                    importItem.ProductBankerObjects.push(banker);
                });

                $scope.application.ApplicationItemObjects.push(importItem);
            }
            
            ngDialog.close('/App/Application/AddProduct.html', '');
            $scope.application.ApplicationStatusCode = 1;

            angular.forEach( $scope.depotList, function (g, p)
            {
                g.ticked = false;
            });

            angular.forEach( $scope.countries, function (g, p)
            {
                g.ticked = false;
            });

            angular.forEach($scope.appBankers, function (g, p) {
                g.ticked = false;
            });

            $scope.InitializeModel();
           
        };
        
        $scope.getProduct = function (productId)
        {
            if (parseInt(productId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.InitializeModel();

            angular.forEach($scope.application.ApplicationItemObjects, function (u, p)
            {
                if (u.ProductId === productId)
                {
                    $scope.ApplicationItem = u;
                }
            });
            
            angular.forEach($scope.depotList, function (c, i) {
                angular.forEach($scope.ApplicationItem.ThroughPutObjects, function (g, p) {
                    if (c.Id === g.DepotId) {
                        c.ticked = true;
                    }
                });
            });

            angular.forEach($scope.countries, function (c, i) {
                angular.forEach($scope.ApplicationItem.ApplicationCountryObjects, function (g, p) {
                    if (c.Id === g.CountryId) {
                        c.ticked = true;
                    }
                });
            });

            angular.forEach($scope.banks, function (c, i)
            {
                angular.forEach($scope.ApplicationItem.ProductBankerObjects, function (g, p)
                {
                    if (c.BankId === g.BankId) {
                        c.ticked = true;
                    }
                });
            });

            $scope.edit = true;
            $scope.add = false;
            $scope.application.Header = 'Update Product Information';
            $scope.buttonText = "Save";
            ngDialog.open({
                template: '/App/Application/AddProduct.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };
        
        $scope.editProduct = function (productId)
        {
            if (parseInt(productId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            angular.forEach($scope.application.ApplicationItemObjects, function (appInfo, i)
            {
                if (appInfo.ProductObject.ProductId === parseInt(productId))
                {
                    appInfo.ProductId = $scope.ApplicationItem.ProductObject.ProductId;
                    appInfo.PSFNumber = $scope.ApplicationItem.PSFNumber;
                    appInfo.EstimatedQuantity = $scope.ApplicationItem.EstimatedQuantity;
                    appInfo.EstimatedValue = $scope.ApplicationItem.EstimatedValue;
                    appInfo.ProductObject = $scope.ApplicationItem.ProductObject;
                    appInfo.StorageProviderTypeId = $scope.ApplicationItem.StorageProviderTypeObject.Id;
                    appInfo.StorageProviderTypeObject  =  $scope.ApplicationItem.StorageProviderTypeObject,
                    appInfo.ProductObject = $scope.ApplicationItem.ProductObject,
                    appInfo.CountryOfOriginName = '';
                    appInfo.DischargeDepotName = '';
                    appInfo.ProductBankerName = '';
                    appInfo.ApplicationCountryObjects = [];
                    appInfo.ProductBankerObjects = [];
                    appInfo.ThroughPutObjects = [];
                    
                    angular.forEach($scope.appDepotList, function (x, y)
                    {
                        var throughput =
                            {
                            'Id': '', 'ApplicationItemId': '', 'DepotId': x.Id, 'ProductId': $scope.ApplicationItem.ProductObject.ProductId,
                            'Quantity': 0, 'Comment': '', 'DocumentId': '', 'IPAddress': ''
                            };

                        if (appInfo.DischargeDepotName == null || appInfo.DischargeDepotName.length < 1)
                        {
                            appInfo.DischargeDepotName += x.Name;
                        }
                        else
                        {
                            appInfo.DischargeDepotName += ', ' + x.Name;
                        }

                        appInfo.ThroughPutObjects.push(throughput);
                    });

                    angular.forEach($scope.appCountries, function (u, y)
                    {
                        var country =
                            {
                                'Id': '', 'ApplicationItemId': '', 'CountryId': u.Id
                            };
                        if (appInfo.CountryOfOriginName == null || appInfo.CountryOfOriginName.length < 1)
                        {
                            appInfo.CountryOfOriginName = u.Name;
                        } else {
                            appInfo.CountryOfOriginName += ', ' + u.Name;
                        }

                        appInfo.ApplicationCountryObjects.push(country); 

                    });

                    angular.forEach($scope.appBankers, function (u, y) {
                        var banker = { 'Id': '', 'ApplicationItemId': '', 'BankId': u.BankId, 'DocumentId': '' };

                        if (appInfo.ProductBankerName == null || appInfo.ProductBankerName.length < 1) {
                            appInfo.ProductBankerName = u.Name;
                        }
                        else {
                            appInfo.ProductBankerName += ', ' + u.Name;
                        }

                        appInfo.ProductBankerObjects.push(banker);
                    });

                }
            });
        };
        
        $scope.removeApp = function (itemId)
        {
            if (itemId < 1)
            {
                alert('Invalid selection');
                return;
            }
            
            angular.forEach($scope.application.ApplicationItemObjects, function (item, index)
            {
                if (item.ProductId === itemId)
                {
                    if (!confirm("This Product information will be removed from the list. Continue?"))
                    {
                        return;
                    }

                    $scope.application.ApplicationItemObjects.splice(index, 1);
                    $scope.InitializeModel();
                }
            });
        };

        $scope.appSummary = function ()
        {
            if ($scope.application.PaymentTypeId == null || $scope.application.PaymentTypeId == undefined || $scope.application.PaymentTypeId === NaN || $scope.application.PaymentTypeId < 1) {
                $scope.setAppError("Please select a payment option.");
                return;
            }
            $scope.getAppDerivedValueAndReqs();
        };

        $scope.processAppX = function ()
        {
            var issd = false;
            angular.forEach($scope.application.ApplicationItemObjects, function (u, y) {

                angular.forEach(u.ProductBankerObjects, function (o, p) {
                    if (o.BankAccountNumber == null || o.BankAccountNumber.length < 1) {
                        issd = true;
                    }

                });
            });

            if (issd === true) {
                alert('Please provide your Bank Account(s) for the selected Sponsoring Bank(s).');
                return;
            }

            $scope.processing = true;
            applicationService.addApplication($scope.application, $scope.processApplicationCompleted);
        };
        
        $scope.processApplicationCompleted = function (data)
        {
            $scope.processing = false;
            if (data.AppId < 1)
            {
                alert(data.Error);
                return;
            }
            else
            {
                if (data.Code < 1)
                {
                    if (data.Code === -11)
                    {
                        alert(data.Error);
                        $location.path('Application/MyApplications');
                    }
                }
               
                $scope.rrr = data.Rrr;
                $scope.application.ReferenceCode = data.RefCode;
                $scope.multiplier = data.Multiplier;
                $scope.application.Id = data.AppId;

                if (data.Code > 0 && data.ErrorCode < 1)
                {
                    $scope.setSummaryError(data.Error);
                    $location.path('Application/MyApplications');
                }
              
                if (data.Code > 0 && data.ErrorCode > 0)
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
                        + '<input name="hash" id="hash" value="'+ data.Hash + '" type="hidden">'
                        + '<input name="rrr" value="' + data.Rrr + '" id="rrr" type="hidden">'
                        + '<input type="hidden" id="pType" name="paymenttype" value="' + pType + '" />'
                        + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                        + '</form>';

                    //var frmInfo = '<form action="http://www.remitademo.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                    //   + '<input name="merchantId" value="' + 2547916 + '" type="hidden">'
                    //   + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                    //   + '<input name="rrr" value="' + data.Rrr + '" id="rrr" type="hidden">'
                    //   + '<input type="hidden" id="pType" name="paymenttype" value="' + pType + '" />'
                    //   + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                    //   + '</form>';

                    $scope.frm = frmInfo;
                    $scope.frmUrl = data.RedirectUri;

                    $scope.item = $scope.application;
                    $scope.printInvoice();
                }
               
            }
        };

        $scope.setStageDocs = function ()
        {
            if ($scope.stageDocs.length < 1)
            {
                alert('An unknown error was encountered. Please refresh the page and try again');
                return;
            }

            var docStr = '<br/><h4><b>Required Documents:</b></h4> <ul>';
            angular.forEach($scope.stageDocs, function (s, m) {
                docStr += '<li><label>&#8226 &nbsp;' + s.Name + '</label></li>';
            });
            docStr += '</ul>';
            angular.element('#appDocs').html(docStr);
            $scope.summary = true;
            $scope.invoiceStage = false;
        };
        
        $scope.printInvoice = function ()
        {
            var pOpt = parseInt($scope.application.PaymentTypeId);
            if (pOpt === 2)
            {
                $scope.bankOpt = true;
                $scope.onlineOpt = false;
                $scope.success = 'Thank you for applying online. Kindly walk into any of the approved Banks to make your Application Payment using the details below:';
               
            }
            if (pOpt === 1)
            {
                $scope.success = "Thank you for applying online. Your Application details are shown below. Click the 'Continue to Payment' button to proceed with the payment process" ;
                $scope.bankOpt = false;
                $scope.onlineOpt = true;
               
            }

            $scope.positivefeedback = true;
            $scope.summary = false;
            $scope.pstage = true;
            $scope.invoiceStage = true;
        };
        
        $scope.resetSummary = function ()
         {
             $scope.summary = false;
             $scope.invoiceStage = false;
         };

        $scope.stage2 = function ()
        {
            $scope.setStageDocs();
        };

        $scope.printReceipt = function() {

            var printContents = document.getElementById('receipt').innerHTML;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('', '_blank', 'width=500,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" />' +
                    '</head><body onload="window.print()"><div class="row" style="width:95%; margin-left:3%; margin-right:2%; margin-top:5%; margin-bottom:2%"><div class="col-md-12">' + printContents + '</div></div></html>');
                popupWin.onbeforeunload = function(event) {
                    popupWin.close();
                    return '.\n';
                };
                popupWin.onabort = function(event) {
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
        
        $scope.pay = function () {
            var content = $scope.frm;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes,menubar=no,toolbar=no,location=no,status=no');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" />' +
                    '</head><body><div class="reward-body">' + content + '</div></html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }
            else {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" /></head><body>' + content + '</html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }

            $location.path('Application/MyApplications');
        };

      
    }]);
    
});


