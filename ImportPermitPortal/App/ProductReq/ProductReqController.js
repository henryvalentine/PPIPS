"use strict";

define(['application-configuration', 'productRequirementService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngProdReq', function ($compile)
    {
        return function ($scope, ngProdReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ProductRequirement/GetProductRequirementObjects";
            tableOptions.itemId = 'ProductRequirementId';
            tableOptions.columnHeaders = ['ProductName', 'DocumentTypeName'];
            var ttc = tableManager($scope, $compile, ngProdReq, tableOptions, 'Add Requirement', 'prepareProdReqTemplate', 'getProdReq', 'deleteProdReq', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('productReqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'productRequirementService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, productRequirementService, $location)
    {

        $scope.prepareProdReqTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/ProductReq/ProcessProdReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getProdReq = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            productRequirementService.getProductRequirement(impId, $scope.getProdReqCompleted);
        };

        $scope.getProdReqCompleted = function (response)
        {
            if (response.ProductRequirementId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.prodReq = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.prodReq.Header = 'Update Product Requirement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/ProductReq/ProcessProdReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processProdReq = function ()
        {
            if ($scope.prodReq == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req =
           {
               'ProductRequirementId': $scope.prodReq.ProductRequirementId,
               'ProductId': $scope.prodReq.Product.ProductId,
               'DocumentTypeId': $scope.prodReq.DocumentType.DocumentTypeId
           };
            
            if ($scope.add)
            {
                productRequirementService.addProductRequirement(req, $scope.processProdReqCompleted);
            }
            else
            {
                productRequirementService.editProductRequirement(req, $scope.processProdReqCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Product Requirements');
            productRequirementService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.products = data.Products;
            $scope.docTypes = data.DocumentTypes;

        };

        $scope.initializeReq = function ()
        {
           $scope.prodReq =
           {
               'ProductRequirementId': '', 'Product': {'ProductId' : '', 'Name' : 'Select Product'}, 'DocumentType': 
                   { 'DocumentTypeId': '', 'Name': 'Select Document Type' }
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.prodReq.Header = 'Add Product Requirement';
            $scope.buttonText = "Add";
        };

        $scope.processProdReqCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/ProductReq/ProcessProdReq.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteProdReq = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                productRequirementService.deleteProductRequirement(id, $scope.deleteProdReqCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteProdReqCompleted = function (data)
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




