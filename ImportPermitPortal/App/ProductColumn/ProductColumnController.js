"use strict";

define(['application-configuration', 'productColumnService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngProdColReq', function ($compile)
    {
        return function ($scope, ngProdColReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ProductColumn/GetProductColumnObjects";
            tableOptions.itemId = 'ProductColumnId';
            tableOptions.columnHeaders = ['ProductName', 'CustomCodeName'];
            var ttc = tableManager($scope, $compile, ngProdColReq, tableOptions, 'Add Requirement', 'prepareProductColTemplate', 'getProductCol', 'deleteProductCol', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('productColReqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'productColumnService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, productColumnService, $location)
    {

        $scope.prepareProductColTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/ProductColumn/ProcessProductColumn.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getProductCol = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            productColumnService.getProductColumn(impId, $scope.getProductColCompleted);
        };

        $scope.getProductColCompleted = function (response)
        {
            if (response.ProductColumnId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();
            
             $scope.prodReq =
             {
                 'ProductColumnId': response.ProductColumnId, 'Product': { 'ProductId': response.ProductId, 'Name': response.ProductName }, 'CustomCode':
                     { 'CustomCodeId': response.CustomCodeId, 'Name': response.CustomCodeName}
             };

            $scope.edit = true;
            $scope.add = false;

            $scope.prodReq.Header = 'Update Product Added Requirement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/ProductColumn/ProcessProductColumn.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processProductCol = function ()
        {
            if ($scope.prodReq == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req =
           {
               'ProductColumnId': $scope.prodReq.ProductColumnId,
               'ProductId': $scope.prodReq.Product.ProductId,
               'CustomCodeId': $scope.prodReq.CustomCode.CustomCodeId
           };
            
            if ($scope.add)
            {
                productColumnService.addProductColumn(req, $scope.processProductColCompleted);
            }
            else
            {
                productColumnService.editProductColumn(req, $scope.processProductColCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Product Added Features');
            productColumnService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.products = data.ProductObjects;
            $scope.customCodes = data.CustomCodes;
        }; 

        $scope.initializeReq = function ()
        {
           $scope.prodReq =
           {
               'ProductColumnId': '', 'Product': {'ProductId' : '', 'Name' : 'Select Product'}, 'CustomCode': 
                   { 'CustomCodeId': '', 'Name': 'Select Custom Code' }
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.prodReq.Header = 'Add Product Added Requirement';
            $scope.buttonText = "Add";
        };

        $scope.processProductColCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/ProductColumn/ProcessProductColumn.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteProductCol = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                productColumnService.deleteProductColumn(id, $scope.deleteProductColCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteProductColCompleted = function (data)
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




