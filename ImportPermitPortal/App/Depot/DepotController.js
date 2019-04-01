"use strict";

define(['application-configuration', 'depotService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngDepot', function ($compile)
    {
        return function ($scope, ngDepot)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Depot/GetDepotObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name', 'JettyName', 'LastName'];
            var ttc = depotTableManager($scope, $compile, ngDepot, tableOptions, 'Add Depot', 'prepareDepotTemplate', 'getDepot', 105);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
     
    app.register.controller('depotController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'depotService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, depotService, $location)
    {
        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 1;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDateWithWeekends($scope, '', miniDate);

        setEndDateWithWeekends($scope, miniDate, '');


        $scope.prepareDepotTemplate = function ()
        {
            $scope.initializeModel();
            ngDialog.open({
                template: '/App/Depot/ProcessDepot.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getJetties = function ()
        {
            depotService.getJetties($scope.getJettiesCompleted);
        };

        $scope.getJettiesCompleted = function (data)
        {
            $scope.jetties = data;
        };

        $scope.getDepot = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            depotService.getDepot(impId, $scope.getDepotCompleted);
        };

        $scope.getDepotCompleted = function (response)
        {
            if (response.DepotId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();
            $scope.UserProfileObject = response;
            angular.forEach($scope.jetties, function (g, l)
            {
                if (g.Id === $scope.UserProfileObject.JettyId)
                {
                    $scope.UserProfileObject.Jetty = g;
                }
            });
           
            $scope.edit = true;
            $scope.add = false;

            $scope.UserProfileObject.IssueDate = response.IssueDateStr;
            $scope.UserProfileObject.ExpiryDate = response.ExpiryDateStr; 

            $scope.UserProfileObject.Header = 'Update Depot Information';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Depot/ProcessDepot.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };
       
        $scope.initializeController = function () {
            $rootScope.setPageTitle('Depot List|DPR-PPIPS');
            $scope.initializeModel();
            $scope.getJetties();
        };

         $scope.initializeModel = function ()
        {
            $scope.UserProfileObject =
            {
                'Id' : '',
                'JettyId': '',
                'Jetty' : {'Id' : '', 'Name' : '-- Select Jetty --'},
                'JettyName' : '',
                'CompanyName' : '',
                'DepotId' : '',
                'UserId' : '',
                'PhoneNumber' : '',
                'CompanyId' : '',
                'PersonId' : '',
                'FirstName' : '',
                'LastName' : '',
                'Email': '',
                'DepotLicense' : '',
                'IssueDate' : '',
                'ExpiryDate' : '',
                'Status' : '',
                'IsActive' : ''
            };

            $scope.add = true;
            $scope.edit = false;
            $scope.UserProfileObject.Header = 'Add Depot';
            $scope.buttonText = "Add";
        };
        
        $scope.processDepot = function () {
            if ($scope.UserProfileObject.CompanyName == null || $scope.UserProfileObject.CompanyName.length < 1)
            {
                alert('Please provide depot Name.');
                return;
            }

            if ($scope.UserProfileObject.Jetty.Id == null || $scope.UserProfileObject.Jetty.Id < 1 || $scope.UserProfileObject.Jetty.Id === undefined || $scope.UserProfileObject.Jetty.Id === NaN) {
                alert('Please select a Jetty.');
                return;
            }

            if ($scope.UserProfileObject.DepotLicense == null || $scope.UserProfileObject.DepotLicense === undefined || $scope.UserProfileObject.DepotLicense.length < 1) {
                alert('Please Provide Depot License.');
                return;
            }

            if ($scope.UserProfileObject.IssueDate == null || $scope.UserProfileObject.IssueDate === undefined || $scope.UserProfileObject.IssueDate.length < 1) {
                alert('Please Provide License Issue Date.');
                return;
            }

            if ($scope.UserProfileObject.ExpiryDate== null || $scope.UserProfileObject.ExpiryDate === undefined || $scope.UserProfileObject.ExpiryDate.length < 1) {
                alert('Please Provide License Expiry Date.');
                return;
            }
            
            var userProfileObject =
                                    {
                                        'Id': $scope.UserProfileObject.Id,
                                        'JettyId': $scope.UserProfileObject.Jetty.Id,
                                        'JettyName': $scope.UserProfileObject.Name,
                                        'CompanyName': $scope.UserProfileObject.CompanyName,
                                        'DepotId': $scope.UserProfileObject.Id,
                                        'UserId': $scope.UserProfileObject.UserId,
                                        'PhoneNumber': $scope.UserProfileObject.PhoneNumber,
                                        'CompanyId': $scope.UserProfileObject.CompanyId,
                                        'PersonId': $scope.UserProfileObject.PersonId,
                                        'FirstName': $scope.UserProfileObject.FirstName,
                                        'LastName': $scope.UserProfileObject.LastName,
                                        'Email': $scope.UserProfileObject.Email,
                                        'DepotLicense': $scope.UserProfileObject.DepotLicense,
                                        'IssueDate': $scope.UserProfileObject.IssueDate,
                                        'ExpiryDate': $scope.UserProfileObject.ExpiryDate,
                                        'Status': $scope.UserProfileObject.Status,
                                        'IsActive': $scope.UserProfileObject.IsActive
                                    };

            

            if (!$scope.validateDepotUser(userProfileObject)) {
                return;
            }

            if ($scope.add) {
                depotService.addDepot(userProfileObject, $scope.processDepotCompleted);
            }
            else {
                depotService.editDepot(userProfileObject, $scope.processDepotCompleted);
            }

        };

        $scope.processDepotCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Depot/ProcessDepot.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteDepot = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                depotService.deleteDepot(id, $scope.deleteDepotCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteDepotCompleted = function (data)
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

        $scope.validateDepotUser = function (user)
        {
            if (user.FirstName == null || user.FirstName.length < 1)
            {
                alert('Please provide User First Name');
                return false;
            }

            if (user.LastName == null || user.LastName.length < 1) {
                alert('Please provide User Last Name');
                return false;
            }

            if (user.Email == null || user.Email.length < 1) {
                alert('Please provide User Email');
                return false;
            }

            if (user.PhoneNumber == null || user.PhoneNumber.length < 1) {
                alert('Please provide User Phone Number');
                return false;
            }
            
            return true;
        };

        $scope.validatePassword = function (user) {

            if (user.Password == null || user.Password.length < 1) {
                alert('Please provide User Password');
                return false;
            }

            if (user.ConfirmPassword == null || user.ConfirmPassword.length < 1) {
                alert('Please confirm User Password');
                return false;
            }

            if (user.ConfirmPassword.length < 8 || user.Password.length < 8) {
                alert('Both passwords should not be less than 8 alphanumeric characters');
                return false;
            }

            if (user.ConfirmPassword !== user.Password) {
                alert('The passwords do not match');
                return false;
            }
            return true;
        };

    }]);

});




