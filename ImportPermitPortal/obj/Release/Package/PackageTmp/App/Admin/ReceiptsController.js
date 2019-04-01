"use strict";

define(['application-configuration', 'ngDialog', 'receiptService'], function (app)
{
    app.register.directive('ngAdReceipts', function ($compile)
    {
        return function ($scope, ngAdReceipts)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Transaction/GetReceipts";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReceiptNoStr','RRR', 'ImporterName', 'TotalAmountPaidStr', 'DateAddedStr', 'PaymentOption', 'ServiceDescription'];
            var ttc = receiptTableManager($scope, $compile, ngAdReceipts, tableOptions, 'getReceipt');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
                                    
   app.register.controller('myReceiptController', ['$scope', '$rootScope', '$routeParams', 'receiptService', '$location',
    function ($scope, $rootScope, $routeParams, receiptService, $location)
    {
        $rootScope.setPageTitle('Payment History|DPR-PPIPS');
        
        $scope.getReceipt = function (appId)
        {
            if (parseInt(appId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            receiptService.getReceiptInfo(appId, $scope.getReceiptCompleted);
        };
        
        $scope.getReceiptCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Receipt information could not be retrieved. Please try again later.');
                return;
            }

           receiptService.setInfo(response);
            $location.path('/Admin/Receipt');
        };

    }]);

});





