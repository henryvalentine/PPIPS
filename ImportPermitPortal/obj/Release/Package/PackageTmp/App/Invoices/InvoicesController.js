"use strict";

define(['application-configuration', 'ngDialog', 'invoiceService', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngNtfs', function ($compile)
    {
        return function ($scope, ngNtfs)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Transaction/GetMyInvoices";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['RRR', 'DateAddedStr', 'TotalAmountDueStr', 'PaymentOption', 'ServiceDescription', 'StatusStr'];
            var ttc = receiptTableManager($scope, $compile, ngNtfs, tableOptions, 'getInvoice');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    }); 
                                 
  
    app.register.controller('myInvoiceController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'invoiceService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, invoiceService, $upload, fileReader, $http, $location)
    {
        $rootScope.setPageTitle('My Invoices|DPR-PPIPS');
        
        $scope.getInvoice = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            invoiceService.getInvoice(appId, $scope.getInvoiceCompleted);
        };
        
        $scope.getInvoiceCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Invoice information could not be retrieved. Please try again later.');
                return;
            }

            invoiceService.setInfo(response);
            $location.path('/Invoices/Invoice');
        };

    }]);

});




