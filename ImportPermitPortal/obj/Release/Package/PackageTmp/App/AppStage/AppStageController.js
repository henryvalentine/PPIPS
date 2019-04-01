"use strict";

define(['application-configuration', 'importStageService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngAppStage', function ($compile)
    {
        return function ($scope, ngAppStage)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ImportStage/GetImportStageObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngAppStage, tableOptions, 'Add Import Stage', 'prepareAppStageTemplate', 'getAppStage', 'deleteAppStage', 175);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
    
    app.register.controller('appStageController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'importStageService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, importStageService)
    {

        $scope.prepareAppStageTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/AppStage/ProcessAppStage.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getAppStage = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            importStageService.getImportStage(impId, $scope.getAppStageCompleted);
        };

        $scope.getAppStageCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.appStage = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.appStage.Header = 'Update Import Stage';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/AppStage/ProcessAppStage.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processAppStage = function ()
        {
            if ($scope.appStage == null || $scope.appStage.Name.length < 1)
            {
                alert('Please provide Import Stage Name');
                return;
            }
            
            if ($scope.add)
            {
                importStageService.addImportStage($scope.appStage, $scope.processAppStageCompleted);
            }
            else
            {
                importStageService.editImportStage($scope.appStage, $scope.processAppStageCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Stage');
            $scope.appStage =
           {
               'Id': '', 'Name': ''
           };
            $scope.appStage.Header = "New Import Stage";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processAppStageCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/AppStage/ProcessAppStage.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteAppStage = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This  Import Stage will be deleted permanently. Continue?"))
                {
                    return;
                }
                importStageService.deleteImportStage(id, $scope.deleteAppStageCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteAppStageCompleted = function (data)
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




