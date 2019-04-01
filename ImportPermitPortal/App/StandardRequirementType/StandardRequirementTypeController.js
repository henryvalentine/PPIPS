"use strict";

define(['application-configuration', 'standardReqTypeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngSReqType', function ($compile)
    {
        return function ($scope, ngSReqType)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/StandardRequirementType/GetStandardRequirementTypeObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngSReqType, tableOptions, 'Add Requirement Type', 'prepareDocTypeTemplate', 'getDocType', 'deleteDocType', 175);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
    
    app.register.controller('standardReqTypeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'standardReqTypeService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, standardReqTypeService, $upload, fileReader, $location)
    {

        $scope.prepareDocTypeTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/StandardRequirementType/ProcessStandardRequirementType.html',
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
            standardReqTypeService.getStandardRequirementType(impId, $scope.getDocTypeCompleted);
        };

        $scope.getDocTypeCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.standardReqType = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.standardReqType.Header = 'Update Standard Requirement Type';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/StandardRequirementType/ProcessStandardRequirementType.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processDocType = function ()
        {
            if ($scope.standardReqType == null || $scope.standardReqType.Name.length < 1)
            {
                alert('Please provide Standard Requirement Type Name');
                return;
            }
            
            if ($scope.add)
            {
                standardReqTypeService.addStandardRequirementType($scope.standardReqType, $scope.processDocTypeCompleted);
            }
            else
            {
                standardReqTypeService.editStandardRequirementType($scope.standardReqType, $scope.processDocTypeCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Standard Requirement Types|DPR-PPIPS');
            $scope.standardReqType =
           {
               'Id': '', 'Name': ''
           };
            $scope.standardReqType.Header = "New Standard Requirement Type";
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
                ngDialog.close('/App/StandardRequirementType/ProcessStandardRequirementType.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteDocType = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This  Standard Requirement Type will be deleted permanently. Continue?"))
                {
                    return;
                }
                standardReqTypeService.deleteStandardRequirementType(id, $scope.deleteDocTypeCompleted);
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




