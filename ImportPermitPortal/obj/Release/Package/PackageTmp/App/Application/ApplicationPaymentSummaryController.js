"use strict";

define(['application-configuration', 'userAppService'], function (app)
{
    
    app.register.controller('applicationController', ['$scope', '$rootScope', '$routeParams', 'userAppService', '$location',
    function ($scope, $rootScope, $routeParams, userAppService, $location)
    {
        $scope.PaymentTypes = [{ 'Id': 1, 'Name': 'Online', 'Identity': 'Online' }, { 'Id': 2, 'Name': 'Bank', 'Identity': 'Bank' }];
        
        
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Payment Sumary|DPR-PPIPS');
            var app = userAppService.getApp();
            if (app == null || app.ApplicationId < 1)
            {
                alert('An errror was encountered on the page.  Please re-select the Application.');
                $location.path('/Application/MyApplications');
            }
            else
            {
               
                $scope.setApp(app);
            }
        };
        
        $scope.updatePaymentType = function (id)
        {
            if (id < 1)
            {
                alert('Error: selected Payment option not detected. Please try again.');
                return;
            }

            $scope.processing = true;
            userAppService.updateAppPaymentOption(id, $scope.item.Id, $scope.updatePaymentTypeCompleted);
        };

        $scope.updatePaymentTypeCompleted = function (res)
        {
            $scope.processing = false;
            if (res.Code > 0)
            {
                $scope.item.PaymentTypeId = $scope.PTypeId;
                $scope.printInvoice();
                alert(res.Error);
            }
            else
            {
                alert(res.Error);
            }
            
        };

        $scope.setApp = function (data)
        {
          
            $scope.rrr = data.Rrr;
            $scope.item = data;
            if (data.Rrr !== null && data.Rrr !== undefined && data.Rrr.length > 0)
            {
                var pOpt = parseInt(data.PaymentTypeId);
                if (pOpt === 2) {
                    $scope.bankOpt = true;
                    $scope.onlineOpt = false;
                    $scope.success = 'Thank you for applying online. Kindly walk into any of the approved Banks to make your Application Payment using the details below:';

                }
                if (pOpt === 1) {
                    $scope.success = "Thank you for applying online. Your Application details are shown below. Click the 'Continue to Payment' button to proceed with the payment process";
                    $scope.bankOpt = false;
                    $scope.onlineOpt = true;

                }
                $scope.positivefeedback = true;
                $scope.PTypeId = $scope.item.PaymentTypeId;

                var frmInfo = '<form action="https://login.remita.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                    + '<input name="merchantId" value="' + 442773233 + '" type="hidden">'
                    + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                    + '<input name="rrr" value="' + data.Rrr + '" id="rrr" type="hidden">'
                    + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                    + '</form>';

                //var frmInfo = '<form action="http://www.remitademo.net/remita/ecomm/finalize.reg" name="SubmitRemitaForm" method="POST" id="rmt_frm" style="display: none">'
                //   + '<input name="merchantId" value="' + 2547916 + '" type="hidden">'
                //   + '<input name="hash" id="hash" value="' + data.Hash + '" type="hidden">'
                //   + '<input name="rrr" value="' + data.Rrr + '" id="rrr" type="hidden">'
                //   + '<input name="responseurl" id="uri" value="' + data.RedirectUri + '" type="hidden">'
                //   + '</form>';

                $scope.frm = frmInfo;
                $scope.frmUrl = data.RedirectUri;
            }
           
            $scope.printInvoice();
        };

        $scope.printInvoice = function ()
        {
            var pOpt = parseInt($scope.item.PaymentTypeId);
            if (pOpt === 2)
            {
                $scope.bankOpt = true;
                $scope.onlineOpt = false;
                $scope.success = 'Thank you for applying online. Kindly walk into any of the approved Banks to make your Application Payment using the details below:';

            }
            if (pOpt === 1)
            {
                $scope.success = "Thank you for applying online. Your Application details are shown below. Click the 'Continue to Payment' button to proceed with the payment process";
                $scope.bankOpt = false;
                $scope.onlineOpt = true;

            }
        };
        
        $scope.goToApps = function ()
        {
            $location.path('Application/MyApplications');
        };
      
        $scope.printReceipt = function () {

            var printContents = document.getElementById('receipt').innerHTML;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1)
            {
                popupWin = window.open('', '_blank', 'width=800,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
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
        }
        
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
            } else {
                popupWin = window.open('Continue to Remita', $scope.frmUrl, 'width=500,height=600,scrollbars=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" /></head><body>' + content + '</html>');
                popupWin.document.getElementById("rmt_frm").submit();
            }

        }
       
    }]);
    
});


