"use strict";

define(['application-configuration', 'feeTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngFeeType', function ($compile)
    {
        return function ($scope, ngFeeType)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/FeeType/GetFeeTypeObjects";
            tableOptions.itemId = 'FeeTypeId';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngFeeType, tableOptions, 'Add Fee Type', 'prepareFeeTypeTemplate', 'getFeeType', 'deleteFeeType', 175);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
    
    app.register.controller('feeTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'feeTypeService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, feeTypeService, $upload, fileReader, $location)
    {

        $scope.prepareFeeTypeTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/FeeType/ProcessFeeType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getFeeType = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            feeTypeService.getFeeType(impId, $scope.getFeeTypeCompleted);
        };

        $scope.getFeeTypeCompleted = function (response)
        {
            if (response.FeeTypeId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.feeType = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.feeType.Header = 'Update Fee Type';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/FeeType/ProcessFeeType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processFeeType = function ()
        {
            if ($scope.feeType == null || $scope.feeType.Name.length < 1)
            {
                alert('Please provide Fee Type');
                return;
            }
            
            if ($scope.add)
            {
                feeTypeService.addFeeType($scope.feeType, $scope.processFeeTypeCompleted);
            }
            else
            {
                feeTypeService.editFeeType($scope.feeType, $scope.processFeeTypeCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Fee Types|DPR-PPIPS');
            $scope.feeType =
           {
               'FeeTypeIdId': '', 'Name': ''
           };
            $scope.feeType.Header = "New Fee Type";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processFeeTypeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/FeeType/ProcessFeeType.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteFeeType = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This  Fee Type will be deleted permanently. Continue?"))
                {
                    return;
                }
                feeTypeService.deleteFeeType(id, $scope.deleteFeeTypeCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteFeeTypeCompleted = function (data)
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




