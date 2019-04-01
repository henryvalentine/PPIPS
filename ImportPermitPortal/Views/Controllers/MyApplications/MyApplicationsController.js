"use strict";

define(['application-configuration', 'applicationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.controller('myApplicationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'applicationService', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, applicationService, $upload, fileReader,$http)
    {
        $scope.initializeController = function () {

            $scope.Apps = [];
            $scope.getApps();
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
        
        
        $scope.getApps = function ()
        {
            $http({ method: 'GET', url: '/Application/MyApplications' }).success(function (response)
            {
                $scope.Apps = response;
            });

        };
        //$scope.getProductsCompleted = function (data)
        //{
        //    $scope.products = data;

        //};

    }]);
    
});




