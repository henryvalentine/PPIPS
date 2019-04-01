"use strict";

define(['application-configuration', 'userAppService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.controller('appBankerDetailController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userAppService', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, userAppService, $location, $upload, fileReader, $http)
    {
        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 1;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDateWithWeekends($scope, '', miniDate);

        setEndDateWithWeekends($scope, miniDate, '');

        $scope.verifyCode = function (code) {

            if ($scope.ApplicationItem.StorageProviderTypeObject.Name === 'Own Depot' || $scope.ApplicationItem.StorageProviderTypeObject.Id === 1) {
                if (code == null || code.length < 1) {
                    alert("ERROR: Please your Depot License Number.");
                    return;
                }

                $scope.depotClass = "verifying";
                var codeVrifier = { 'RefCode': code, 'LicenseType': 4, 'ImporterId': 0 }
                userAppService.VerifyDepotLicenseCode(codeVrifier, $scope.verifyCodeCompleted);
            }

        };

        $scope.verifyCodeCompleted = function (response) {
            if ($scope.ApplicationItem.StorageProviderTypeObject.Name === 'Own Depot' || $scope.ApplicationItem.StorageProviderTypeObject.Id === 1) {
                if (response.Code < 1) {
                    $scope.depotClass = "notVerified";
                    alert(response.Error);
                    return;
                }

                $scope.depotClass = "verified";
                $scope.appDepotList = [];
                angular.forEach($scope.depotList, function (c, i) {
                    if (c.Id === response.Code) {
                        c.ticked = true;

                    }
                });

            }
        };
        
        $scope.getBanksCompleted = function (data)
        {
            $scope.banks = data;

            if ($scope.application.NotificationBankerObjects !== null && $scope.application.NotificationBankerObjects.length > 0)
            {
                var bnkr = $scope.application.NotificationBankerObjects[0];
                $scope.myBankInfo = { 'BankId': bnkr.BankId, 'Name': bnkr.CompanyName, 'BankBranch': bnkr.BankBranch };
                $scope.myBankInfo.ContactPersonObject =
                {
                    'ContactPersonId': bnkr.ContactPersonId,
                    'LastName': bnkr.LastName,
                    'FirstName': bnkr.FirstName,
                    'Email': bnkr.Email,
                    'PhoneNumber': bnkr.PhoneNumber,
                    'Companyd': ''
                }

                $scope.newUser = false;
                $scope.eidtUser = true;
                $scope.getBankUsers2();
            }
            
        };
        
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Completion|DPR-PPIPS');
            $scope.edit = false;
            $scope.selectedDoc = null;
            $scope.processing = false;
            $scope.pstage = false;

            $scope.ldr = '<img src="/Images/load.GIF" style="float: left" id="loder" title="Processing File"/>';
            $scope.scs = '<img src="/Images/success.png" style="float: left" id="saveSuccess" title="File was successfully processed"/>';
            $scope.sEr = '<img src="/Images/error.png" style="float: left" id="saveError" title="File processing failed. Please try again."/>';
            
            $scope.add = true;
            $scope.buttonText = "Add Product";
            
            var applicationId = $routeParams.id;
            if (applicationId < 1)
            {
                alert('An error was encountered on the page. Please try again later.');
                $location.path('Application/MyApplications');
            }

            $scope.getApp(applicationId);
        };
       
        $scope.getApp = function (appId)
        {
            userAppService.getImportAppDocsX(appId, $scope.getAppCompleted);
        };

        $scope.updateBankAccounts = function ()
        {
            var bankers = [];
            var error = 0;
            angular.forEach($scope.application.ApplicationItemObjects, function (u, y)
            {
                angular.forEach(u.ProductBankerObjects, function (o, p)
                {
                    if (o.BankAccountNumber == null || o.BankAccountNumber.length < 1)
                    {
                        error += 1;
                    }
                    else
                    {
                        bankers.push(o);
                    }
                    
                });
            });

            if (error > 0)
            {
                alert('Please provide your Bank Account(s) for all the selected Sponsoring Bank(s).');
                return;
            } else {
                userAppService.updateBankAccounts(bankers, $scope.updateBankAccountsCompleted);
            }
           
        };

        $scope.updateBankAccountsCompleted = function (response) 
        {
            angular.forEach($scope.application.ApplicationItemObjects, function (u, y)
            {
                var bankName = '';
                angular.forEach(u.ProductBankerObjects, function (o, p)
                {
                    if (bankName == null || bankName.length < 1) {
                        bankName = o.BankName2 + "(" + o.BankAccountNumber + ")";

                    }
                    else {
                        bankName.ProductBankerName += ', ' + o.BankName2 + "(" + o.BankAccountNumber + ")";
                    }
                    u.ProductBankerName = bankName;
                });
            });

            alert(response.Error);
            return;
        };
        
        $scope.getAppCompleted = function (application)
        {
            if (application.Id < 1)
            {
                alert('An error was encountered. The Application Infromation could not be retrieved.');
                $location.path('Application/MyApplications');
            }
            
            $scope.application = application;
            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            $scope.nextDocs = [];
            $scope.throughPuts = [];
            $scope.unsuppliedthroughPuts = [];
            $scope.IsThrouputhAvailable = false;
            $scope.newDpocList = [];
            $scope.throughPut = {};
            $scope.taxClrnc = {};
            $scope.getProducts();
            
        angular.forEach($scope.application.DocumentTypeObjects, function (n, m)
        {
            if (n.Uploaded === true)
            {
                n.index = m + 1;
                $scope.suppliedDocs.push(n);
            }
            else
            {
                if (n.StageId === 1 || n.IsDepotDoc === true)
                {
                    if (n.DocumentTypeId === 25)
                    {
                        $scope.throughPut = n; 
                    }
                    else
                    {
                        $scope.bnkDocs.push(n);
                    }
                }
                else
                {
                    $scope.nextDocs.push(n);
                }
            }
        });

        angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
        {
            angular.forEach(g.ThroughPutObjects, function (y, p)
            {
                if (y.DocumentId !== null && y.DocumentId > 0)
                {
                    y.index = p + 1;
                    p++;
                    y.IsUploaded = true;
                    $scope.throughPuts.push(y);
                }

                else
                {
                    if (g.StorageProviderTypeId !== 1)
                    {
                        y.IsUploaded = false;
                        $scope.unsuppliedthroughPuts.push(y);
                    }
                }
            });
        });
          $scope.refLetters = [];
          $scope.unsuppliedRefLetters = [];

          angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
          {
                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0)
                {
                    angular.forEach(g.ProductBankerObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0)
                        {
                            y.index =p + 1;
                            p++;
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

          if ($scope.unsuppliedthroughPuts.length > 0)
          {
            angular.forEach($scope.application.ApplicationItemObjects, function (k, m)
            {
                k.hasDocs = 0;
                angular.forEach($scope.unsuppliedthroughPuts, function (g, i)
                {
                    if (g.ApplicationItemId === k.Id)
                    {
                        k.hasDocs += 1;
                    }
                });
            });
              
          }
            
           $scope.doc =
           {
               'Id': '',
               'ImporterId': '',
               'StandardRequirementTypeId': '',
               'TempPath': '',
               'DocumentPath': '',
               'ValidFrom': '',
               'ValidTo': '',
               'LastUpdated': '',
               'IsOpened': false,
               'IsExpOpened' : false,
               'Title': ''
           };
            
            $scope.getReqs($scope.application.ImporterId);
        };

        $scope.getReqs = function (id)
        {
            userAppService.getEligibility(id, $scope.getReqsCompleted);
        };

        $scope.getReqsCompleted = function (response)
        {
            $scope.stadDocs = [];
            if (response != null && response.length > 0)
            {
                angular.forEach(response, function (n, i)
                {
                    if (n.Id === 3)
                    {
                        $scope.taxClrnc = n;
                    }
                    else
                    {
                        n.className = 't' + n.DocumentTypeId;
                        $scope.stadDocs.push(n);
                    }
                });
            }
        };

        $scope.InitializeTphF = function ()
        {
            $scope.tphF =
            {
                'Id': '',
                'DepotId': '',
                'ProductName': '',
                'ProductId': '',
                'Quantity': '',
                'DocumentId': '',
                'IPAddress': '',
                'Comment': '',
                'ApplicationItemId': '',
            };
        };
        
        $scope.open = function ($event, elementOpened)
        {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.doc[elementOpened] = !$scope.doc[elementOpened];
        };

        $scope.editDoc = function (docId)
        {
            if (docId == null || docId < 1)
            {
                alert('Invalid selection!');
                return;
            }

            $scope.selectedDoc = {};

            angular.forEach($scope.suppliedDocs, function (n, m)
            {
                if (n.DocumentId === docId) {
                    $scope.selectedDoc = n;
                    $scope.bnkDocs.push(n);
                    $scope.suppliedDocs.splice(m, 1);
                }

            });

            $scope.numberCollection();
        };
        
        $scope.numberCollection = function ()
        {
            angular.forEach($scope.suppliedDocs, function (n, m)
            {
                n.index = m + 1;
            });

            angular.forEach($scope.throughPuts, function (y, m)
            {
                y.index = m + 1;
                m++;
            });

            angular.forEach($scope.refLetters, function (y, m) {
                y.index = m + 1;
                m++;
            });
        };

        $scope.ProcessDocument = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                return;
            }
            var file = el.files[0];
            var doctTypeId = el.id.split('j')[0];
            if (doctTypeId < 1)
            {
                alert('There is an error on the page. Please refresh the page and try again.');
                return;
            }

            var divf = 'infoDiv' + doctTypeId;
            var infoDiv = angular.element('#' + divf);
            var url = '';
            var isupdate = false;
            if ($scope.selectedDoc != null)
            {
                if ($scope.selectedDoc.DocumentId > 0)
                {
                    url = "/Document/UpdateStageFile?documentId=" + $scope.selectedDoc.DocumentId;
                    isupdate = true;
                }
            }
            else
            {
                url = "/Document/SaveStageFile?docTypeId=" + doctTypeId + '&&importAppId=' + $scope.application.Id;
            }
           
            $upload.upload({
                url: url,
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt)
           {
               infoDiv.html($scope.ldr);
           }).success(function (data)
           {
               if (data.code < 1)
               {
                   infoDiv.html($scope.sEr);
               }

               else
               {
                   infoDiv.html($scope.scs);

                   if (isupdate === true)
                   {
                       $scope.selectedDoc.DocumentPath = data.Path;
                       $scope.suppliedDocs.push($scope.selectedDoc);
                       u.Uploaded = true;

                       angular.forEach($scope.bnkDocs, function (n, m)
                       {
                           if (n.DocumentTypeId === doctTypeId)
                           {
                               $scope.bnkDocs.splice(m, 1);
                               $scope.selectedDoc = null;
                           }
                       });
                   }
                   else
                   {
                       angular.forEach($scope.bnkDocs, function (u, t)
                       {
                           if (u.DocumentTypeId === doctTypeId)
                           {
                               u.DocumentId = data.Code;
                               u.Uploaded = true;
                               u.DocumentPath = data.Path;
                               $scope.suppliedDocs.push(u);
                               $scope.bnkDocs.splice(t, 1);
                           }
                       });
                   }
                   $scope.numberCollection();
               }

           });


        };

        $scope.checkAppSubmit = function () {

            if (!$scope.accept)
            {
                alert('Please check the Declaration Acceptance option.');
                return;
            }

            var isQualified = true;

            angular.forEach($scope.stadDocs, function (n, i)
            {
                if (n.StandardRequirementId < 1 || n.IsUploaded === false)
                {
                    isQualified = false;
                }
               
            });

            if (isQualified === false)
            {
                alert('The requirents for this Application are yet to be completely met.');
                return;
            }

            userAppService.checkAppSubmit($scope.application.Id, $scope.checkAppSubmitCompleted);
        };

        $scope.checkAppSubmitCompleted = function (response)
        {
            if (response.IsRequirementsMet === true)
            {
                ngDialog.open({
                    template: '/App/ApplicationDetail/ConfirmSubmit.html',
                    className: 'ngdialog-theme-flat',
                    scope: $scope
                });
                return;
            }

            alert('The requirents for this Application are yet to be completely met.');
            return;
        };

        $scope.appSubmit = function ()
        {
            userAppService.submitApp($scope.application.Id, $scope.appSubmitCompleted);
        };

        $scope.appSubmitCompleted = function (response)
        {
            if (response.Code > 0)
            {
                $scope.application.StatusCode = response.Code;
                alert('Application has been submitted successfully.');
                ngDialog.close('/App/ApplicationDetail/ConfirmSubmit.html', '');
                $location.path('/Application/MyApplications');
            }
            alert('Application could not be submitted. Please try again later.');
            return;
        };

        $scope.submitDoc = function () {
            if ($scope.doc == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.doc.StandardRequirementTypeId < 1) {
                alert('Request could not be processed. Please try again.');
                return;
            }

            if ($scope.doc.Title == null || $scope.doc.Title.length < 1) {
                alert('Request could not be processed. Please try again.');
                return;
            }

            if ($scope.doc.ValidFrom == null || $scope.doc.ValidFrom.length < 1) {
                alert('Please provide Date Obtained.');
                return;
            }

            if ($scope.doc.ValidTo == null || $scope.doc.ValidTo.length < 1) {
                alert('Please provide Expiry Date for the Tax Clearance.');
                return;
            }

            var doc =
            {
                'Id': 0,
                'ImporterId': 0,
                'StandardRequirementTypeId': $scope.doc.StandardRequirementTypeId,
                'DocumentPath': $scope.doc.DocumentPath,
                'ValidFrom': $scope.doc.ValidFrom,
                'ValidTo': $scope.doc.ValidTo,
                'LastUpdated': '',
                'Title': $scope.doc.Title,
                'TempPath': $scope.doc.TempPath
            };

            $scope.processing = true;
            userAppService.addCompanyDocument(doc, $scope.submitTaxCompleted);
        };

        $scope.submitTax = function ()
        {
            if ($scope.doc == null || $scope.taxClrnc == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.taxClrnc.DocumentTypeId < 1)
            {
                alert('Request could not be processed. Please try again.');
                return;
            }

            if ($scope.taxClrnc.Name == null || $scope.taxClrnc.Name.length < 1)
            {
                alert('Request could not be processed. Please try again.');
                return;
            }

            if ($scope.doc.ValidFrom == null || $scope.doc.ValidFrom.length < 1)
            {
                alert('Please provide Date Obtained.');
                return;
            }

            if ($scope.doc.ValidTo == null || $scope.doc.ValidTo.length < 1)
            {
                alert('Please provide Expiry Date for the Tax Clearance.');
                return;
            }

            var doc =
            {
                'Id': 0,
                'ImporterId': 0,
                'StandardRequirementTypeId': $scope.taxClrnc.Id,
                'DocumentPath': $scope.doc.DocumentPath,
                'ValidFrom': $scope.doc.ValidFrom,
                'ValidTo': $scope.doc.ValidTo,
                'LastUpdated': '',
                'Title': $scope.taxClrnc.Name,
                'TempPath': $scope.doc.TempPath
            };

            $scope.processing = true;
            userAppService.addCompanyDocument(doc, $scope.submitTaxCompleted);
        };

        $scope.submitTaxCompleted = function (data)
        {
            $scope.processing = false;
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else
            {
                alert(data.Error);
                $scope.taxClrnc.StandardRequirementId = data.Code;
                $scope.taxClrnc.IsUploaded = true;
                $scope.taxClrnc.DocumentPath = $scope.doc.TempPath;
            }
        };

        $scope.saveTaxDoc = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];
            $scope.processing = true;
            $upload.upload({
                url: "/CompanyDocument/SaveTempFile",
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt) {
               $scope.trendy = 'verifying';
           }).success(function (data) {
               $scope.processing = false;
               if (data.Code < 1) {
                   $scope.trendy = 'notVerified';
               }

               else {
                   $scope.trendy = 'verified';
                   $scope.doc.TempPath = data.Path;
               }

           });


        };

        $scope.ProcessDoc = function (e, d)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                return;
            }
            var file = el.files[0];
         
            var name = el.name;
            if (name == null || name.length < 1)
            {
                alert('Error: request could not be processed. Please refresh the page and try again.');
                return;
            }

            $scope.stD = d;
            $scope.processing = true;
           d.className  = 'verifying';
            $upload.upload({
                url: "/CompanyDocument/SaveTempFile",
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt)
           {
               
           }).success(function (data)
           {
               if (data.Code < 1)
               {
                   d.className = 'notVerified';
                   $scope.processing = false;
               }

               else
               {
                   $scope.xclassName = d.className;
                   var doc =
                   {
                       'Id': 0,
                       'ImporterId': 0,
                       'StandardRequirementTypeId': d.Id,
                       'DocumentPath': '',
                       'ValidFrom': '',
                       'ValidTo': '',
                       'LastUpdated': '',
                       'Title': name,
                       'TempPath': data.Path
                   };

                   userAppService.addCompanyDoc(doc, $scope.ProcessDocCompleted);
               }

           });
        };

        $scope.ProcessDocCompleted = function (data)
        {
            $scope.processing = false;
           
            if (data.Code < 1)
            {
                alert(data.Error);
                $scope.xclassName = 'notVerified';
            }
            else {
                alert(data.Error);
                $scope.xclassName = 'verified';

                $scope.stD.StandardRequirementId = data.Code;
                $scope.stD.DocumentPath = data.Path;
                $scope.stD.IsUploaded =true;
            }
        };
        
        $scope.getTh = function (thId)
        {
            if (thId == null || thId < 1)
            {
                alert('Invalid selection!');
                return;
            }

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
            {
                angular.forEach(g.ThroughPutObjects, function (y, p)
                {
                    if (y.Id === thId && y.DocumentId > 0)
                    {
                        y.IsUploaded = false;
                        $scope.throughPuts.splice(p, 1);
                        $scope.unsuppliedthroughPuts.push(y);
                    }
                });
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
            $scope.numberCollection();
        };

        $scope.getStd = function (d)
        {
            if (d == null || d.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }

            d.IsUploaded = false;
        };

        $scope.submitThroughput = function(th) {
            if (th.DocumentId == null) {
                if (th.file == null || th.file.size < 1) {
                    alert('Please select Throughput file!');
                    return;
                }
            }

            if (th.Id < 1) {
                alert('Error : Invalid operation detected. Please refresh the page and try again.');
                return;
            }

            if (th.Quantity == null || th.Quantity < 1) {
                alert('Please provide the aggreed Throughput Quantity!');
                return;
            }


            $scope.tphF = th;
            var url = '';
            if (th.DocumentId == null) {
                url = "/ThroughPut/AddThroughPutByApplicant?throughPutId=" + th.Id + '&quantity=' + th.Quantity;
            } else {
                url = "/ThroughPut/EditThroughPut?throughPutId=" + th.Id + '&documentId=' + th.DocumentId + '&quantity=' + th.Quantity;
            }

            $scope.processing = true;

            $upload.upload({
                    url: url,
                    method: "POST",
                    data: { file: th.file }
                })
                .progress(function(evt) {
                    $rootScope.busy = true;
                }).success(function(data) {
                    $scope.processing = false;
                    $scope.submitThroughputComplete(data);

                });
        };

        $scope.submitThroughputComplete = function (response)
        {
            $rootScope.busy = false;
            if (response.Code < 1)
            {
                alert(response.Error);
                return;
            }

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
            {
                angular.forEach(g.ThroughPutObjects, function (y, p)
                {
                    if ($scope.tphF.Id === y.Id)
                    {
                        if ($scope.tphF.file != null && $scope.tphF.file.size > 1)
                        {
                            y.IsUploaded = true;
                            y.Status = 1;
                            y.DocumentId = response.Code;
                            y.DocumentPath = response.Error;
                            y.StatusStr = 'Pending';
                        }

                        $scope.throughPuts.push(y);
                        $scope.unsuppliedthroughPuts.splice(p, 1);
                    }
                });
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

            $scope.numberCollection();

        };

        $scope.ProcessThroughput = function (e, d)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                return;
            }
            var file = el.files[0];
            if (d.Id < 1)
            {
                alert('There is an error on the page. Please refresh the page and try again.');
                return;
            }
            d.file = file;
        };
        

        //Update Depot and Bankers info for selected Products

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
                    
                    angular.forEach($scope.providerTypes, function (c, i)
                    {
                        if (c.Id === u.StorageProviderTypeId)
                        {
                            $scope.ApplicationItem.StorageProviderTypeObject = c;
                        }
                    });
                }
            });

            angular.forEach($scope.depotList, function (c, i)
            {
                angular.forEach($scope.ApplicationItem.ThroughPutObjects, function (g, p)
                {
                    if (c.Id === g.DepotId)
                    {
                        c.ticked = true;
                    }
                });
            });
            

            //angular.forEach($scope.countries, function (c, i)
            //{
            //    angular.forEach($scope.ApplicationItem.ApplicationCountryObjects, function (g, p)
            //    {
            //        if (c.Id === g.CountryId) {
            //            c.ticked = true;
            //        }
            //    });
            //});

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
            $scope.application.Header = 'Update Product Information';
            $scope.buttonText = "Save";
            ngDialog.open({
                template: '/App/ApplicationDetail/EditProduct.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.editProduct = function ()
        {
            
            if ($scope.ApplicationItem == null || $scope.ApplicationItem === undefined) {
                alert("ERROR: Please select a Product.");
                return;
            }
            
            if ($scope.ApplicationItem.StorageProviderTypeObject == null || $scope.ApplicationItem.StorageProviderTypeObject.Id < 1) {
                alert("ERROR: Please Select Storage Option.");
                return;
            }

            if ($scope.ApplicationItem.StorageProviderTypeObject.Id === 1 || $scope.ApplicationItem.StorageProviderTypeObject.Name === 'Own Depot') {
                if ($scope.ApplicationItem.DepotLicense == null || $scope.ApplicationItem.DepotLicense === undefined || $scope.ApplicationItem.DepotLicense.length < 1) {
                    alert("ERROR: Please Provide your Depot License number.");
                    return;
                }
            }

            if ($scope.appDepotList == null || $scope.appDepotList.length < 1) {
                alert("ERROR: Please select at least one Disharge Depot.");
                return;
            }

            if ($scope.appBankers == null || $scope.appBankers.length < 1) {
                alert("ERROR: Please select at least one Sposoring Bank.");
                return;
            }
            
            angular.forEach($scope.application.ApplicationItemObjects, function (appInfo, i)
            {
                if (appInfo.Id === $scope.ApplicationItem.Id)
                {
                    appInfo.StorageProviderTypeId = $scope.ApplicationItem.StorageProviderTypeObject.Id;
                    appInfo.StorageProviderTypeObject = $scope.ApplicationItem.StorageProviderTypeObject,
                    appInfo.ProductObject = $scope.ApplicationItem.ProductObject,
                    appInfo.CountryOfOriginName = '';
                    appInfo.DischargeDepotName = '';
                    appInfo.ProductBankerName = '';
                    appInfo.ApplicationCountryObjects = [];
                    appInfo.ProductBankerObjects = [];
                    appInfo.ThroughPutObjects = [];

                    angular.forEach($scope.appDepotList, function (x, y)
                    {
                       
                        if (appInfo.DischargeDepotName == null || appInfo.DischargeDepotName.length < 1)
                        {
                            appInfo.DischargeDepotName += x.Name;
                        }
                        else
                        {
                            appInfo.DischargeDepotName += ', ' + x.Name;
                        }

                        if (!$scope.itemExists(appInfo.ThroughPutObjects, 'ProductId', $scope.ApplicationItem.ProductObject.ProductId))
                        {
                            var throughput =
                           {
                               'Id': '',
                               'ApplicationItemId': $scope.ApplicationItem.Id,
                               'DepotId': x.Id,
                               'ProductId': $scope.ApplicationItem.ProductObject.ProductId,
                               'Quantity': 0,
                               'Comment': '',
                               'DocumentId': '',
                               'IPAddress': ''
                           };
                            appInfo.ThroughPutObjects.push(throughput);
                        }
                       
                    });

                    angular.forEach($scope.appBankers, function (u, y)
                    {
                        if (appInfo.ProductBankerName == null || appInfo.ProductBankerName.length < 1)
                        {
                            appInfo.ProductBankerName = u.Name;
                        }
                        else
                        {
                            appInfo.ProductBankerName += ', ' + u.Name;
                        }
                        
                        if (!$scope.itemExists(appInfo.ProductBankerObjects, 'BankId', u.BankId))
                        {
                            var banker = { 'Id': '', 'ApplicationItemId': $scope.ApplicationItem.Id, 'BankId': u.BankId, 'DocumentId': '' };
                            appInfo.ProductBankerObjects.push(banker);
                        }
                    });

                    $scope.removeIfNotExists(appInfo.ProductBankerObjects, $scope.appBankers, 'BankId');
                    $scope.removeIfNotExists(appInfo.ApplicationCountryObjects, $scope.appBankers, 'CountryId');
                    $scope.removeIfNotExists(appInfo.ThroughPutObjects, $scope.appDepotList, 'ProductId');

                    userAppService.editApplication($scope.application, $scope.updateApplicationCompleted);
                }
            });
           
        };

        $scope.updateApplicationCompleted = function (data)
        {
            alert(data.Error);
            if (data.Code < 1)
            {
                return;
            }
            ngDialog.close('/App/ApplicationDetail/EditProduct.html', '');
            $scope.getApp($scope.application.Id);
        };


        $scope.getProductsCompleted = function (data)
        {

            ngDialog.close('/App/Application/EditProduct.html', '');

            angular.forEach($scope.depotList, function (g, p) {
                g.ticked = false;
            });

            angular.forEach($scope.countries, function (g, p) {
                g.ticked = false;
            });

            angular.forEach($scope.appBankers, function (g, p) {
                g.ticked = false;
            });

            $scope.InitializeModel();
        };

        $scope.itemExists = function (array, itemProp, value)
        {
            var xists = false;
            angular.forEach(array, function (x, y)
            {
                if (x.itemProp === value)
                {
                    xists = true;
                }
            });
            return xists;
        };

        $scope.removeIfNotExists = function (originalArray, newArray, prop)
        {
            angular.forEach(originalArray, function (x, y)
            {
                var xists = false;
                angular.forEach(newArray, function (o, i)
                {
                    if (x.prop === o.prop)
                    {
                        xists = true;
                    }

                });

                if (!xists)
                {
                    if (confirm('Please not that any document already attached to the deselected item(s) will be deleted. Continue?')) {
                        originalArray.splice(y, 1);
                    }
                    
                }
            });
           
        };

        $scope.InitializeModel = function () {
            $scope.ApplicationItem =
            {
                'Id': '',
                'ApplicationId': '',
                'ProductId': '',
                'DepotLicense': '',
                'EstimatedQuantity': '',
                'StorageProviderTypeId': '',
                'EstimatedValue': '',
                'CountryOfOriginName': '',
                "DischargeDepotName": '',
                'ProductBankerName': '',
                'PSFNumber': '',
                'ReferenceLicenseCode': '',
                'OutstandingQuantity': '',
                'ImportedQuantityValue': '',
                'DateImported': '',
                'ThroughPutObjects': [],
                'ProductBankerObjects': [],
                'ApplicationCountryObjects': [],
                'StorageProviderTypeObject': { 'Id': '', 'Name': '-- Select Storage Option --' },
                'ProductObject': { 'ProductId': '', 'Name': '-- Select Product --' }
            };

            $scope.appDepotList = [];
            $scope.appCountries = [];
            $scope.appBankers = [];
        };


        //Get Generic Lists

        $scope.getProductsCompleted = function (data)
        {
            $scope.providerTypes = data.StorageProviderTypes;
            $scope.banks = data.Banks;
            $scope.getPorts();
        };

        $scope.getPorts = function () {
            userAppService.getPortsAndCountries($scope.getPortsCompleted);
        };

        $scope.getProducts = function () {
            userAppService.getRunList($scope.getProductsCompleted);

        };

        $scope.getPortsCompleted = function (response) {
            $scope.depotList = response.DepotList;
            $scope.countries = response.Countries;
        };

    }]);
    
});


