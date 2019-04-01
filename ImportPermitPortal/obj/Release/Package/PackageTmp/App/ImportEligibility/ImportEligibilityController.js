"use strict";

define(['application-configuration', 'importEligibilityService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngImpel', function ($compile)
    {
        return function ($scope, ngImpel)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ImportEligibility/GetImportEligibilityObjects";
            tableOptions.itemId = 'ImportEligibilityId';
            tableOptions.columnHeaders = ['Shortname'];
            var ttc = tableManager($scope, $compile, ngImpel, tableOptions, 'Add Eligibility', 'prepareImpTemplate', 'getImp', 'deleteImp', 123);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('importElController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'importEligibilityService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, importEligibilityService, $location)
    {

        $scope.prepareImpTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/ImportEligibility/ProcessImportEligibility.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getImp = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            importEligibilityService.getImportEligibility(impId, $scope.getImpCompleted);
        };

        $scope.getImpCompleted = function (response)
        {
            if (response.ImportEligibilityId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.impEl = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.impEl.Header = 'Update Import Eligibility';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/ImportEligibility/ProcessImportEligibility.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processImportEligibility = function ()
        {
            if ($scope.impEl == null || $scope.impEl.Shortname.length < 1)
            {
                alert('Please provide Eligibility Short Name');
                return;
            }
            
            if ($scope.add)
            {
                importEligibilityService.addImportEligibility($scope.impEl, $scope.processImportEligibilityCompleted);
            }
            else
            {
                importEligibilityService.editImportEligibility($scope.impEl, $scope.processImportEligibilityCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $scope.impEl =
           {
               'ImportEligibilityIdId': '', 'Shortname': '', 'Description': ''
           };
            $scope.impEl.Header = "Add Import Eligibility";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processImportEligibilityCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/ImportEligibility/ProcessImportEligibility.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteImp = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Import Eligibility information will be deleted permanently. Continue?")) {
                    return;
                }
                importEligibilityService.deleteImportEligibility(id, $scope.deleteImpCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteImpCompleted = function (data)
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




