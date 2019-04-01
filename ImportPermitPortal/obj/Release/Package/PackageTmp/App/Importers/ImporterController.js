"use strict";

define(['application-configuration', 'importerService','ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    
    app.register.controller('importerController', ['$scope', '$rootScope', '$routeParams', 'importerService', '$location',
    function ($scope, $rootScope, $routeParams, importerService, $location)
    {
       
        $scope.initializeController = function () 
        {
            $rootScope.setPageTitle('Importer Details|DPR-PPIPS');
            $scope.processing = false;
            $scope.isEditInitiated = false;
            $scope.bizCategory = "CAC Registration Number *";
            $scope.getCompanyInformation();
            $scope.company =
            {
                'CompanyId': '', 'ParentId': '', 'RCNumber': '', 'TIN': '', 'Name': '', 'StructureId': '', 'LogoPath': '', 'BusinessCommencementDate': '',
                'IsActive': '', 'DateAdded': '', 'BusinessCommencementDateStr': '', 'ShortNme': '', 'TotalStaff': '', 'TotalExpatriate': '',
                'CompanyAddressObjects': [{
                    'AddressId': '', 'AddressLine1': '', 'AddressLine2': '', 'LastUpdated': '', 'CityId': '', 'AddressTypeId': 1, 'CompanyId': '', 'IsRegisteredSameAsOperational': '', 'tempId': 1,
                    'CityObject': { 'CityId': '', 'Name': '-- Select City --' }
                }]
            };

            var importerId = $routeParams.id;

            if (parseInt(importerId) < 1) 
            {
                $location.path('Importers/Importers');
            }
            $scope.importerId = importerId;
            $scope.getCompanyInformation(importerId);
        };
        

        $scope.setAppSuccess = function (msg) {
            $scope.isError = false;
            $scope.appError = '';
            $scope.appSuccess = msg;
            $scope.isSuccess = true;
        };
        $scope.setAppError = function (msg) {
            $scope.appError = msg;
            $scope.isError = true;
            $scope.isSuccess = false;
            $scope.appSuccess = '';
        };
        
        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 1;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDateWithWeekends($scope, '', miniDate);

        $scope.getCompanyInformation = function (id)
        {
            importerService.getGeneralInformation(id, $scope.getCompanyInformationComplete);
        };

        $scope.getCompanyInformationComplete = function (data)
        {
            if (data == null || data.CpompanyId)
            {
                $scope.setAppError('Error: Please refresh the page and try again.');
                return;
            }

            $scope.company = data;

            $scope.appCount = data.AppCountObject.ApplicationCount;
            $scope.ntfCount = data.AppCountObject.NotificationCount;
            $scope.ptCount = data.AppCountObject.ExpiringPermitCount;
            $scope.rctCount = data.AppCountObject.RecertificationCount;

            $scope.apphref = 'ngs.html#Importers/ImporterApplications/' + $scope.importerId;
            $scope.ntfhref = 'ngs.html#Importers/ImporterNotifications/' + $scope.importerId;
            $scope.ptfhref = 'ngs.html#Importers/ImporterPermits/' + $scope.importerId;
            $scope.rtfhref = 'ngs.html#Importers/ImporterRecertifications/' + $scope.importerId;
          
            $scope.company.BusinessCommencementDate = data.BusinessCommencementDateStr;
	
            if ($scope.company.CompanyAddressObjects != null && $scope.company.CompanyAddressObjects.length > 0) {
                $scope.cityProvided = true;
                angular.forEach($scope.company.CompanyAddressObjects, function (s, i)
                {
                    angular.forEach($rootScope.countries, function (h, j)
                    {
                        angular.forEach(h.CityObjects, function (t, p)
                        {
                            if (s.CityId === t.CityId)
                            {
                                s.CountryObject = h;
                                s.CityObject = t;
                            }
                        });
                    });
                });
            } else {
                $scope.cityProvided = false;
                $scope.company.CompanyAddressObjects = [
                    {
                        'AddressId': '',
                        'AddressLine1': '',
                        'AddressLine2': '',
                        'LastUpdated': '',
                        'CityId': '',
                        'AddressTypeId': 1,
                        'tempId' : 1,
                        'CompanyId': '',
                        'IsRegisteredSameAsOperational': '',
                        'CityObject': { 'CityId': '', 'Name': '-- Select City --' }
                    }
                ];
            }
           
            $scope.company.Structure = { 'StructureId': data.StructureId, 'Name': data.StructureName };

        };

        $scope.setAppSuccess = function (msg)
        {
            $scope.isError = false;
            $scope.appError = '';
            $scope.appSuccess = msg;
            $scope.isSuccess = true;
        };

        $scope.toggleHolder = function (sel)
        {
            if (sel > 0)
            {
                if (sel === 2)
                {
                    $scope.bizCategory = "CAC Business Number *";
                } else
                {
                    $scope.bizCategory = "CAC Registration Number *";
                }
            }

        }

        $scope.setAppError = function (msg) 
        {
            $scope.appError = msg;
            $scope.isError = true;
            $scope.isSuccess = false;
            $scope.appSuccess = '';
        };

        $scope.moreAddress = function () 
        {
            if ($scope.company.CompanyAddressObjects.length > 1)
            {
                return;
            }

            var tempId = 1;
            
            if ($scope.company.CompanyAddressObjects.length > 0)
            {
                tempId = $scope.company.CompanyAddressObjects.length + 1;
            }
            var newAdd = {
                'AddressId': '', 'AddressLine1': '', 'AddressLine2': '', 'LastUpdated': '', 'CityId': '', 'AddressTypeId': '2', 'CompanyId': '', 'IsRegisteredSameAsOperational': '',
                'CityObject': { 'CityId': '', 'Name': '-- Select City --' }, 'tempId': tempId
            }
            $scope.company.CompanyAddressObjects.push(newAdd);

        };

        $scope.removeAddress = function (itemId)
        {
            if (itemId < 1)
            {
                alert('Invalid selection');
                return;
            }
            
            angular.forEach($scope.company.CompanyAddressObjects, function (item, index)
            {
                if (item.tempId === itemId)
                {
                    $scope.tempId = itemId;
                    if ($scope.company.CompanyAddressObjects.length === 1)
                    {
                        if (item.AddressId !== null && item.AddressId !== undefined && item.AddressId !== NaN && item.AddressId > 0)
                        {
                            alert('This Address should not be deleted.');
                            return;
                        }
                    }
                    else
                    {
                        if (item.AddressId !== null && item.AddressId !== undefined && item.AddressId !== NaN && item.AddressId > 0)
                        {
                            if (!confirm("This Address information will be removed from the list. Continue?"))
                            {
                                return;
                            }
                            importerService.deleteAddress($scope.removeAddressCompleted);

                        } else {
                            $scope.company.CompanyAddressObjects.splice(index, 1);
                            $scope.tempId = null;
                        }
                    }
                }
            });
        };

        $scope.removeAddressCompleted = function (data)
        {
            $scope.processing = false;
            if (data.Code < 1)
            {
                $scope.setAppError(data.Error);
                return;
            }
           
            angular.forEach($scope.company.CompanyAddressObjects, function (item, index)
            {
                if (item.tempId === $scope.tempId)
                {
                    $scope.company.CompanyAddressObjects.splice(index, 1);
                    $scope.tempId = null;
                }
            });
            $scope.setAppSuccess(data.Error);
        };
        
        $scope.processCompanyInformation = function ()
        {
            if ($scope.company.CompanyAddressObjects.length < 1)
            {
                $scope.setAppError("Please add at least one Address.");
                return;
            }

            if ($scope.company.CompanyId < 1)
            {
                $scope.setAppError("FATAL ERROR: Please refresh the page and try again.");
                return;
            }

            if ($scope.company.RCNumber == null || $scope.company.RCNumber.length < 1)
            {
                $scope.setAppError("Please provide " + $scope.bizCategory);
                return;
            }

            if ($scope.company.TIN == null || $scope.company.TIN.length < 1)
            {
                $scope.setAppError("Please provide Tax Indentification Number");
                return;
            }
            if ($scope.company.Name == null || $scope.company.Name.length < 1) {
                $scope.setAppError("Please provide Company Name");
                return;
            }
            if ($scope.company.Structure == null || $scope.company.Structure.StructureId < 1)
            {
                $scope.setAppError("Please Select Business Type");
                return;
            }

            if ($scope.company.Structure == null || $scope.company.Structure.StructureId < 1)
            {
                $scope.setAppError("Please Select Business Type");
                return;
            }
          var company =
          {
              'CompanyId': $scope.company.CompanyId, 'ParentId': $scope.company.ParentId, 'RCNumber': $scope.company.RCNumber, 'TIN': $scope.company.TIN, 'Name': $scope.company.Name, 'StructureId': $scope.company.Structure.StructureId, 'LogoPath': $scope.company.LogoPath, 'BusinessCommencementDate': $scope.company.BusinessCommencementDate,
              'IsActive':  $scope.company.IsActive, 'DateAdded':  $scope.company.DateAdded, 'BusinessCommencementDateStr':  $scope.company.BusinessCommencementDateStr, 'ShortNme':  $scope.company.ShortNme, 'TotalStaff':  $scope.company.TotalStaff, 'TotalExpatriate':  $scope.company.TotalExpatriate
          };
          company.CompanyAddressObjects = [];
          angular.forEach($scope.company.CompanyAddressObjects, function (item, index)
          {
              company.CompanyAddressObjects.push({
                  'AddressId': item.AddressId, 'AddressLine1': item.AddressLine1, 'AddressLine2': item.AddressLine2, 'LastUpdated': item.LastUpdated, 'CityId': item.CityObject.CityId, 'AddressTypeId': item.AddressTypeId, 'CompanyId': item.CompanyId, 'IsRegisteredSameAsOperational': item.IsRegisteredSameAsOperational, 'CityName': item.CityObject.Name
              });

          });
            
            $scope.processing = true;
            importerService.processGeneralInformation(company, $scope.processCompanyInformationCompleted);
        };

        $scope.processCompanyInformationCompleted = function (data)
        {
            $scope.processing = false;
            if (data.Code < 1)
            {
                $scope.setAppError(data.Error);
                return;
            }

            $scope.isEditInitiated = false;
            $scope.setAppSuccess(data.Error);

        };
      
    }]);
    
});


