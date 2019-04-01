"use strict";

define(['application-configuration', 'storageProviderTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngProd', function ($compile)
    {
        return function ($scope, ngProd)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/StorageProviderType/GetStorageProviderTypeObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'Requirements'];
            var ttc = productsTableManager($scope, $compile, ngProd, tableOptions, 'Add Storage Provider Type', 'prepareProdTemplate', 'getStorageProvider', 'deleteProd', 210);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('storageProviderTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'storageProviderTypeService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, storageProviderTypeService, $location)
    {
        $scope.prepareProdTemplate = function ()
        {
            $scope.initialize();
            ngDialog.open({
                template: '/App/StorageProviderRequirements/ProcessStorageProviderReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getStorageProvider = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            storageProviderTypeService.getStorageProviderType(impId, $scope.getStorageProviderCompleted);
        };

        $scope.getStorageProviderCompleted = function (response)
        {
            if(response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initialize();

            $scope.prod = response;
            
            if (response.StorageProviderRequirementObjects != null && response.StorageProviderRequirementObjects.length > 0) {
                
                angular.forEach(response.StorageProviderRequirementObjects, function (doc, i)
                {
                    angular.forEach($scope.docTypes, function (g, p)
                    {
                        if (doc.DocumentTypeId === g.DocumentTypeId)
                        {
                            g.ticked = true;
                        }
                    });
                });
            }
            
            $scope.edit = true;
            $scope.add = false;

            $scope.prod.Header = 'Update Storage Provider Type';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/StorageProviderRequirements/ProcessStorageProviderReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processProd = function ()
        {
             if ($scope.prod == null)
             {
                 alert('An error was encountered. Please refresh the page and try again.');
                 return;
             }

             if ($scope.prod.Name == null || $scope.prod.Name.length < 1)
             {
                 alert('Please provide Storage Provider Type.');
                 return;
             }

            var storageProviderType =
            {
                'Id': $scope.prod.Id,
                'Name': $scope.prod.Name,
                'StorageProviderRequirementObjects': []
            };

            if ($scope.prodReqs != null && $scope.prodReqs.length > 0)
            {
                angular.forEach($scope.prodReqs, function (doc, i)
                {
                    storageProviderType.StorageProviderRequirementObjects.push(
                        {
                            'StorageProviderTypeId': doc.StorageProviderTypeId,
                            'Id': doc.Id,
                            'DocumentTypeId': doc.DocumentTypeId
                        });
                });
            }
          
            if ($scope.add)
            {
                storageProviderTypeService.addStorageProviderType(storageProviderType, $scope.processProdCompleted);
            }
            else
            {
                storageProviderTypeService.editStorageProviderType(storageProviderType, $scope.processProdCompleted);
            }

        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Storage Provider Requirments');
            storageProviderTypeService.getDocTypes($scope.getDocTypesCompleted);
        };

        $scope.getDocTypesCompleted = function (data)
        {
           $scope.docTypes = data;
        };

        $scope.initialize = function ()
        {
            $scope.prod = {
                'Id': '',
                'Name': ''
            };
            $scope.prodReqs = [];
            $scope.add = true;
            $scope.edit = false;
            $scope.prod.Header = 'Add Storage Provider Type';
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
                ngDialog.close('/App/StorageProviderRequirements/ProcessStorageProviderReq.html', '');
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
            });
        };
     
    }]);

});




