"use strict";

define(['application-configuration', 'jettyService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngJetty', function ($compile)
    {
        return function ($scope, ngJetty)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Jetty/GetJettyObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'PortName'];
            var ttc = tableManager($scope, $compile, ngJetty, tableOptions, 'Add Jetty', 'prepareJettyTemplate', 'getJetty', 'deleteJetty', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('jettyController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'jettyService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, jettyService, $location)
    {

        $scope.prepareJettyTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/Jetty/ProcessJetty.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getJetty = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            jettyService.getJetty(impId, $scope.getJettyCompleted);
        };

        $scope.getJettyCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.jetty = response;

            $scope.jetty =
{
    'Id': $scope.jetty.Id,
    'Name': $scope.jetty.Name,

    'Port': { 'Id': response.PortId, 'PortName': response.Port }



};

            $scope.edit = true;
            $scope.add = false;

            $scope.jetty.Header = 'Update Jetty';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Jetty/ProcessJetty.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processJetty = function ()
        {
            if ($scope.jetty == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req = {
                Id: $scope.jetty.Id,
                Name: $scope.jetty.Name,
                PortId: $scope.jetty.Port.Id
               
            };
            
            if ($scope.add)
            {
                jettyService.addJetty(req, $scope.processJettyCompleted);
            }
            else
            {
               jettyService.editJetty(req, $scope.processJettyCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Jetties');
            jettyService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.ports = data.Ports;
           
        };

        $scope.initializeReq = function ()
        {
           $scope.jetty =
           {
               'Id': '',
               'Name': '',
               'Port': { 'Id': '', 'Name': 'Select Port' }
              
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.jetty.Header = 'Add Jetty';
            $scope.buttonText = "Add";
        };

        $scope.processJettyCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Jetty/ProcessJetty.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteJetty = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                jettyService.deleteJetty(id, $scope.deleteJettyCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteJettyCompleted = function (data)
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




