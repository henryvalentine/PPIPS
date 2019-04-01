"use strict";

define(['application-configuration', 'vesselService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngVessel', function ($compile)
    {
        return function ($scope, ngVessel)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Vessel/GetVesselObjects";
            tableOptions.itemId = 'VesselId';
            tableOptions.columnHeaders = ['CompanyName', 'Name', 'CapacityStr'];
            var ttc = depotTableManager($scope, $compile, ngVessel, tableOptions, 'Add Vessel', 'prepareVesselTemplate', 'getVessel', 105);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
    
    app.register.controller('vesselController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'vesselService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, vesselService, $location)
    {
        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 1;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDateWithWeekends($scope, '', miniDate);

        setEndDateWithWeekends($scope, miniDate, '');


        $scope.prepareVesselTemplate = function ()
        {
            $scope.initializeModel();
            ngDialog.open({
                template: '/App/Vessels/ProcessVessel.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getJettiesCompleted = function (data)
        {
            $scope.jetties = data;
        };

        $scope.getVessel = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            vesselService.getVessel(impId, $scope.getVesselCompleted);
        };

        $scope.getVesselCompleted = function (response)
        {
            if (response.VesselId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();
            $scope.vessel = response;
          
            $scope.edit = true;
            $scope.add = false;

            $scope.vessel.IssueDate = response.IssueDateStr;
            $scope.vessel.ExpiryDate = response.ExpiryDateStr; 

            $scope.vessel.Header = 'Update Vessel Information';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Vessels/ProcessVessel.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Vessel List|DPR-PPIPS');
            $scope.initializeModel();
            $scope.getJetties();
        };

         $scope.initializeModel = function ()
         {
            $scope.vessel =
            {
                'VesselId' : '',
                'CompanyName' : '',
                'Capacity' : '',
                'Name': '',
                'DateAdded': '',
                'VesselLicense' : '',
                'IssueDate' : '',
                'ExpiryDate' : '',
                'Status' : ''
            };

            $scope.add = true;
            $scope.edit = false;
            $scope.vessel.Header = 'Add Vessel';
            $scope.buttonText = "Add";
        };
        
         $scope.processVessel = function ()
         {
            if ($scope.vessel.CompanyName == null || $scope.vessel.CompanyName.length < 1)
            {
                alert('Please provide Company Name.');
                return;
            }

            if ($scope.vessel.Name == null || $scope.vessel.Name.length < 1)
            {
                alert('Please provide vessel Name.');
                return;
            }

            if ($scope.vessel.Capacity < 1)
            {
                alert('Please provide vessel Name.');
                return;
            }

            if ($scope.vessel.VesselLicense == null || $scope.vessel.VesselLicense === undefined || $scope.vessel.VesselLicense.length < 1) {
                alert('Please Provide Vessel License.');
                return;
            }

            if ($scope.vessel.IssueDate == null || $scope.vessel.IssueDate === undefined || $scope.vessel.IssueDate.length < 1) {
                alert('Please Provide License Issue Date.');
                return;
            }

            if ($scope.vessel.ExpiryDate== null || $scope.vessel.ExpiryDate === undefined || $scope.vessel.ExpiryDate.length < 1) {
                alert('Please Provide License Expiry Date.');
                return;
            }
            
            var vessel =
             {
                 'VesselId': '',
                 'CompanyName': $scope.vessel.CompanyName,
                 'Capacity': $scope.vessel.Capacity,
                 'Name': $scope.vessel.Name,
                 'DateAdded': $scope.vessel.DateAdded,
                 'VesselLicense': $scope.vessel.VesselLicense,
                 'IssueDate': $scope.vessel.IssueDate,
                 'ExpiryDate': $scope.vessel.ExpiryDate,
                 'Status': $scope.vessel.Status
             };

            if ($scope.add) {
                vesselService.addVessel(vessel, $scope.processVesselCompleted);
            }
            else {
                vesselService.editVessel(vessel, $scope.processVesselCompleted);
            }

        };

        $scope.processVesselCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Vessels/ProcessVessel.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteVessel = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                vesselService.deleteVessel(id, $scope.deleteVesselCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteVesselCompleted = function (data)
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




