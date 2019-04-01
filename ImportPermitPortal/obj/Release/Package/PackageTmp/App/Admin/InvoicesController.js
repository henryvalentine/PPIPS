"use strict";

define(['application-configuration', 'ngDialog', 'invoiceService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngNtfs', function ($compile)
    {
        return function ($scope, ngNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Transaction/GetInvoices";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['RRR', 'ImporterName', 'DateAddedStr', 'TotalAmountDueStr', 'PaymentOption', 'ServiceDescription', 'StatusStr'];
            var ttc = adminInvoiceTableManager($scope, $compile, ngNtfs, tableOptions, 'getInvoiceDetails', 'generateRrr', 'getTransactionDetails');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    }); 
                                 
    app.register.controller('adminInvoiceController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'invoiceService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, invoiceService, $upload, fileReader, $http, $location)
    {
        $rootScope.setPageTitle('Invoices|DPR-PPIPS'); 

        $scope.preview = false;

        $scope.getInvoiceDetails = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            invoiceService.getInvoiceDetails(appId, $scope.getInvoiceDetailsCompleted);
        };
        
        $scope.getInvoiceDetailsCompleted = function (inv)
        {
            if (inv == null || inv.Id < 1)
            {
                alert('Invoice information could not be retrieved. Please try again later.');
                return;
            }
            else {
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

                $scope.preview = true;
            }
        };

        $scope.generateRrr = function (id)
        {
            if (id < 1)
            {
                alert('Invalid selection!');
                return;
            }

            invoiceService.generateRrr(id, $scope.generateRrrCompleted);
        };

        $scope.generateRrrCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
                return;
            }
            var msg = data.Error + " Do you want to refresh the records?";
            if (!confirm(msg)) {
                return;
            }
            
            $scope.jtable.fnClearTable();
        };

        $scope.getTransactionDetails = function (e)
        {
            console.log(e.target);

            var el = (e.srcElement || e.target);
            if (el.id == null || el.id.length < 1)
            {
                alert('Invalid selection!');
                return;
            }

            var rrr = el.id;

            if (rrr == null || rrr.length < 1)
            {
                alert('Invalid selection!');
                return;
            }

            invoiceService.getTransactionDetails(rrr.trim(), $scope.getTransactionDetailsCompleted);
        };

        $scope.getTransactionDetailsCompleted = function (rrr)
        {
            $scope.rrr = null;
            $scope.rrr = rrr;
            ngDialog.open({
                template: '/App/Admin/TransactionDetails.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.insertPayment = function ()
        {
            if ($scope.rrr == null || $scope.rrr.OrderId.length < 1 || $scope.rrr.Rrr.length < 1)
            {
                alert('Error! Request could not be completed. Please try again later');
                return;
            }

            invoiceService.insertPayment($scope.rrr.Rrr, $scope.rrr.OrderId, $scope.insertPaymentCompleted);
        };

        $scope.insertPaymentCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
                return;
            }
            var msg = data.Error + " Do you want to refresh the records?";
            if (!confirm(msg))
            {
                return;
            }
            $scope.jtable.fnClearTable();
        };
        
        $scope.closeDetails = function ()
        {
            ngDialog.close('/App/Admin/TransactionDetails.html', '');
            $scope.rrr = null;
        };
        
        $scope.printReceipt = function () {
            var printContents = '';

            if ($scope.applicationInvoice !== null) {
                if ($scope.applicationInvoice.Id > 0) {
                    printContents = document.getElementById('applicationReceipt').innerHTML;
                }
            } else {
                if ($scope.notificationInvoice !== null) {
                    if ($scope.notificationInvoice.Id > 0) {
                        printContents = document.getElementById('notificationReceipt').innerHTML;
                    }
                }

            }

            if (printContents.length < 1) {
                alert('The print process could not be initiated.');
                return null;
            }

            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
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

        //Input Stream Tester
        $scope.notify = function ()
        {
            var data = [{ "rrr": "T49742615", "channnel": "BRANCH", "amount": "285000.0", "responseCode": "01", "transactiondate": "23/06/2015", "debitdate": "29/06/2015", "bank": "070", "branch": "070150168", "serviceTypeId": "437411514", "dateRequested": "29/06/2015", "orderRef": "295", "payerName": "INTER-FRONTIER INVESTMENT LIMITED ", "payerPhoneNumber": "08033049884 OR 07086982580", "payerEmail": "info@interfrontiersltd.com", "uniqueIdentifier": null }];
            var dataToSend = JSON.stringify(data);
            $.ajax({
                contentType: 'application/json',
                data: dataToSend,
                dataType: 'json',
                type: 'POST',
                url: '/Transaction/Invoice/',
                success: function (data)
                {
                    console.log(data);
                },
                error: function ()
                {
                    console.log(data);
                }
            });

        };

    }]);

});




