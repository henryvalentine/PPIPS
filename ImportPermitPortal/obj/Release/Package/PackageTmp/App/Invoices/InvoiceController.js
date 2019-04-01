"use strict";

define(['application-configuration', 'invoiceService'], function (app)
{
    
    app.register.controller('invoiceController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'invoiceService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, invoiceService, $location)
    {
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Invoice Details|DPR-PPIPS');
            var inv = invoiceService.getInfo();
            if (inv.Id < 1)
            {
                alert('An error was encountered on the page. Please try again.');
                $location.path('/Invoices/Invoices');
            }
            else
            {
                if (inv.ApplicationObject !== null && inv.ApplicationObject.Id > 0)
                {
                    $scope.applicationInvoice = inv;
                }
                else
                {
                    if (inv.NotificationObject !== null && inv.NotificationObject.Id > 0)
                    {
                        $scope.notificationInvoice = inv;
                    }
                }
            }
            
        };
        
        $scope.printReceipt = function ()
        {
            var printContents = '';

            if ($scope.applicationInvoice !== null)
            {
                if ($scope.applicationInvoice.Id > 0)
                {
                    printContents = document.getElementById('applicationReceipt').innerHTML;
                }
            } else {
                if ($scope.notificationInvoice !== null) {
                    if ($scope.notificationInvoice.Id > 0)
                    {
                        printContents = document.getElementById('notificationReceipt').innerHTML;
                    }
                }

            }
            
            if (printContents.length < 1)
            {
                alert('The print process could not be initiated.');
                return null;
            }

            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1)
            {
                popupWin = window.open('', '_blank', 'width=800,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="style.css" />' +
                    '</head><body onload="window.print()"><div class="reward-body">' + printContents + '</div></html>');
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
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin.document.close();
            }
            popupWin.document.close();

            return true;
        }

    }]);
    
});


