"use strict";

define(['application-configuration', 'countryService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngCountry', function ($compile)
    {
        return function ($scope, ngCountry)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Country/GetCountryObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'CountryCode','RegionName'];
            var ttc = tableManager($scope, $compile, ngCountry, tableOptions, 'Add Country', 'prepareCountryTemplate', 'getCountry', 'deleteCountry', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('countryController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'countryService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, countryService, $location)
    {

        $scope.prepareCountryTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/Country/ProcessCountry.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getCountry = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            countryService.getCountry(impId, $scope.getCountryCompleted);
        };

        $scope.getCountryCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.country = response;

            $scope.country =
            {
                'Id': $scope.country.Id,
                'Name': $scope.country.Name,
                'CountryCode': $scope.country.CountryCode,
                'Region': { 'Id': response.RegionId, 'RegionName': response.Region }

    

            };

            $scope.edit = true;
            $scope.add = false;

            $scope.country.Header = 'Update Country';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Country/ProcessCountry.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processCountry = function ()
        {
            if ($scope.country == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req = {
                Id: $scope.country.Id,
                Name: $scope.country.Name,
                CountryCode: $scope.country.Code,
                RegionId: $scope.country.Region.Id
               
            };
            
            if ($scope.add)
            {
                countryService.addCountry(req, $scope.processCountryCompleted);
            }
            else
            {
               countryService.editCountry(req, $scope.processCountryCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Countries|DPR-PPIPS');
            countryService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.regions = data.Regions;
           
        };

        $scope.initializeReq = function ()
        {
           $scope.country =
           {
               'Id': '',
               'Name': '',
               'Region': { 'Id': '', 'Name': 'Select Region' }
              
           };
            $scope.add = true;
            $scope.edit = false;
            $scope.country.Header = 'Add Country';
            $scope.buttonText = "Add";
        };

        $scope.processCountryCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Country/ProcessCountry.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteCountry = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                countryService.deleteCountry(id, $scope.deleteCountryCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteCountryCompleted = function (data)
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







