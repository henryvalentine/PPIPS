"use strict";

define(['application-configuration', 'importClassService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngImpClass', function ($compile)
    {
        return function ($scope, ngImpClass)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ImportClass/GetImportClassObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngImpClass, tableOptions, 'Add Import Class', 'prepareImportClassTemplate', 'getImportClass', 'deleteImportClass', 175);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
    
    app.register.controller('importClassController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'importClassService', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, importClassService)
    {
        $scope.prepareImportClassTemplate = function ()
        {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/ImportClasses/ProcessImportClass.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getImportClass = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            importClassService.getImportClass(impId, $scope.getImportClassCompleted);
        };

        $scope.getImportClassCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.importClass = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.importClass.Header = 'Update Import Class';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/ImportClasses/ProcessImportClass.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processImportClass = function ()
        {
            if ($scope.importClass == null || $scope.importClass.Name.length < 1)
            {
                alert('Please provide Import Class Name');
                return;
            }
            
            if ($scope.add)
            {
                importClassService.addImportClass($scope.importClass, $scope.processImportClassCompleted);
            }
            else
            {
                importClassService.editImportClass($scope.importClass, $scope.processImportClassCompleted);
            }
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Import Classes|DPR-PPIPS');
            $scope.importClass =
           {
               'Id': '', 'Name': '', 'Description' : ''
           };
            $scope.importClass.Header = "New Import Class";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processImportClassCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/ImportClasses/ProcessImportClass.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteImportClass = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This  Import Class will be deleted permanently. Continue?"))
                {
                    return;
                }
                importClassService.deleteImportClass(id, $scope.deleteImportClassCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteImportClassCompleted = function (data)
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




