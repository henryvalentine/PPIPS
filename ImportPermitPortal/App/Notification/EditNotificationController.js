"use strict";

define(['application-configuration', 'userNotificationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{

    app.register.controller('editNotificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userNotificationService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, userNotificationService, $location) {
        var xcvb = new Date();
        $scope.year = xcvb.getFullYear();
        $scope.month = xcvb.getMonth();
        $scope.day = xcvb.getDate();

        $scope.open = function ($event, elementOpened) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.notification[elementOpened] = !$scope.notification[elementOpened];
        };

         $scope.PaymentTypes = [{ 'Id': 1, 'Name': 'Online', 'Identity': 'Online' }, { 'Id': 2, 'Name': 'Bank', 'Identity': 'Bank' }];

        $scope.getLists = function () {
            userNotificationService.getGenericList($scope.getListsCompleted);

        };


        $scope.compareDates = function () {
            if ($scope.notification.ArrivalDate == undefined || $scope.notification.ArrivalDate == null || $scope.notification.ArrivalDate.length < 1
                || $scope.notification.DischargeDate == null || $scope.notification.DischargeDate === undefined || $scope.notification.DischargeDate.length < 1) {
                return;
            }
            if ((new Date($scope.notification.ArrivalDate)) > (new Date($scope.notification.DischargeDate))) {
                alert('Vessel Discharge Date should be equal to or greater than Vessel Arrival Date.');
                $scope.notification.DischargeDate = null;
                return;
            }
        };

        $scope.getListsCompleted = function (data) {
          
            $scope.dateSetting = data.ImportSettingObject;
            $scope.month = $scope.month + 1;
            $scope.day = $scope.day + $scope.dateSetting.VesselArrivalLeadTime;
            $scope.minDate = $scope.year + '/' + $scope.month + '/' + $scope.day;

            setMaxDate($scope, $scope.minDate, '');
            setControlDate($scope, $scope.minDate, '');

            $scope.ordinaryproducts = data.Products;
            $scope.countries = data.Countries;
            $scope.depotList = data.DepotList;
            $scope.classes = data.ImportClasses;

            if ($scope.notificationType == null || $scope.notificationType == undefined || $scope.notificationType !== 1) {
                $scope.applicationItems = $scope.ordinaryproducts;
            }

        };

        $scope.setDischargeDepot = function (depotId) {
            if (parseInt($scope.notification.ClassObject.Id) < 2) {
                if (parseInt(depotId) < 1) {
                    return;
                }
                angular.forEach($scope.depotList, function (d, i) {
                    if (d.Id === depotId) {
                        $scope.notification.Depot = d;
                    }
                });
            }
        };

        $scope.toggleType = function () {
            if ($scope.notificationType > 1) {
                $scope.applicationItems = $scope.ordinaryproducts;
                $scope.showDetails = false;
            }

        };

        $scope.setEstimatedQuantity = function () {
            if (parseInt($scope.notificationType) === 1) {
                angular.forEach($scope.notification.ApplicationItemObjects, function (y, l) {
                    if (y.ProductId === $scope.notification.ApplicationItemId) {
                        $scope.notification.QuantityOnVessel = y.EstimatedQuantity;
                    }
                });
            } else {
                $scope.notification.QuantityOnVessel = 0;
            }
        };

        $scope.serchAppByPermitValue = function () {
            if ($scope.notification.PermitNumber == null || $scope.notification.PermitNumber < 1) {
                alert('Please provide Permit Number.');
                return;
            }

            userNotificationService.getApplicationByPermitValue($scope.notification.PermitNumber.trim(), $scope.serchAppByPermitValueCompleted);
        };

        $scope.serchAppByPermitValueCompleted = function (response)
        {
            if (response == null || response.ApplicationId < 1 || response.ApplicationItemObjects.length < 1)
            {
                alert('Permit Number could not be verified.');
                return;
            }

            $scope.permitApplication = response;
            $scope.clss = 'col-md-6';

            $scope.notification.ClassObject = { 'Id': response.ClassificationId, 'Name': response.ImportClassName },
             $scope.applicationItems = $scope.permitApplication.ApplicationItemObjects;
            $scope.showDetails = true;

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('New Notification|DPR-PPIPS');
            $scope.clss = 'col-md-4';
            $scope.paymentOption = 0;
            $scope.next = false;
            $scope.add = true;
            $scope.edit = false;
            $scope.accept = false;
            $scope.processing = false;
            $scope.showDetails = false;
            $scope.pstage = false;

            var app = userNotificationService.getNotification();
            if (app == null || app.Id < 1) {
                alert('An errror was encountered on the page.  Please reselect the Notification.');
                $location.path('/Notification/MyNotifications');
            } else {
                $scope.notification = app;
                $scope.notification.ArrivalDate = new Date(app.ArrivalDateStr);
                $scope.notification.DischargeDate = new Date(app.DischargeDateStr);

                $scope.permitValue = app.PermitValue;

                $scope.notification = app;
                $scope.getLists();
            }



        };

        $scope.getValWithReqs = function () {
            if (!$scope.ValidateNotification()) {
                return;
            }

            if ($scope.notification.QuantityToDischarge > $scope.notification.QuantityOnVessel) {
                alert("The Quantity to Discharge must not be greater than Quantity on Vessel.");
                return;
            }

            var notificationProps =
                {
                    'NotificationClassId': $scope.notification.ClassObject.Id,
                    'CargoInformationTypeId': parseInt($scope.notification.CargoInformationTypeId),
                    'ArrivalDate': $scope.notification.ArrivalDate,
                    'DischargeDate': $scope.notification.DischargeDate,
                    'EstimatedQuantity': $scope.notification.ApplicationItem.EstimatedQuantity,
                    'TotalImportedQuantity': $scope.notification.ApplicationItem.TotalImportedQuantity + $scope.notification.QuantityToDischarge,
                    'QuantityToDischarge': $scope.notification.QuantityToDischarge
                };
            
            userNotificationService.getAmountDueWithReqs(notificationProps, $scope.getValWithReqsCompleted);
        };

        $scope.getValWithReqsCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
                return;
            }

            $scope.notification.FeeObjects = data.FeeObjects;
            if ($scope.notification.FeeObjects.length > 2) {
                angular.forEach($scope.notification.FeeObjects, function (h, i) {
                    if (h.FeeTypeId === 3) {
                        $scope.expenditionaryInvolved = true;

                    }
                });
            }

            angular.forEach($scope.applicationItems, function (z, t) {
                if (z.ProductId === $scope.notification.ApplicationItem.ProductObject.ProductId) {
                    $scope.notification.ApplicationItemCode = z.Code;
                }
            });

            $scope.expenditionaryFee = 165000;
            $scope.notification.AmountDue = data.Extent;
            $scope.stageDocs = data.DocumentTypeObjects;
            $scope.notification.NotificationId = data.Code;
            $scope.notification.DerivedValue = data.Extent;
            $scope.notification.PermitValue = $scope.permitApplication.PermitValue;
            $scope.notification.CompanyName = data.UserName;
            $scope.notification.NotificationClassName = $scope.notification.ClassObject.Name;
            $scope.notification.CargoTypeName = data.CargoTypeName;
            $scope.notification.PermitId = $scope.permitApplication.PermitId;
            $scope.notification.fees = data.FeeObjects;
            $scope.notification.impObject = $scope.impObject;
            $scope.notification.ArrivalDateStr = data.ArrivalDate + ' - ' + data.DischargeDate;
            $scope.notification.DischargeDateStr = data.DischargeDate;
            $scope.notification.CargoTypeName = data.CargoTypeName;
            angular.forEach($scope.PaymentTypes, function (g, i) {
                if ($scope.notification.PaymentTypeId === g.Id) {
                    $scope.pOpt = g.Name;
                }
            });


            $scope.notification.PaymentOption = $scope.pOpt;

            var code = $scope.notification.ApplicationItem.ProductObject.Code;
            var cds = code.split("-")[0];
            $scope.notification.ProductCode = cds + data.BalanceVolume;
            $scope.setStageDocs();
        };

        $scope.processNotification = function () {

            if ($scope.notification == null || $scope.notification.Id < 1) {
                alert('An errror was encountered on the page.  Please reselect the Notification.');
                $location.path('/Notification/MyNotifications');
            }

            if (!$scope.ValidateNotification())
            {
                return;
            }
          

            if (parseInt($scope.notification.ClassObject.Id) === 1)
            {

                if (($scope.notification.QuantityToDischarge + $scope.notification.ApplicationItem.TotalImportedQuantity) > $scope.notification.ApplicationItem.EstimatedQuantity) {
                    alert("The Quantity to Discharge must not be greater than the PERMIT Quantity.");
                    return;
                }
            }

            if ($scope.notification.QuantityToDischarge > $scope.notification.QuantityOnVessel)
            {
                alert("The Quantity to Discharge must not be greater than Quantity on Vessel.");
                return;
            }

            if ($scope.notification.ClassObject == null || $scope.notification.ClassObject.Id < 1)
            {
                alert("Please select Notification Category.");
                return;
            }

            var notification =
            {
                'Id': $scope.notification.Id,
                'ReferenceCode': $scope.notification.ReferenceCode,
                'ClassificationId': $scope.notification.ClassObject.Id,
                'ApplicationQuantity': $scope.notification.ApplicationItem.EstimatedQuantity,
                'InvoiceId' : $scope.notification.ApplicationId,
                'QuantityImported': $scope.notification.ApplicationItem.TotalImportedQuantity,
                'ApplicationId': $scope.notification.ApplicationId,
                'PaymentTypeId': $scope.notification.PaymentTypeId,
                'DischargeDepotId': $scope.notification.Depot.DepotId,
                'ProductId': $scope.notification.ApplicationItem.ProductObject.ProductId,
                'QuantityToDischarge': $scope.notification.QuantityToDischarge,
                'QuantityOnVessel': $scope.notification.QuantityOnVessel,
                'CargoInformationTypeId': $scope.notification.CargoInformationTypeId,
                'ArrivalDate': $scope.notification.ArrivalDate,
                'DischargeDate': $scope.notification.DischargeDate,
                'DateCreated': $scope.notification.DateCreated,
                'FeeObjects': $scope.notification.fees,
                'AmountDue': $scope.notification.AmountDue,
                'DerivedValue': $scope.notification.DerivedValue,
                'PermitId': $scope.notification.PermitId,
                'PortName': $scope.notification.PortName,
                'CountryId': $scope.notification.Country.CountryId
            };

            $scope.processing = true;
            userNotificationService.editNotification(notification, $scope.processNotificationCompleted);

        };

        $scope.processNotificationCompleted = function (data)
        {
            $scope.processing = false;
            if (data.NotificationId < 1)
            {
                alert(data.Error);
                return;
            } else {
                if (data.Code < 1) {
                    if (data.Code === -11)
                    {
                        alert(data.Error);
                        $location.path('Notification/MyNotifications');
                    }
                }

                $scope.rrr = data.Rrr;
                $scope.notification.ReferenceCode = data.RefCode;
                $scope.multiplier = data.Multiplier;
                $scope.notification.Id = data.AppId;

                if (data.Code > 0 && data.ErrorCode < 1) {
                    alert(data.Error);
                    $location.path('Notification/MyNotifications');
                }

                if (data.Code > 0 && data.ErrorCode > 0) {
                    var pType = '';
                    angular.forEach($scope.PaymentTypes, function (k, i) {
                        if ($scope.notification.PaymentTypeId === i.Id) {
                            pType = k.Identity;
                        }
                    });

                    $scope.rrr = data.Rrr;
                    $scope.notification.ReferenceCode = data.RefCode;

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

                    $scope.item = $scope.notification;
                    $scope.printInvoice();
                }
            }
        };

        $scope.setStageDocs = function () {
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
            $scope.summary = true;
            $scope.invoiceStage = false;
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.printInvoice = function () {
            var pOpt = parseInt($scope.notification.PaymentTypeId);
            if (pOpt === 3) {
                $scope.bankOpt = true;
                $scope.onlineOpt = false;
                $scope.success = 'Thank you for applying online. Kindly walk into any of the approved Banks to make your Application Payment using the details below:';
                $scope.positivefeedback = true;
            }
            if (pOpt === 1 || pOpt === 2) {
                $scope.success = "Thank you for applying online. Your Application details are shown below. Click the 'Continue to Payment' button to proceed with the payment process";
                $scope.bankOpt = false;
                $scope.onlineOpt = true;
                $scope.positivefeedback = true;
            }


            $scope.refreshSession();
            $scope.summary = false;
            $scope.pstage = true;
            $scope.invoiceStage = true;
        };

        $scope.resetSummary = function () {
            $scope.add = false;
            $scope.edit = true;
            $scope.summary = false;
            $scope.invoiceStage = false;
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

        $scope.ValidateNotification = function () {
            if (parseInt($scope.notification.CargoInformationTypeId) < 1) {
                alert("Please select Cargo Type.");
                return false;
            }

            if ($scope.notification.QuantityToDischarge < 1) {
                alert("Please provide Quantity To Discharge.");
                return false;
            }

            if ($scope.notification.QuantityOnVessel < 1) {
                alert("Please provide Quantity on Vessel.");
                return false;
            }


            if ($scope.notification.Country.Id < 1) {
                alert("Please select a Country.");
                return false;
            }


            if ($scope.notification.PortName == null || $scope.notification.PortName.length < 1) {
                alert("Please provide Load Port.");
                return false;
            }

            if ($scope.notification.Depot.DepotId < 1) {
                alert("Please select discharge Depot.");
                return false;
            }

            if ($scope.notification.ApplicationItem.ProductObject.ProductId < 1) {
                alert("Please select a product.");
                return false;
            }

            if ($scope.notification.ArrivalDate == undefined || $scope.notification.ArrivalDate == null || $scope.notification.ArrivalDate.length < 1) {
                alert("Please provide Vessel Arrival Date.");
                return false;
            }

            if ($scope.notification.DischargeDate == undefined || $scope.notification.DischargeDate == null || $scope.notification.DischargeDate.length < 1) {
                alert("Please provide Vessel Discharge Date.");
                return false;
            }
            if ($scope.notification.PaymentTypeId == null || $scope.notification.PaymentTypeId == undefined || $scope.notification.PaymentTypeId < 1) {
                alert("Please select a payment option.");
                return false;
            }

            if ($scope.notification.ClassObject == null || $scope.notification.ClassObject.Id < 1) {
                alert("Please select Notification Category.");
                return false;
            }
            return true;
        };
    }]);

});


