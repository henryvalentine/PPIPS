"use strict";

define(['application-configuration', 'standardReqService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngStandardReq', function ($compile)
    {
        return function ($scope, ngStandardReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ImportRequirement/GetImportRequirementObjects";
            tableOptions.itemId = 'ImportRequirementId';
            tableOptions.columnHeaders = ['DocumentTypeName'];
            var ttc = tableManager($scope, $compile, ngStandardReq, tableOptions, 'Add Requirement', 'prepareStandardReqTemplate', 'getStandardReq', 'deleteStandardReq', 150);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('standardReqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'standardReqService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, standardReqService, $location)
    {

        $scope.prepareStandardReqTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/StandardReq/ProcessStandardReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getStandardReq = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            standardReqService.getImportRequirement(impId, $scope.getStandardReqCompleted);
        };

        $scope.getStandardReqCompleted = function (response)
        {
            if (response.ImportRequirementId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.standardReq = response;
           $scope.standardReq =
           {
               'ImportRequirementId': response.ImportRequirementId, 'DocumentType':
                   { 'DocumentTypeId': response.DocumentTypeId, 'Name': response.DocumentTypeName }
           };
            $scope.edit = true;
            $scope.add = false;

            $scope.standardReq.Header = 'Update Standard Requirement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/StandardReq/ProcessStandardReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processStandardReq = function ()
        {
            if ($scope.standardReq == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req =
           {
               'ImportRequirementId': $scope.standardReq.ImportRequirementId,
               'DocumentTypeId': $scope.standardReq.DocumentType.DocumentTypeId
           };
            
            if ($scope.add)
            {
                standardReqService.addImportRequirement(req, $scope.processStandardReqCompleted);
            }
            else
            {
                standardReqService.editImportRequirement(req, $scope.processStandardReqCompleted);
            }
            
        };
        
        $scope.processStandardReqCompleted = function (data)
        {
            if (data.Code < 1) {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/StandardReq/ProcessStandardReq.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };
       
        $scope.initializeController = function ()
        {
            standardReqService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {
            $scope.docTypes = data.DocumentTypes;

        };

        $scope.initializeReq = function ()
        {
           $scope.standardReq =
           {
               'ImportRequirementId': '', 'DocumentType':
                   { 'DocumentTypeId': '', 'Name': 'Select Document Type' }
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.standardReq.Header = 'Add Standard Requirement';
            $scope.buttonText = "Add";
        };

        $scope.deleteStandardReq = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                standardReqService.deleteImportRequirement(id, $scope.deleteStandardReqCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteStandardReqCompleted = function (data)
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




