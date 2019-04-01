"use strict";

define(['application-configuration', 'feeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngFee', function ($compile)
    {
        return function ($scope, ngFee)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Fee/GetFeeObjects";
            tableOptions.itemId = 'FeeId';
            tableOptions.columnHeaders = ['FeeTypeName', 'ImportStageName', 'Amount'];
            var ttc = tableManager($scope, $compile, ngFee, tableOptions, 'Add Fee', 'prepareFeeTemplate', 'getFee', 'deleteFee', 95);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('feeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'feeService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, feeService, $location)
    {
        $scope.prepareFeeTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/Fee/ProcessFee.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getFee = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            feeService.getFee(impId, $scope.getFeeCompleted);
        };
        
        $scope.getFeeCompleted = function (response)
        {
            if (response.FeeId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.fee = response;

            angular.forEach($scope.feeTypes, function (o, i)
            {
                if (o.FeeTypeId === response.FeeTypeId) {
                    $scope.fee.FeeType = o;
                }
            });
            angular.forEach($scope.ImportStages, function (o, i) {
                if (o.Id === response.ImportStageId) {
                    $scope.fee.ImportStage = o;
                }
            });

            $scope.edit = true;
            $scope.add = false;

            $scope.fee.Header = 'Update Fee|DPR-PPIPS';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Fee/ProcessFee.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processFee = function ()
        {
            if ($scope.fee == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

           var req =
           {
               'FeeId': $scope.fee.FeeId,
               'FeeTypeId': $scope.fee.FeeType.FeeTypeId,
               'ImportStageId': $scope.fee.ImportStage.Id,
               'Amount': $scope.fee.Amount,
               'PrincipalSplit': $scope.fee.PrincipalSplit,
               'VendorSplit': $scope.fee.VendorSplit,
               'PaymentGatewaySplit': $scope.fee.PaymentGatewaySplit,
               'BillableToPrincipal': $scope.fee.BillableToPrincipal,
               'Description': $scope.fee.Description
           };
            
            if ($scope.add)
            {
                feeService.addFee(req, $scope.processFeeCompleted);
            }
            else
            {
                feeService.editFee(req, $scope.processFeeCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Fees');
            feeService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.feeTypes = data.FeeTypes;
            $scope.ImportStages = data.ImportStages;
        };

        $scope.initializeReq = function ()
        {
            $scope.fee =
                {
                    'FeeId': '',
                    'FeeType': { 'FeeTypeId': '', 'Name': '-- Select Fee Type --' },
                    'ImportStage': { 'Id': '', 'Name': '-- Select Import Stage --' },
                    'Amount': '',
                    'PrincipalSplit': '',
                    'VendorSplit': '',
                    'PaymentGatewaySplit': '',
                    'BillableToPrincipal': false,
                    'Description': ''
              };

            $scope.add = true;
            $scope.edit = false;
            $scope.fee.Header = 'Add Fee';
            $scope.buttonText = "Add";
        };

        $scope.processFeeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Fee/ProcessFee.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteFee = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                feeService.deleteFee(id, $scope.deleteFeeCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteFeeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);

            }
            else
            {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };
     
    }]);

});




