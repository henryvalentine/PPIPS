"use strict";

define(['application-configuration'], function (app)
{
    
    app.register.controller('transactionController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams,  $location)
    {
        $rootScope.setPageTitle('Query Transaction|DPR-PPIPS');

        $scope.getTransaction = function (rrr)
        {
            if (rrr == null || rrr.length < 1)
            {
                alert('Please provide an RRR.');
                return;
            }

            $rootScope.getQuery('/Transaction/GetTransactionDetails?rrr=' + rrr, $scope.getTransactionCompleted);
        };

        ngDialog.close('/App/Banks/ProcessBank.html', '');

        $scope.getTransactionCompleted = function (data)
        {
            $scope.invoice = null;
            if (data == null || data.Id < 1)
            {
                alert('Transaction details could not be retrieved. Please try again later.');
                return;
            }

            $scope.invoice = data;

        };

        $scope.generateHash = function ()
        {
            $rootScope.getQuery('/Application/GenerateHash?orderId=' + $scope.orderId + '&url=' + $scope.url, $scope.getTransactionCompleted);
        };

        $scope.generateHashCompleted = function (hash)
        {
            if (hash == null || hash.length < 1)
            {
                alert('Hash could not be generated.');
                return;
            }

            angular.element('#hash').val(hash);
            angular.element('#uri').val($scope.url);
            angular.element('#orderId').val($scope.orderId);

            alert('hash generated!');
            document.getElementById('rrfmt').submit();
        };
        
        $scope.printReceipt = function ()
        {
            var printContents = document.getElementById('notificationReceipt').innerHTML;

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
                    '<link rel="stylesheet" type="text/css" href="/Content/bootstrap.css"  />' +
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
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css"  /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin.document.close();
            }
            popupWin.document.close();

            return true;
        }

    }]);

   
    
});


