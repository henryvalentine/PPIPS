"use strict";

define(['application-configuration', 'userAppService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.controller('companyAddressController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userAppService', '$upload', 'fileReader', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, userAppService, $upload, fileReader, $location)
    {
     
        $scope.moreAddress = function () {
            if ($scope.addressObjects.length > 1) {
                return;
            }

            var tempId = 1;

            if ($scope.addressObjects.length > 0) {
                tempId = $scope.addressObjects.length + 1;
            }
            var newAdd =
            {
                'AddressId': '', 'AddressLine1': '', 'AddressLine2': '', 'LastUpdated': '', 'CityId': '', 'AddressTypeId': '2', 'CompanyId': '', 'IsRegisteredSameAsOperational': '',
                'CityObject': { 'CityId': '', 'Name': '-- Select City --' }, 'tempId': tempId
            }
            $scope.addressObjects.push(newAdd);

        };

        $scope.initializeController = function ()
        {

            var appId = $routeParams.id;
            if (parseInt(appId) < 1) {
                alert('Sorry but something seems to be missing on the page. Please try again.');
                $location.path('Application/MyApplications');
            } else {
                $rootScope.setPageTitle('Company Address|DPR-PPIPS');
                $scope.processing = false;
                $scope.selId = appId;
                $scope.addressObjects =
                    [
                        {
                            'AddressId': '',
                            'AddressLine1': '',
                            'AddressLine2': '',
                            'LastUpdated': '',
                            'CityId': '',
                            'AddressTypeId': 1,
                            'CompanyId': '',
                            'IsRegisteredSameAsOperational': '',
                            'tempId': 1
                        }
                    ];
            }

        };

        $scope.removeAddress = function (itemId)
        {
            if (itemId < 1)
            {
                alert('Invalid selection');
                return;
            }

            angular.forEach($scope.addressObjects, function (item, index)
            {
                if (item.tempId === itemId)
                {
                    $scope.tempId = itemId;
                    if ($scope.addressObjects.length === 1)
                    {
                        alert('This Address should not be deleted.');
                        return;
                    }
                    else {
                       
                            $scope.addressObjects.splice(index, 1);
                            $scope.tempId = null;
                        
                    }
                }
            });
        };
        

        $scope.processCompanyAddress = function ()
        {
            if ($scope.addressObjects.length < 1)
            {
                alert("Please provide at least one Address.");
                return;
            }
            
            var addressObjects = [];
            angular.forEach($scope.addressObjects, function (item, index)
            {
                addressObjects.push({
                    'AddressId': item.AddressId, 'AddressLine1': item.AddressLine1, 'AddressLine2': item.AddressLine2, 'LastUpdated': item.LastUpdated, 'CityId': item.CityObject.CityId, 'AddressTypeId': item.AddressTypeId, 'CompanyId': '', 'IsRegisteredSameAsOperational': item.IsRegisteredSameAsOperational, 'CityName': item.CityObject.Name
                });
            });

            $scope.processing = true;
            userAppService.processCompanyAddresses(addressObjects, $scope.processCompanyAddressCompleted);
        };

        $scope.processCompanyAddressCompleted = function (data)
        {
            $scope.processing = false;
            if (data.Code < 1)
            {
                alert(data.Error);
                return;
            }

            $location.path('ApplicationDetail/BankerDetail/' + $scope.selId);
        };
        
    }]);
  
});




