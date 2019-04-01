"use strict";

define(['application-configuration', 'ngDialog', 'receiptService'], function (app)
{
    app.register.directive('ngReceipts', function ($compile)
    {
        return function ($scope, ngReceipts)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Transaction/GetMyReceipts";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['RRR', 'DateAddedStr', 'AmountPaidStr', 'PaymentOption', 'ServiceDescription'];
            var ttc = receiptTableManager($scope, $compile, ngReceipts, tableOptions, 'getReceipt');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    }); 
                                 
  
    app.register.controller('myReceiptController', ['$scope', '$rootScope', '$routeParams', 'receiptService', '$location',
    function ($scope, $rootScope, $routeParams, receiptService, $location)
    {
        $rootScope.setPageTitle('My Payment History|DPR-PPIPS');
        
        $scope.getReceipt = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
           receiptService.getReceipt(appId, $scope.getReceiptCompleted);
        };
        
        $scope.getReceiptCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Receipt information could not be retrieved. Please try again later.');
                return;
            }

           receiptService.setInfo(response);
            $location.path('/Receipts/Receipt');
        };

    }]);

});





