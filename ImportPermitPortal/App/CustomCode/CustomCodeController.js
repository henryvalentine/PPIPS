"use strict";

define(['application-configuration', 'customCodeService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngCCode', function ($compile)
    {
        return function ($scope, ngCCode)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/CustomCode/GetCustomCodeObjects";
            tableOptions.itemId = 'CustomCodeId';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngCCode, tableOptions, 'Add Custom Code', 'prepareCCodeTemplate', 'getCCode', 'deleteCCode', 158);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('customCodeController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'customCodeService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, customCodeService, $location)
    {

        $scope.prepareCCodeTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/CustomCode/ProcessCustomCode.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getCCode = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            customCodeService.getCustomCode(impId, $scope.getCCodeCompleted);
        };

        $scope.getCCodeCompleted = function (response)
        {
            if (response.CustomCodeId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.cCode = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.cCode.Header = 'Update Custom Code';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/CustomCode/ProcessCustomCode.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.initializeController = function () {
            $rootScope.setPageTitle('Custom Codes');
            $scope.cCode =
           {
               'CustomCodeIdId': '', 'Name': ''
           };
            $scope.cCode.Header = "Add Custom Code";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processCustomCode = function ()
        {
            if ($scope.cCode == null || $scope.cCode.Name.length < 1)
            {
                alert('Please provide Custom Code');
                return;
            }
            
            if ($scope.add)
            {
                customCodeService.addCustomCode($scope.cCode, $scope.processCustomCodeCompleted);
            }
            else
            {
                customCodeService.editCustomCode($scope.cCode, $scope.processCustomCodeCompleted);
            }
            
        };

        $scope.processCustomCodeCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/CustomCode/ProcessCustomCode.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteCCode = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Custom Code information will be deleted permanently. Continue?")) {
                    return;
                }
                customCodeService.deleteCustomCode(id, $scope.deleteCCodeCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteCCodeCompleted = function (data)
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




