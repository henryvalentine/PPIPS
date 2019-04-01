"use strict";

define(['application-configuration', 'documentTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngDocType', function ($compile)
    {
        return function ($scope, ngDocType)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/DocumentType/GetDocumentTypeObjects";
            tableOptions.itemId = 'DocumentTypeId';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngDocType, tableOptions, 'Add Document Type', 'prepareDocTypeTemplate', 'getDocType', 'deleteDocType', 175);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
    
    app.register.controller('documentTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'documentTypeService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, documentTypeService, $upload, fileReader, $location)
    {

        $scope.prepareDocTypeTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/DocumentType/ProcessDocumentType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getDocType = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            documentTypeService.getDocumentType(impId, $scope.getDocTypeCompleted);
        };

        $scope.getDocTypeCompleted = function (response)
        {
            if (response.DocumentTypeId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.documentType = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.documentType.Header = 'Update Document Type';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/DocumentType/ProcessDocumentType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processDocType = function ()
        {
            if ($scope.documentType == null || $scope.documentType.Name.length < 1)
            {
                alert('Please provide Document Type Name');
                return;
            }
            
            if ($scope.add)
            {
                documentTypeService.addDocumentType($scope.documentType, $scope.processDocTypeCompleted);
            }
            else
            {
                documentTypeService.editDocumentType($scope.documentType, $scope.processDocTypeCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Document Types');
            $scope.documentType =
           {
               'DocumentTypeIdId': '', 'Name': ''
           };
            $scope.documentType.Header = "New Document Type";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processDocTypeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/DocumentType/ProcessDocumentType.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteDocType = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This  Document Type will be deleted permanently. Continue?"))
                {
                    return;
                }
                documentTypeService.deleteDocumentType(id, $scope.deleteDocTypeCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteDocTypeCompleted = function (data)
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




