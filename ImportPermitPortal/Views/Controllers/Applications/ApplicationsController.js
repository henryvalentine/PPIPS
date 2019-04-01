"use strict";

define(['application-configuration', 'applicationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.controller('applicationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'applicationService', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, applicationService, $upload, fileReader,$http)
    {
        $scope.initializeController = function ()
        {
            $scope.edit = false;
            $scope.add = true;
            $scope.buttonText = "Add Product";
            $scope.application =
            {
                'ApplicationId': '', 'Id': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '','DateApplied': '','LastModified': '',
                'Header': 'New Application', 'ApplicationItems': []
            };
            $scope.InitializeModel();
        };

        $scope.InitializeModel = function() 
        {
            $scope.ApplicationItem =
            {
                'ApplicationItemId': '',
                'ApplicationId': '',
                'ProductId': '',
                'EstimatedQuantity': '',
                'EstimatedValue': '',
                'CountryOfOriginName': '',
                'PortOfDischarge': '',
                'Product' : {'ProductId': '', 'Code': '', 'Availability': '', 'Name': '-- Select Product --'}
            };
            $scope.getProducts();
        };

        $scope.addNewProduct = function ()
        {
            $scope.InitializeModel();
            $scope.edit = false;
            $scope.add = true;
            $scope.buttonText = "Add Product";
            
            ngDialog.open({
                template: '/Views/Application/AddProduct.cshtml',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.addApp = function ()
        {
            if (parseInt($scope.ApplicationItem.Product.ProductId) < 1)
            {
                alert("ERROR: Please select a Product.");
                return;
            }

            if (parseInt($scope.ApplicationItem.EstimatedQuantity) < 1)
            {
                alert("ERROR: Please provide Estimated Quantity.");
                return;
            }

            if (parseInt($scope.ApplicationItem.EstimatedValue) < 1)
            {
                alert("ERROR: Please provide Estimated Value.");
                return;
            }
            
            if ($scope.ApplicationItem.CountryOfOriginName == undefined || $scope.ApplicationItem.CountryOfOriginName.length < 1 || $scope.ApplicationItem.CountryOfOriginName == null)
            {
                alert("ERROR: Please provide Port of Origin.");
                return;
            }

            if ($scope.ApplicationItem.DischargeDepotName == undefined || $scope.ApplicationItem.DischargeDepotName.length < 1 || $scope.ApplicationItem.DischargeDepotName == null)
            {
                alert("ERROR: Please provide Port of Discharge.");
                return;
            }

            $scope.application.ApplicationItems.push($scope.ApplicationItem);
        };
        $scope.getProduct = function (productId)
        {
            if (parseInt(productId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            $scope.InitializeModel();
            for (var i = 0; i < $scope.application.ApplicationItems.length; i++)
            {
                var appInfo = $scope.application.ApplicationItems[i];
                if (appInfo.Product.ProductId === parseInt(productId))
                {
                    $scope.ApplicationItem = appInfo;
                    $scope.edit = true;
                    $scope.add = false;
                    application.Header = 'Update Product Information';
                    $scope.buttonText = "Update Product";
                    ngDialog.open({
                        template: '/Views/Application/AddProduct.cshtml',
                        className: 'ngdialog-theme-flat',
                        scope: $scope
                    });
                }
            }
        };
        
        $scope.editProduct = function (productId)
        {
            if (parseInt(productId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            for (var i = 0; i < $scope.application.ApplicationItems.length; i++)
            {
                var appInfo = $scope.application.ApplicationItems[i];
                if (appInfo.Product.ProductId === parseInt(productId))
                {
                    appInfo = $scope.ApplicationItem;
                    appInfo.ProductId = $scope.ApplicationItem.ProductId.ProductId;
                    appInfo.EstimatedQuantity = $scope.ApplicationItem.EstimatedQuantity;
                    appInfo.EstimatedValue = $scope.ApplicationItem.EstimatedValue;
                    appInfo.CountryOfOriginName = $scope.ApplicationItem.CountryOfOriginName;
                    appInfo.DischargeDepotName = $scope.ApplicationItem.DischargeDepotName;
                    appInfo.Product.ProductId = $scope.ApplicationItem.Product.ProductId;
                    appInfo.Product.Name = $scope.ApplicationItem.Product.Name;
                    ngDialog.close('/Views/Application/AddProduct.cshtml', '');
                    $scope.InitializeModel();
                }
            }
        };
        
        $scope.processApplication = function ()
        {
            if ($scope.application.ApplicationItems.length < 1)
            {
                alert("ERROR: Please add at least one Product.");
                return;
            }
            
            if ($scope.application.ApplicationId == null || $scope.application.ApplicationId == '' || $scope.application.ApplicationId == 0 || $scope.application.ApplicationId < 1 || $scope.application.ApplicationId == undefined)
            {
                applicationService.addApplication($scope.application, $scope.processApplicationCompleted);
            }
            else
            {
                applicationService.editApplication(product, $scope.processApplicationCompleted);
            }
        };
        $scope.processApplicationCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Code);
            }
            else
            {
                alert(data.Error);
                $window.location = '/Application/NextStep?id=' + data.Code;
            }
        };
        $scope.getProducts = function ()
        {
            applicationService.getGenericList($scope.getProductsCompleted);

        };
        $scope.getProductsCompleted = function (data)
        {
            $scope.products = data;

        };

    }]);
    
});




