"use strict";

define(['application-configuration', 'jettyMappingService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngJettyMapping', function ($compile)
    {
        return function ($scope, ngJettyMapping)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/JettyMapping/GetJettyMappingObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['JettyName', 'ZoneName'];
            var ttc = tableManager($scope, $compile, ngJettyMapping, tableOptions, 'Add JettyMapping', 'prepareJettyMappingTemplate', 'getJettyMapping', 'deleteJettyMapping', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('jettyMappingController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'jettyMappingService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, jettyMappingService, $location)
    {

        $scope.prepareJettyMappingTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/JettyMapping/ProcessJettyMapping.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getJettyMapping = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            jettyMappingService.getJettyMapping(impId, $scope.getJettyMappingCompleted);
        };

        $scope.getJettyMappingCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.jettyMapping = response;


            $scope.jettyMapping =
{
    'Id': $scope.jettyMapping.Id,
   
    'Jetty': { 'Id': response.JettyId, 'JettyName': response.Port },

    'Zone': { 'Id': response.ZoneId, 'ZoneName': response.Zone }

};

            

            $scope.edit = true;
            $scope.add = false;

            $scope.jettyMapping.Header = 'Update JettyMapping';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/JettyMapping/ProcessJettyMapping.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processJettyMapping = function ()
        {
            if ($scope.jettyMapping == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req = {
                Id: $scope.jettyMapping.Id,
               
                JettyId: $scope.jettyMapping.Jetty.Id,

                ZoneId: $scope.jettyMapping.Zone.Id
               
            };
            
            if ($scope.add)
            {
                jettyMappingService.addJettyMapping(req, $scope.processJettyMappingCompleted);
            }
            else
            {
               jettyMappingService.editJettyMapping(req, $scope.processJettyMappingCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Jetties Mappings');
            jettyMappingService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {
            $scope.jettys = data.Jetties;
            $scope.zones = data.Zones;

        };

        $scope.initializeReq = function ()
        {
           $scope.jettyMapping =
           {
               'Id': '',
              
               'Jetty': { 'Id': '', 'Name': 'Select Jetty' },

               'Zone': { 'Id': '', 'Name': 'Select Zone' }
              
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.jettyMapping.Header = 'Add JettyMapping';
            $scope.buttonText = "Add";
        };

        $scope.processJettyMappingCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/JettyMapping/ProcessJettyMapping.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteJettyMapping = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                jettyMappingService.deleteJettyMapping(id, $scope.deleteJettyMappingCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteJettyMappingCompleted = function (data)
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




