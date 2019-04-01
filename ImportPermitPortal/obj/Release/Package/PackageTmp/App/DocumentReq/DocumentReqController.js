"use strict";

define(['application-configuration', 'documentRequirementService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngDocReq', function ($compile)
    {
        return function ($scope, ngDocReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/DocumentsRequirement/GetDocumentsRequirementObjects";
            tableOptions.itemId = 'DocumentsRequirementId';
            tableOptions.columnHeaders = ['ImportStageName', 'DocumentTypeName'];
            var ttc = tableManager($scope, $compile, ngDocReq, tableOptions, 'Add Requirement', 'prepareDocReqTemplate', 'getDocReq', 'deleteDocReq', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('documentReqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'documentRequirementService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, documentRequirementService, $location)
    {

        $scope.prepareDocReqTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/DocumentReq/ProcessDocReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getDocReq = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            documentRequirementService.getDocumentsRequirement(impId, $scope.getDocReqCompleted);
        };

        $scope.getDocReqCompleted = function (response)
        {
            if (response.DocumentRequirementId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.docReq = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.docReq.Header = 'Update Document Requirement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/DocumentReq/ProcessDocReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processDocReq = function ()
        {
            if ($scope.docReq == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.docReq.ImportStage.ImportStageId < 1)
            {
                alert('Please select Application Stage.');
                return;
            }
            if ($scope.docReq.DocumentType.DocumentTypeId < 1)
            {
                alert('Please select Document Type.');
                return;
            }
           var docReq =
           {
               'DocumentRequirementId': $scope.docReq.DocumentRequirementId,
               'ImportStageId': $scope.docReq.ImportStage.ImportStageId,
               'DocumentTypeId': $scope.docReq.DocumentType.DocumentTypeId
           };
            
            if ($scope.add)
            {
                documentRequirementService.addDocumentsRequirement(docReq, $scope.processDocReqCompleted);
            }
            else
            {
                documentRequirementService.editDocumentsRequirement(docReq, $scope.processDocReqCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Document Requirements');
            documentRequirementService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.appStages = data.ImportStages;
            $scope.docTypes = data.DocumentTypes;

        };

        $scope.initializeReq = function ()
        {
           $scope.docReq =
           {
               'DocumentRequirementId': '', 'ImportStage': { 'ImportStageId': '', 'Name': 'Select Application Stage' }, 'DocumentType':
                   { 'DocumentTypeId': '', 'Name': 'Select Document Type' }
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.docReq.Header = 'Add Document Requirement';
            $scope.buttonText = "Add";
        };

        $scope.processDocReqCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/DocumentReq/ProcessDocReq.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteDocReq = function (id)
        {
            if (parseInt(id) > 0)
            {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }

                documentRequirementService.deleteDocumentsRequirement(id, $scope.deleteDocReqCompleted);

            }
            else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteDocReqCompleted = function (data)
        {
            if(data.Code < 1)
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




