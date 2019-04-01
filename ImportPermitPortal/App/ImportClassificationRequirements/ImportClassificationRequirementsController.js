"use strict";

define(['application-configuration', 'importClassReqService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngIClassReq', function ($compile)
    {
        return function ($scope, ngIClassReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ImportClassificationRequirement/GetImportClassificationRequirementObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ImportClassName', 'ImportStageName', 'Requirements']; 
            var ttc = productsTableManager($scope, $compile, ngIClassReq, tableOptions, 'Add Requirement', 'prepareReqTemplate', 'getReq', 'deleteReq', 149);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('importClassReqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'importClassReqService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, importClassReqService, $location)
    {

        $scope.prepareReqTemplate = function ()
        {
            $scope.initialize();
            ngDialog.open({
                template: '/App/ImportClassificationRequirements/ProcessImportClassificationReq.html',
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
            importClassReqService.getImportClassificationRequirement(impId, $scope.getReqCompleted);
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
              'ClassificationId': response.ClassificationId,
              'ImportStageId': response.ImportStageId,
              'DocumentTypeId': '',
              'ImportStageObject': { 'Id': response.ImportStageId, 'Name': response.ImportStageName },
              'ImportClassObject': { 'Id': response.ClassificationId, 'Name': response.ImportClassName },
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
                template: '/App/ImportClassificationRequirements/ProcessImportClassificationReq.html',
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
            
             if ($scope.prod.ImportClassObject == null || $scope.prod.ImportClassObject.Id < 1)
             {
                 alert('Please Select Import Class.');
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
                            'ClassificationId': $scope.prod.ImportClassObject.Id,
                            'ImportStageId': $scope.prod.ImportStageObject.Id,
                            'DocumentTypeId': doc.DocumentTypeId
                        });
                });
            }
          
            if ($scope.add)
            {
                importClassReqService.addImportClassificationRequirement(reqs, $scope.processReqCompleted);
            }
            else
            {
                importClassReqService.editImportClassificationRequirement(reqs, $scope.processReqCompleted);
            }

        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Import Classification Requirements|DPR-PPIPS');
            importClassReqService.getList($scope.getListCompleted);
        };

        $scope.getListCompleted = function (data)
        {
            $scope.docTypes = data.DocumentTypes;
            $scope.importStages = data.ImportStages;
            $scope.classes = data.Classes;
        };

        $scope.initialize = function ()
        {
           $scope.prod =
           {
                'Id': '',
                'ClassificationId': '',
                'ImportStageId': '',
                'DocumentTypeId': '',
                'ImportStageObject': {'Id' : '', 'Name' : '-- Select Import Stage --'},
                'ImportClassObject': { 'Id': '', 'Name': '-- Select Import Class --' },
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
                ngDialog.close('/App/ImportClassificationRequirements/ProcessImportClassificationReq.html', '');
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




