"use strict";

define(['application-configuration', 'importReqService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngImpReq', function ($compile)
    {
        return function ($scope, ngImpReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ImportRequirement/GetImportRequirementObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ImportStageName', 'Requirements'];
            var ttc = productsTableManager($scope, $compile, ngImpReq, tableOptions, 'Add Requirement', 'prepareReqTemplate', 'getReq', 'deleteReq', 149);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    }); 

    app.register.controller('importReqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'importReqService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, importReqService, $location)
    {

        $scope.prepareReqTemplate = function ()
        {
            $scope.initialize();
            ngDialog.open({
                template: '/App/ImportRequirements/ProcessImportRequirementReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getReq = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            importReqService.getImportRequirement(impId, $scope.getReqCompleted);
        };

        $scope.getReqCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initialize();

           $scope.prod =
          {
              'Id': response.Id,
              'ImportStageId': response.ImportStageId,
              'DocumentTypeId': '',
              'ImportStageObject': { 'Id': response.ImportStageId, 'Name': response.ImportStageName },
              'DocumentTypeObjects': response.DocumentTypeObjects
          };
            
            if (response.DocumentTypeObjects != null && response.DocumentTypeObjects.length > 0)
            {
                
                angular.forEach(response.DocumentTypeObjects, function (doc, i)
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

            $scope.prod.Header = 'Update Requirement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/ImportRequirements/ProcessImportRequirementReq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processReq = function ()
        {
           
             if ($scope.prod == null)
             {
                 alert('An error was encountered. Please refresh the page and try again.');
                 return;
             }

             if ($scope.prodReqs == null || $scope.prodReqs.length < 1)
             {
                 alert('Please add at least one Requirement.');
                 return;
             }

             if ($scope.prod.ImportStageObject == null || $scope.prod.ImportStageObject.Id < 1)
             {
                 alert('Please Select Import Stage.');
                 return;
             }
            
            var reqs = [];
            if ($scope.prodReqs != null && $scope.prodReqs.length > 0)
            {
                angular.forEach($scope.prodReqs, function (doc, i)
                {
                    reqs.push(
                        {
                            'Id': $scope.prod.Id,
                            'ImportStageId': $scope.prod.ImportStageObject.Id,
                            'DocumentTypeId': doc.DocumentTypeId
                        });
                });
            }
          
            if ($scope.add)
            {
                importReqService.addImportRequirement(reqs, $scope.processReqCompleted);
            }
            else
            {
                importReqService.editImportRequirement(reqs, $scope.processReqCompleted);
            }

        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Import Requirements|DPR-PPIPS');
            importReqService.getList($scope.getListCompleted);
        };

        $scope.getListCompleted = function (data)
        {
            $scope.docTypes = data.DocumentTypes;
            $scope.importStages = data.ImportStages;
        };

        $scope.initialize = function ()
        {
           $scope.prod =
           {
                'Id': '',
                'ImportStageId': '',
                'DocumentTypeId': '',
                'ImportStageObject': {'Id' : '', 'Name' : '-- Select Import Stage --'},
                'DocumentTypeObjects': []
            };
            $scope.prodReqs = [];
            $scope.add = true;
            $scope.edit = false;
            $scope.prod.Header = 'Add Requirement';
            $scope.buttonText = "Add";
        };

        $scope.processReqCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/ImportRequirements/ProcessImportRequirementReq.html', '');
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




