"use strict";

define(['application-configuration', 'userNotificationService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    
    app.register.controller('continueNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $location, $upload, fileReader, $http)
    {

        $scope.toggleType = function () {
            if ($scope.notificationType > 1) {
                $scope.showDetails = false;
            }

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
                if (n.DocumentId === docId)
                {
                    $scope.selectedDoc = n;
                    $scope.bnkDocs.push(n);
                    $scope.suppliedDocs.splice(m, 1);
                }
                
            });

        };

        $scope.gotoNotifications = function ()
        {
            $location.path('/Notification/MyNotifications');

        };
        
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Complete Notification|DPR-PPIPS');
            $scope.ntStat = { invoiceStage: false, accept: false, bankOpt: false, onlineOpt : false, rrr : ""};
            
            $scope.processing = false;
            $scope.ldr = '<img src="/Images/load.GIF" style="float: left" id="loder" title="Processing File"/>';
            $scope.scs = '<img src="/Images/success.png" style="float: left" id="saveSuccess" title="File was successfully processed"/>';
            $scope.sEr = '<img src="/Images/error.png" style="float: left" id="saveError" title="File processing failed. Please try again."/>';

            $scope.notificationType = 0;
            $scope.cargoType = 0;
            var notifictionId = $routeParams.id;
            if (parseInt(notifictionId) < 1)
            {
                alert('An errror was encountered on the page.  Please reselect the Notification.');
                $scope.goToApps();
            }

            userNotificationService.getNotificationForEdit(notifictionId, $scope.getNotificationCompleted);
        };
        
        $scope.getNotificationCompleted = function (app)
        {
            if (app == null || app.Id < 1)
            {
                alert('Notification information could not be retrieved. Please reselect the Notification.');
                $scope.goToApps();
            }
            else {
                $scope.notification = app;
                $scope.bnkDocs = [];
                $scope.suppliedDocs = [];
                $scope.nextDocs = [];
                $scope.notification.VesselObject = {};
                angular.forEach($scope.notification.DocumentTypeObjects, function (n, m)
                {
                    if (n.Uploaded === true)
                    {
                        n.index = m + 1;
                        $scope.suppliedDocs.push(n);
                    }
                    else
                    {
                        if (n.StageId === 1)
                        {
                            $scope.bnkDocs.push(n);
                        }
                        else
                        {
                            $scope.nextDocs.push(n);
                        }
                    }
                });

                $scope.permitValue = app.PermitValue;
                $scope.notification.ArrivalDate = app.ArrivalDateStr;
                $scope.notification.DischargeDate = app.DischargeDateStr;
                $scope.notificationType = app.NotificationTypeId;
                $scope.cargoType = app.CargoInformationTypeId;
                $scope.getNotificationVessels($scope.notification.Id);
            }
        };

        $scope.getDepotCollection = function ()
        {
            userNotificationService.getDepotCollection($scope.getDepotCollectionCompleted);
        };

        $scope.getDepotCollectionCompleted = function (response)
        {
            $scope.depotList = response.DepotList;
        };

        $scope.numberCollection = function () {
            angular.forEach($scope.suppliedDocs, function (n, m)
            {
                n.index = m + 1;
            });
        };

        $scope.verifyCode = function (code) {
            $scope.licenseVerified = false;
            if (code == null || code.length < 1)
            {
                alert("ERROR: Please provide Shuttle Vessel License Number.");
                return;
            }
          
            $scope.depotClass = "verifying";
            var codeVrifier = { 'RefCode': code, 'LicenseType': 5, 'ImporterId': 0 }
            userNotificationService.VerifyVesselLicense(codeVrifier, $scope.verifyCodeCompleted);
        };

        $scope.verifyCodeCompleted = function (response)
        {
            if (response.Code < 1)
            {
                $scope.pgenClass = "notVerified";
                alert(response.Error);
                return;
            }

            $scope.licenseVerified = true;
            $scope.pgenClass = "verified";

            $scope.depotClass = "verified";
            angular.forEach($scope.depotList, function (v, i)
            {
                if (v.Id === response.Code) {
                    $scope.ApplicationItem.DepotObject = v;
                }
            });

            
        };

        $scope.getNotificationVessels = function (notificationId)
        {
            if (notificationId == null || notificationId < 1)
            {
                alert('An errror was encountered on the page.  Please reselect the Notification.');
                $location.path('/Notification/MyNotifications');
            }
            userNotificationService.getNotificationVesssels(notificationId, $scope.getNotificationVesselsCompleted);
        };

        $scope.getNotificationVesselsCompleted = function (vessels)
        {
            $scope.getDepotCollection();
            if (vessels.length > 0)
            {
               angular.forEach(vessels, function (y, k) 
                {
                   if (y.VesselClassTypeId === 1)
                   {
                       $scope.ms =
                       {
                           'NotificationVesselId': y.VesselClassTypeId,
                           'VesselId': y.VesselClassTypeId,
                           'NotificationId': $scope.notification.Id,
                           'VesselClassTypeId': y.VesselClassTypeId,
                           'Name': y.Name,
                           'VesselClassName': y.VesselClassName
                       }
                   }
                   if (y.VesselClassTypeId === 2)
                   {
                       angular.forEach($rootScope.vessels, function (q, p)
                       {
                           if (y.VesselId === q.VesselId)
                           {
                               $scope.notification.VesselObject = q;

                             $scope.sh =
                             {
                                 'NotificationVesselId': y.NotificationVesselId,
                                 'VesselId': y.VesselId,
                                 'NotificationId': $scope.notification.Id,
                                 'VesselClassTypeId': y.VesselClassTypeId,
                                 'Name': y.Name,
                                 'VesselClassName': y.VesselClassName
                             }
                           }

                       });
                       
                       
                   }
                });
                $scope.edit = true;
                $scope.add = false;
            } 
            else
            {
                $scope.ms =
                     {
                         'NotificationVesselId': '',
                         'VesselId': '',
                         'NotificationId': $scope.notification.Id,
                         'VesselClassTypeId': 1,
                         'Name': '',
                         'VesselClassName': ''
                     }

                $scope.sh =
                      {
                          'NotificationVesselId': '',
                          'VesselId': '',
                          'NotificationId': $scope.notification.Id,
                          'VesselClassTypeId': 2,
                          'Name': '',
                          'VesselClassName': ''
                      }
                $scope.edit = false;
                $scope.add = true;
            }
        };

        $scope.submitVessels = function ()
        {
            $scope.vesselTracker = [];
           
            if ($scope.ms.Name == null || $scope.ms.Name.length < 1)
            {
                alert('Please provide mother Vessel Name.');
                return;
            }

            if ($scope.notification.VesselObject == null || $scope.notification.VesselObject.VesselId < 1)
            {
                alert('Please select Shuttle Vessel.');
                return;
            }

            var ms =
                    {
                        'NotificationVesselId': $scope.ms.NotificationVesselId,
                        'VesselId':  $scope.ms.VesselId,
                        'NotificationId': $scope.notification.Id,
                        'VesselClassTypeId':  $scope.ms.VesselClassTypeId,
                        'Name':  $scope.ms.Name,
                        'VesselClassName':  $scope.ms.VesselClassName
                    }

            var sh =
                  {
                      'NotificationVesselId': $scope.sh.NotificationVesselId,
                      'VesselId': $scope.notification.VesselObject.VesselId,
                      'NotificationId': $scope.notification.Id,
                      'VesselClassTypeId': $scope.sh.VesselClassTypeId,
                      'VesselClassName': $scope.sh.VesselClassName
                  }

            $scope.vesselTracker.push(ms, sh);

            if ($scope.vesselTracker.length < 1)
            {
                alert('Add the required Notification Vessels.');
                return;
            }
            
            if ($scope.add === true)
            {
                userNotificationService.addNotificationVessels($scope.vesselTracker, $scope.submitVesselsCompleted);
            }

            if ($scope.edit === true)
            {
                userNotificationService.updateNotificationVessels($scope.vesselTracker, $scope.submitVesselsCompleted);
            }
        };

        $scope.submitVesselsCompleted = function (response)
        {
            if (response.Code < 1)
            {
                alert(response.Error);
                return;
            }
            alert(response.Error);
            $scope.getNotificationVessels($scope.notification.Id);
        };

        $scope.checkIfArrayIsUnique = function (arr)
        {
            arr.sort();
            for (var i = 1; i < arr.length; i++)
            {
                if (arr[i].VesselClassTypeId === arr[i - 1].VesselClassTypeId && arr[i].VesselClassTypeId === 1)
                {
                    return false;
                }
            }
            return true;
        };
        
        $scope.removeVessel = function (vsl)
        {
            if (vsl.TempId < 1)
            {
                alert('invalid selection!');
                return;
            }
            if ($scope.vesselTracker.length === 1)
            {
                return;
            }
            if (!confirm("This Item will be removed from the list. Continue?"))
            {
                return;
            }
            angular.forEach($scope.vesselTracker, function (y, k)
            {
                if (y.TempId === vsl.TempId)
                {
                    $scope.vesselTracker.splice(k, 1);
                }
            });
        };
      
        $scope.goToApps = function ()
        {
            $location.path('Notification/MyNotifications');
        };
      
        $scope.ProcessDocument = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];
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
            }
            else
            {
                url = "/Document/SaveNotificationFile?docTypeId=" + doctTypeId + '&&notificationId=' + $scope.notification.Id;
            }

            $upload.upload({
                url: url,
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt) {
               infoDiv.html($scope.ldr);
           }).success(function (data)
           {
               if (data.code < 1)
               {
                   infoDiv.html($scope.sEr);
               }
               else {
                   infoDiv.html($scope.scs);
                   // 
                   if (isupdate === true)
                   {
                       $scope.selectedDoc.DocumentPath = data.Path;
                       $scope.suppliedDocs.push($scope.selectedDoc);
                       n.Uploaded = true;
                       angular.forEach($scope.bnkDocs, function (n, m)
                       {
                           if (n.DocumentTypeId === $scope.selectedDoc.DocumentTypeId)
                           {
                               $scope.bnkDocs.splice(m, 1);
                               $scope.selectedDoc = null;
                           }
                       });
                   }
                   else {
                       angular.forEach($scope.bnkDocs, function (n, m)
                       {
                           if (n.DocumentTypeId === doctTypeId) {
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

        $scope.checkNotificationSubmit = function ()
        {
            if ($scope.accept !== true)
            {
                alert('Please accept the Declaration before proceeding.');
                return;
            }
            userNotificationService.checkNotificationSubmit($scope.notification.Id, $scope.checkNotificationSubmitCompleted);
        };

        $scope.checkNotificationSubmitCompleted = function (response)
        {
            if (response.Code < 1)
            {
                alert(response.Error);
                return;
            }

            $scope.NotificationRequirementDetails = response;
            $scope.NotificationRequirementDetails.PaymentTypeId = 0;
            if (response.IsVessselsProvided === false)
            {
                alert('You are yet to provide vessel information for this Notification.');
                return;
            }

            if (response.IsVessselsProvided === false)
            {
                alert('You are yet to provide vessel information for this Notification.');
                return;
            }

            if ((response.IsExpenditionaryApplicable === true && response.ExpenditionaryFee !== null && response.ExpenditionaryFee.FeeId > 0) || (response.UnsuppliedDocuments !== null && response.UnsuppliedDocuments.length > 0))
            {
                ngDialog.open({
                    template: '/App/Notification/UnsuppliedDocsView.html',
                    className: 'ngdialog-theme-flat',
                    scope: $scope
                });
                return;
            }

            $scope.NotificationSubmit(); 
            
        };
        
        $scope.getRRR = function ()
        {
            if ($scope.NotificationRequirementDetails === null)
            {
                alert('An error was encountered on the page. Please rfresh the page andtry again.');
                return;
            }

            if ($scope.NotificationRequirementDetails.PaymentTypeId < 1)
            {
                alert('Please select a payment option.');
                return;
            }

            userNotificationService.getRRR($scope.NotificationRequirementDetails, $scope.getRRRCompleted);
        };

        $scope.getRRRCompleted = function (data)
        {
            if (data.Code < 1)
            {
                if (data.Code === -11)
                {
                    alert(data.Error);
                    return;
                }
            }

            $scope.rrr = data.Rrr;
            $scope.notification.ReferenceCode = data.RefCode;

            if (data.Code > 0 && data.ErrorCode < 1)
            {
                alert(data.Error);
                return;
            }

            if (data.Code > 0 && data.ErrorCode > 0)
            {
                var pType = '';
                angular.forEach($scope.PaymentTypes, function (k, i)
                {
                    if ($scope.notification.PaymentTypeId === i.Id)
                    {
                        pType = k.Identity;
                    }
                });

                $scope.ntStat.rrr = data.Rrr;
                $scope.notification.ReferenceCode = data.RefCode;
                $scope.notification.FeeObjects.push($scope.NotificationRequirementDetails.ExpenditionaryFee);
                $scope.notification.CompanyName = data.CompanyName;
                var frmInfo = '<form action="https://login.remita.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                    + '<input name="merchantId" value="' + 442773233 + '" type="hidden">'
                    + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                    + '<input name="rrr" value="' + data.Rrr + '" id="rrr" type="hidden">'
                    + '<input type="hidden" id="pType" name="paymenttype" value="' + pType + '" />'
                    + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                    + '</form>';

                //var frmInfo = '<form action="http://www.remitademo.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                //    + '<input name="merchantId" value="' + 2547916 + '" type="hidden">'
                //    + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                //    + '<input name="rrr" value="' + data.Rrr + '" id="rrr" type="hidden">'
                //    + '<input type="hidden" id="pType" name="paymenttype" value="' + pType + '" />'
                //    + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                //    + '</form>';
                
                $scope.frm = frmInfo;
                $scope.frmUrl = data.RedirectUri;
                var pOpt = parseInt($scope.notification.PaymentTypeId);

                if (pOpt === 2)
                {

                    $scope.ntStat.bankOpt = true;
                    $scope.ntStat.onlineOpt = false;
                    $scope.success = 'Please Kindly walk into any of the approved Banks to make complete this Payment using the details below:';
                    $scope.positivefeedback = true;
                    $scope.notification.PaymentOption = 'BANK';
                }

                if (pOpt === 1)
                {
                    $scope.success = "Please Click the 'Continue to Payment' button to proceed with the payment process";
                    $scope.ntStat.bankOpt = false;
                    $scope.ntStat.onlineOpt = true;
                    $scope.notification.PaymentOption = 'Online';
                    $scope.positivefeedback = true;
                }

                $scope.ntStat.invoiceStage = true;
                ngDialog.close('/App/Notification/UnsuppliedDocsView.html', '');
                
            }
           
        };

        $scope.NotificationSubmit = function ()
        {
            if ($scope.ntStat.accept === false)
            {
                alert('Please check the Declaration Acceptance option.');
                return;
            }
            
            userNotificationService.submitNotification($scope.notification.Id, $scope.notificationSubmitCompleted);
        };

        $scope.notificationSubmitCompleted = function (response)
        {

            if (response.Code < 1)
            {
                alert(response.Error);
                return;
            }
            
            if (response.IsVessselsProvided === false)
            {
                alert('You are yet to provide vessel information for this Notification.');
                return;
            }

            if (response.Code > 0 && response.Code < 10)
            {
                $scope.NotificationRequirementDetails = response;

                if ((response.IsExpenditionaryApplicable === true && response.ExpenditionaryFee !== null && response.ExpenditionaryFee.FeeId > 0) || (response.UnsuppliedDocuments !== null && response.UnsuppliedDocuments.length > 0)) {
                    ngDialog.open({
                        template: '/App/Notification/UnsuppliedDocsView.html',
                        className: 'ngdialog-theme-flat',
                        scope: $scope
                    });
                    return;
                }
                else
                {
                    alert('An unknown error was encountered and process could not be completed. Please try again later.');
                    return;
                }
            }
            else
            {
                alert(response.Error);
                $scope.gotoNotifications();
            }
            
        };
        
        $scope.setStageDocs = function ()
        {
            if ($scope.stageDocs.length < 1) {
                alert('An unknown error was encountered. Please refresh the page and try again');
                return;
            }
            var docStr = '<br/><h4><b>Required Documents:</b></h4> <ul>';
            angular.forEach($scope.stageDocs, function (s, m) {
                docStr += '<li><label>&#8226 &nbsp;' + s.Name + '</label></li>';
            });
            docStr += '</ul>';
            angular.element('#appDocs').html(docStr);
            $scope.invoiceStage = false;
            $scope.add = true;
            $scope.edit = false;
        };
        
        $scope.printReceipt = function () {
            var printContents = document.getElementById('receipt').innerHTML;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('', '_blank', 'width=800,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css"  href="/Content/bootstrap.css"  />' +
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
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css"  href="/Content/bootstrap.css"  /></head><body onload="window.print()"><div class="row" style="width:95%; margin-left:3%; margin-right:2%; margin-top:5%; margin-bottom:2%"><div class="col-md-12">' + printContents + '</div></div></html>');
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

            $location.path('Notification/MyNotifications');
        };

        $scope.changeDepotInfo = function ()
        {
            $scope.depotObject =
               {
                   'Id': '',
                   'Name': '-- Select Depot --',
                   'DepotLicense': '',
                   'ImporterId': '',
                   'IssueDate': '',
                   'ExpiryDate': '',
                   'Status': ''
               };

            ngDialog.open({
                template: '/App/Notification/ChangeDepot.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.saveTempDoc = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                return;
            }

            $scope.newThroughput = el.files[0];
            
        };

        $scope.checkDepot = function (depotObject)
        {
            if (depotObject == null || depotObject.Id < 1)
            {
                return;
            }
            $scope.depotObject = depotObject;
            $scope.newDepotSelected = false;
            if ($scope.notification.SelectedDepotList !== null && $scope.notification.SelectedDepotList.length > 0)
            {
                var newDepotId = 0;
                angular.forEach($scope.notification.SelectedDepotList, function (v, i)
                {
                    if (v === depotObject.Id && v !== $scope.notification.DischargeDepotId)
                    {
                        newDepotId = v;
                        $scope.newDepotSelected = true;
                    }
                });
            }
        };

        $scope.UpdateDepot = function ()
        {
            if ($scope.depotObject == null || $scope.depotObject.Id < 1)
            {
                return;
            }

            if ($scope.depotObject.Id === $scope.notification.DischargeDepotId)
            {
                alert('Please select a different Depot inorder to proceed.');
                return;
            }
            
            userNotificationService.updateDepot($scope.notification.Id, $scope.depotObject.Id, $scope.UpdateDepotCompleted);

        };

        $scope.UpdateDepotCompleted = function (response)
        {
            if (response.Code < 1) {
                alert(response.Error);
                return;
            }

            alert(response.Error);
            $scope.notification.DischargeDepotId = $scope.depotObject.Id;
            $scope.notification.DepotName = $scope.depotObject.Name;
            ngDialog.close('/App/Notification/ChangeDepot.html', '');
            $scope.depotObject =
              {
                  'Id': '',
                  'Name': '-- Select Depot --',
                  'DepotLicense': '',
                  'ImporterId': '',
                  'IssueDate': '',
                  'ExpiryDate': '',
                  'Status': ''
              };
            return;
        };

        $scope.changeThroughput = function ()
        {
            if ($scope.depotObject == null || $scope.depotObject.Id < 1)
            {
                alert('Please select a Depot.');
                return;
            }

            if ($scope.newDepotSelected !== null && $scope.newDepotSelected !== false)
            {
                $scope.UpdateDepot();
                return;
            }

            if ($scope.newThroughput == null || $scope.newThroughput.size < 1)
            {
                alert('Please select a Throughput first.');
                return;
            }
           
            var url = "/Document/SaveNewThrouput?notificationId=" + $scope.notification.Id + '&depotId=' + $scope.depotObject.Id;
            $upload.upload({
                url: url,
                method: "POST",
                data: { file: $scope.newThroughput}
            })
           .progress(function (evt)
           {
               $rootScope.busy = true;
           }).success(function (data)
           {
               $rootScope.busy = false;
               if (data.code < 1)
               {
                   alert(data.Error);
                   return;
               }

               alert(data.Error);
               $scope.notification.DepotName = $scope.depotObject.Name;
               var newDoc = { 'DocumentId': data.Code, 'Name': data.FileName + '(' + $scope.depotObject.Name + ')', 'DocumentTypeId': data.DocumentTypeId, 'DocumentPath': data.Path }
               $scope.suppliedDocs.push(newDoc);
               $scope.newThroughput = null;
               ngDialog.close('/App/Notification/ChangeDepot.html', '');
           });


        };
         
    }]);
    
});
