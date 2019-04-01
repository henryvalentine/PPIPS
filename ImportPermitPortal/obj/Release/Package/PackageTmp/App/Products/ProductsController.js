"use strict";

define(['application-configuration', 'productService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngProd', function ($compile)
    {
        return function ($scope, ngProd)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Product/GetProductObjects";
            tableOptions.itemId = 'ProductId';
            tableOptions.columnHeaders = ['Name', 'Code', 'AvailableStr', 'Requirements'];
            var ttc = productsTableManager($scope, $compile, ngProd, tableOptions, 'Add Product', 'prepareProdTemplate', 'getProd', 'deleteProd', 118);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('productController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'productService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, productService, $location)
    {

        $scope.prepareProdTemplate = function ()
        {
            $scope.initialize();
            ngDialog.open({
                template: '/App/Products/ProcessProduct.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getProd = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            productService.getProduct(impId, $scope.getProdCompleted);
        };

        $scope.getProdCompleted = function (response)
        {
            if (response.ProductId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initialize();

            $scope.prod = response;
            
            if (response.ProductDocumentRequirementObjects != null && response.ProductDocumentRequirementObjects.length > 0) {
                
                angular.forEach(response.ProductDocumentRequirementObjects, function (doc, i)
                {
                    angular.forEach($scope.docTypes, function (g, p)
                    {
                        if (doc.DocumentTypeId === g.DocumentTypeId)
                        {
                            g.ticked = true;
                            g.ProductRequirementId = doc.ProductRequirementId;
                        }
                    });
                });
            }
            
            $scope.edit = true;
            $scope.add = false;

            $scope.prod.Header = 'Update Product';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Products/ProcessProduct.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processProd = function () {
           
             if ($scope.prod == null)
             {
                 alert('An error was encountered. Please refresh the page and try again.');
                 return;
             }

             if ($scope.prod.Code== null || $scope.prod.Code.length < 1) {
                 alert('Please provide Product Code.');
                 return;
             }

             if ($scope.prod.Name == null || $scope.prod.Name.length < 1)
             {
                 alert('Please provide Product Name.');
                 return;
             }

            var product =
            {
                'ProductId': $scope.prod.ProductId,
                'Code': $scope.prod.Code,
                'Name': $scope.prod.Name,
                'Availability': $scope.prod.Availability,
                'ProductDocumentRequirementObjects': []
            };

            if ($scope.prodReqs != null && $scope.prodReqs.length > 0)
            {
                angular.forEach($scope.prodReqs, function (doc, i)
                {
                    product.ProductDocumentRequirementObjects.push({
                        'ProductRequirementId': doc.ProductRequirementId,
                        'ProductId': product.ProductId,
                        'DocumentTypeId': doc.DocumentTypeId
                    });
                });
            }
          
            if ($scope.add)
            {
                productService.addProduct(product, $scope.processProdCompleted);
            }
            else
            {
                productService.editProduct(product, $scope.processProdCompleted);
            }

        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Products|DPR-PPIPS');
            productService.getDocTypes($scope.getDocTypesCompleted);
        };

        $scope.getDocTypesCompleted = function (data)
        {
           $scope.docTypes = data;
        };

        $scope.initialize = function ()
        {
            $scope.prod = {
                'ProductId': '',
                'Code': '',
                'Name': '',
                'Availability': ''
            };
            $scope.prodReqs = [];
            $scope.add = true;
            $scope.edit = false;
            $scope.prod.Header = 'Add Product';
            $scope.buttonText = "Add";
        };

        $scope.processProdCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Products/ProcessProduct.html', '');
                $scope.jtable.fnClearTable();
                $scope.initialize();
                $scope.clearSelections();
            }
        };


        $scope.clearSelections = function ()
        {
            angular.forEach($scope.docTypes, function (g, p)
            {
                g.ticked = false;
                g.ProductRequirementId = 0;
            });
        };
     
    }]);

});




