"use strict";

define(['application-configuration', 'receiptService'], function (app)
{
    
    app.register.controller('receiptController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'receiptService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, receiptService, $location)
    {
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Payment Receipt|DPR-PPIPS');
            var inv = receiptService.getInfo();
            if (inv.Id < 1)
            {
                alert('An error was encountered on the page. Please try again.');
                $location.path('/Receipts/Receipts');
            }

            $scope.receipt = inv;
            var naira = "Naira";
            var kobo = "Kobo";

            var xVal = numbersToWord($scope.receipt.TotalAmountDue, naira, kobo);

            if (xVal.indexOf('Naira') === -1)
            {
                xVal = xVal + ' ' + naira;
            }
            
            $scope.receipt.AmountInWords = xVal + ' only';

        };
        
        $scope.printReceipt = function () {
            var printContents = document.getElementById('applicationReceipt').innerHTML;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('', '_blank', 'width=500,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css"  />' +
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
                popupWin = window.open('', '_blank', 'width=500,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css"  /></head><body onload="window.print()"><div class="row" style="width:95%; margin-left:3%; margin-right:2%; margin-top:5%; margin-bottom:2%"><div class="col-md-12">' + printContents + '</div></div></html>');
                popupWin.document.close();
            }
            popupWin.document.close();

            return true;
        }

    }]);
    
});


