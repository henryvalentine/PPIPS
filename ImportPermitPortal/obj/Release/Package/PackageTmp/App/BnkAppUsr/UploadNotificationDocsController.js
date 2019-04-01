"use strict";

define(['application-configuration', 'bnkNotificationService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.controller('bnkUsereNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkNotificationService', '$location', '$upload', '$timeout', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkNotificationService, $location, $upload, $timeout, fileReader, $http)
    {

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
                if (n.DocumentId === docId)
                {
                    $scope.selectedDoc = n;
                    $scope.bnkDocs.push(n);
                    $scope.suppliedDocs.splice(m, 1);
                }
                
            });

            $scope.numberCollection();

        };

        $scope.initializeBanker = function ()
        {
            $scope.fnLt = {};
            $scope.fnltList = [];
          $scope.appBanker =
          {
              'FinancedQuantity': '',
              'ProductId': '',
              'ActualQuantity': '',
              'TransactionAmount': '',
              'ApplicationId': '',
              'BankId': '',
              'FinLetterPath': ''
          };

        };

        $scope.initializeController = function ()  
        {
            $rootScope.setPageTitle('Notification Support|DPR-PPIPS');
          
            $scope.IsformMProvided = true;
            $scope.next = false;
            $scope.processing = false;
            $scope.pstage = false;
            $scope.editFnlt = true;

            $scope.ldr = '<img src="/Images/load.GIF" style="float: left" id="loder" title="Processing File"/>';
            $scope.scs = '<img src="/Images/success.png" style="float: left" id="saveSuccess" title="File was successfully processed"/>';
            $scope.sEr = '<img src="/Images/error.png" style="float: left" id="saveError" title="File processing failed. Please try again."/>';

            var refNumber = $routeParams.id;

            if (refNumber == null || refNumber.length < 1) 
            {
                alert('Notification Query tag could not be retrieved. Please reselect the item and try again.');
                return;
            }

            $scope.getNotificationByReference(refNumber);
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

        $scope.numberCollection = function ()
        {
            angular.forEach($scope.suppliedDocs, function (n, m)
            {
                n.index = m + 1;
            });
        };

        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() - 1;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day ;
        setMaxDate($scope, miniDate, new Date());
        
        $scope.getAppBankerInfo = function (productId, permitValue)
        {
            bnkNotificationService.getAppBanker(productId, permitValue, $scope.getAppBankerInfoCompleted);
        };

        $scope.getAppBankerInfoCompleted = function (data)
        {
            $scope.initializeBanker();
            
            $scope.editFnlt = true;
           
            if (data !== null && data !== undefined && data.Id > 0)
            {
                $scope.appBanker = data;
                $scope.fnLt = { 'DocumentId': data.AttachedDocumentId, 'DocumentPath': data.FinLetterPath }; 
            } 
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

        $scope.editFinLtDoc = function (fnlt)
        {
            if (fnlt === null) {
                alert('An unknown error was encountered. Please try again later.');
                return;
            }
            $scope.fnltList = [];
            $scope.fnltList.push(fnlt);
            $scope.editFnlt = true;
            $scope.fnLt = null;
        };

        $scope.getProducts = function () {
            bnkAdminService.getProducts($scope.getProductsCompleted);

        };

        $scope.getProductsCompleted = function (data)
        {
            $scope.products = data;
        };
        
        $scope.ProcessDocument = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];
            if (file.size > 4096000)
            {
                alert('File size must not exceed 4MB');
                return;
            }
            var doctTypeId = el.id.split('j')[0];
            if (doctTypeId < 1) {
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
            }  //AddNotificationDocument
            else
            {
                url = "/Document/SaveNotificationFile?docTypeId=" + doctTypeId + '&notificationId=' + $scope.notification.Id;
            }

            $upload.upload({
                url: url,
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt) {
               infoDiv.html($scope.ldr);
           }).success(function (data) {
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
                       $scope.selectedDoc.Uploaded = true;
                       $scope.suppliedDocs.push($scope.selectedDoc);

                       angular.forEach($scope.bnkDocs, function (n, m)
                       {
                           if (n.DocumentTypeId === $scope.selectedDoc.DocumentTypeId)
                           {
                             $scope.bnkDocs.splice(m, 1);
                               $scope.selectedDoc = null;
                           }
                       });
                   }
                   else
                   {
                       angular.forEach($scope.bnkDocs, function (n, m)
                       {
                           if (n.DocumentTypeId === doctTypeId)
                           {
                               n.DocumentId = data.Code;
                               n.Uploaded = true;
                               n.DocumentPath = data.Path;
                               $scope.suppliedDocs.push(n);
                               $scope.bnkDocs.splice(m, 1);

                           }
                       });
                   }
                   $scope.numberCollection();
               }

           });


        };

        $scope.getNotificationByReference = function (referenceCode)
        {
          bnkNotificationService.searchNotification(referenceCode.trim(), $scope.getNotificationByReferenceCompleted);
        };

        $scope.getNotificationByReferenceCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('The Notification you are looking for could not be found.');
                return;
            }

            $scope.notification =
           {
               'Id': 0,
               'ReferenceCode': '',
               'NotificationTypeId': '',
               'ApplicationQuantity': '',
               'QuantityImported': '',
               'ApplicationId': '',
               'PermitId': '',
               'PortOfOriginId': '',
               'DischargeJettyId': '',
               'ProductId': '',
               'QuantityToDischarge': '',
               'QuantityOnVessel': '',
               'CargoInformationTypeId': '',
               'ArrivalDate': '',
               'DischargeDate': '',
               'AmountDue': '',
               'DateCreated': ''
           };

            $scope.notification = response;
            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            //ng-if="response.Status != 3 && response.Status != 5 && response.Status != 6 && response.Status != 7 && response.Status != 8 && response.Status != 9"

            if (response.Status !== 3 && response.Status !== 5 && response.Status !== 6 && response.Status !== 7 && response.Status !== 8 && response.Status !== 9) {
                $scope.isLegit = true;
            } else {
                $scope.isLegit = false;
            }

            angular.forEach(response.DocumentTypeObjects, function (n, m)
            {
                if (n.IsFormM === true)
                {
                    $scope.formM = n;
                }
                else
                {
                    if (n.Uploaded === true)
                    {
                        n.index = m + 1;
                        $scope.suppliedDocs.push(n);
                    }
                    else {
                        if (n.IsBankDoc === true) {
                            $scope.bnkDocs.push(n);
                        }
                    }
                }
            });

            $scope.initializeFormM();
            if ($scope.notification.FormMDetailObjects != null && $scope.notification.FormMDetailObjects.length > 0)
            {
                $scope.formMBtnText = 'Update Form M Details';
                $scope.formM = $scope.notification.FormMDetailObjects[0];
                $scope.formM.DateIssued = $scope.formM.DateIssuedStr;
            }

             $scope.initializeBanker();

            if ($scope.notification.NotificationBankerObjects != null && $scope.notification.NotificationBankerObjects.length > 0)
            {
                var appBanker = $scope.notification.NotificationBankerObjects[0];
                $scope.getAppBankerInfoCompleted(appBanker);
            }
            
            $scope.permitValue = response.PermitValue;
            $scope.notification.ArrivalDate = app.ArrivalDateStr;
            $scope.notification.DischargeDate = app.DischargeDateStr;
            $scope.viewApp = true;
            //$scope.getAppBankerInfo($scope.notification.ProductId, $scope.permitValue);
        };

        $scope.numberCollection = function () {
            angular.forEach($scope.suppliedDocs, function (n, m) {
                n.index = m + 1;
            });
        };

        $scope.processAppBankerInfo = function ()
        {
            if ($scope.appBanker.Id == null || $scope.appBanker.Id == undefined || $scope.appBanker.Id === NaN || $scope.appBanker.Id < 1)
            {
                if ($scope.finDocSession == null || $scope.finDocSession < 1) {
                    $scope.setAppError("ERROR: Please attach Letter of Financing first.");
                    $scope.hideFeedBack();
                    return;
                }
            }
            
            if ($scope.notification == null)
            {
                $scope.setAppError("ERROR: Please refresh the page and try again.");
                $scope.hideFeedBack();
                return;
            }

            if ($scope.appBanker == null)
            {
                $scope.setAppError("ERROR: Please refresh the page and try again.");
                $scope.hideFeedBack();
                return;
            }
            
            if ($scope.appBanker.FinancedQuantity == null || $scope.appBanker.FinancedQuantity < 1) {
                $scope.setAppError("ERROR: Please provide Financed Quantity.");
                $scope.hideFeedBack();
                return;
            }
           
            if ($scope.appBanker.TransactionAmount == null || $scope.appBanker.TransactionAmount < 1) {
                $scope.setAppError("ERROR: Please provide Transaction Amount.");
                $scope.hideFeedBack();
                return;
            }

            $scope.appBanker.BankId = $scope.notification.BankId;
            $scope.appBanker.ActualQuantity = $scope.notification.QuantityToDischarge;
            $scope.appBanker.ApplicationId = $scope.notification.ApplicationId;
            $scope.appBanker.NotificationId = $scope.notification.Id;
            $scope.appBanker.ImporterId = $scope.notification.ImporterId;
            $scope.appBanker.ProductId = $scope.notification.ProductId;

            if ($scope.appBanker.Id == null || $scope.appBanker.Id == undefined || $scope.appBanker.Id === NaN || $scope.appBanker.Id < 1)
            {
                bnkNotificationService.addAppBanker($scope.appBanker, $scope.processAppBankerInfoCompleted);
            }

            else
            {
                bnkNotificationService.updateAppBanker($scope.appBanker, $scope.processAppBankerInfoCompleted);
            }
        };

        $scope.processAppBankerInfoCompleted = function (response)
        {
            if (response.Code < 1)
            {
                alert(response.Error);
                return;
            }

            $scope.fnLt.DocumentPath = response.Path;
            $scope.appBanker.Id = response.Code;
            alert(response.Error);
           
        };
        
        $scope.saveDocToSession = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null && el.files.length < 1)
            {
                return;
            }
            $scope.processinLt = true;
            $scope.finDocSession = 0;
            var finDoc = el.files[0];
            $scope.genClass = "verifying";

            var url = '';
            var fnltIsupdate = false;
            if ($scope.fnltList !== null && $scope.fnltList.length > 0)
            {
                if ($scope.fnltList[0].DocumentId > 0 && $scope.fnltList[0].DocumentPath != null && $scope.fnltList[0].DocumentPath.length > 0)
                {
                    url = "/Application/UpdateFinLtFile";
                    fnltIsupdate = true;
                }
               
            }
            else
            {
                url = "/Document/SaveTempFinLtFile";
            }

            $upload.upload({
                url: url,
                method: "POST",
                data: { file: finDoc }
            }).progress(function (evt)
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

                if (fnltIsupdate === true)
                {
                    $scope.fnLt = $scope.fnltList[0];
                    $scope.fnLt.DocumentPath = data.Path;
                    $scope.fnLt.index = $scope.suppliedDocs.length + 1;
                    $scope.editFnlt = false;
                    alert(data.Error);
                }
                else
                {
                    $scope.appBanker.FinLetterPath = data.Path;
                    $scope.finDocSession = 5;
                }

                $scope.genClass = "verified";
            });
        };


        //FORM M PAROLES

        $scope.initializeFormM = function ()
        {
            $scope.formM =
                {
                    'Id': 0,
                    'NotificationId': $scope.notification.Id,
                    'DateIssued': '',
                    'FormMReference': '',
                    'Quantity': '',
                    'LetterOfCreditNo': '',
                    'AttachedDocumentId': 0,
                    'DateAttached': '',
                    'BankId' : 0
                };
            $scope.formMBtnText = 'Add Form M Details';
            $scope.processingFormM = false;
        };

        $scope.editFormM = function ()
        {
            if ($scope.formM.Id < 1) 
            {
                $scope.buttonText = "Add";
            }
            else
            {
                $scope.buttonText = "Update";
            }
            
            ngDialog.open({
                template: '/App/BnkAppUsr/ProcessFormM.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.addFormM = function (obj) 
        {
            if (obj == null || obj.ProductId < 1)
            {
                return;
            }

            $scope.initializeFormM();
            $scope.buttonText = "Add";
            ngDialog.open({
                template: '/App/BnkAppUsr/ProcessFormM.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.uploadLtFinDocument = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null && el.files.length < 1)
            {
                return;
            }
           
            $scope.processinLt = true;
            var finDoc = el.files[0];
            if (finDoc.size > 4096000)
            {
                alert('File size must not exceed 4MB');
                return;
            }
            $scope.genClass = "verifying";
            $upload.upload({
                url: "/Document/SaveTempFile",
                method: "POST",
                data: { file: finDoc }
            })
             .progress(function (evt) 
             {

             }).success(function (data) {
                 $scope.processinLt = false;
                 if (data.Code < 1) {
                     $scope.finDocSession = 0;
                     $scope.genClass = "notVerified";
                     $scope.setAppError('Document could not be processed. Please try again.');
                     $scope.hideFeedBack();
                     return;
                 }
                 $scope.finDocSession = 5;
                 $scope.appBanker.FinLetterPath = data.Path;
                 $scope.genClass = "verified";
             });
        };

        $scope.ProcessFormMDoc = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null && el.files.length < 1) 
            {
                return;
            }

            $scope.formMDoc = el.files[0];
        };

        $scope.ProcessFormMDocCompleted = function (response)
        {
            if (response.Code < 1)
            {
                $scope.formMDoc = null;
                alert('The selected document could not be processed. Please try again later.');
                return;
            }
           
        };

        $scope.processformM = function ()
        {
            if (!$scope.ValidateFormM($scope.formM))
            {
                return;
            }

            if ($scope.formMDoc.size > 4096000)
            {
                alert('File size must not exceed 4MB');
                return;
            }
           
            var payLoad = 'id=' +  $scope.formM.Id + '&notificationId=' + $scope.notification.Id + '&dateIssued=' + $scope.convert($scope.formM.DateIssued) + '&formMReference=' + $scope.formM.FormMReference
                + '&quantity=' + $scope.formM.Quantity + '&letterOfCreditNo=' + $scope.formM.LetterOfCreditNo + '&attachedDocumentId=' + $scope.formM.AttachedDocumentId + '&bankId=' + $scope.notification.BankId;

            var url;
            if ($scope.formM.Id < 1)
            {
                if ($scope.formMDoc == null || $scope.formMDoc.size < 1)
                {
                    alert('Please select a document to be attached.');
                    return;
                }
                url = "/Document/SaveFormM?";
            }
            else {
                url = "/Document/UpdateFormM?";
            }

            $upload.upload({
                url: url + payLoad,
                method: "POST",
                data: { file: $scope.formMDoc }
            }).progress(function (evt) 
            {
                $rootScope.busy = true;
            }).success(function (data)
            {
                $rootScope.busy = false;
                $scope.processformMCompleted(data);
            });
        };

        $scope.processformMCompleted = function (response)
        {
            $scope.processingFormM = false;
            if (response.Code < 1) {
                alert(response.Error);
                return;
            }

            $scope.formMBtnText = 'Update Form M Details';
            alert(response.Error);
            ngDialog.close('/App/BnkAppUsr/ProcessFormM.html', '');
        };

        $scope.ValidateFormM = function (formM) 
        {
            if (formM.Quantity < 1) 
            {
                alert('Please provide product Quantity.');
                return false;
            }

            if (formM.DateIssued == null || formM.DateIssued.length < 1)
            {
                alert('Please provide product Quantity.');
                return false;
            }

            if (formM.FormMReference == null || formM.FormMReference.length < 1) {
                alert('Please provide FormM Reference.');
                return false;
            }
            if (formM.LetterOfCreditNo == null || formM.LetterOfCreditNo.length < 1) {
                alert('Please provide Letter Of Credit No.');
                return false;
            }

            if (formM.NotificationId < 1)
            {
                alert('An error was encountered on the Page. Please refresh the page and try again.');
                return false;
            }

            return true;
        };

        $scope.convert = function (str)
        {
            var date = new Date(str),
                mnth = ("0" + (date.getMonth()+1)).slice(-2),
                day  = ("0" + date.getDate()).slice(-2);
            return [ date.getFullYear(), mnth, day ].join("-");
        }
    }]);
    
});
