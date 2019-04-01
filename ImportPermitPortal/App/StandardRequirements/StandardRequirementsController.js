"use strict";

define(['application-configuration', 'companyDocumentService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngDocReq', function ($compile)
    {
        return function ($scope, ngDocReq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/CompanyDocument/GetStandardRequirement";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Title', 'DocumentTypeName', 'ValidFromStr', 'ValidToStr'];
            var ttc = tableManager($scope, $compile, ngDocReq, tableOptions, 'Add Requirement', 'preparecompanyDocumentTemplate', 'getcompanyDocument', 'deletecompanyDocument', 150);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
    
    app.register.controller('standardRequirementController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'companyDocumentService', '$upload', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, companyDocumentService, $upload, $location)
    {
        var xcvb = new Date();
        var year = xcvb.getFullYear();
        var month = xcvb.getMonth() + 1;
        var day = xcvb.getDate();
        var miniDate = year + '/' + month + '/' + day;

        setMaxDateWithWeekends($scope, '', miniDate);

        setEndDateWithWeekends($scope, miniDate, '');

        $scope.preparecompanyDocumentTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/StandardRequirements/ProcessStandardRequirement.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };
        
        $scope.getcompanyDocument = function (id)
        {
            if (parseInt(id) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            companyDocumentService.getCompanyDocument(id, $scope.getcompanyDocumentCompleted);
        };

        $scope.getcompanyDocumentCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();
            $scope.doc = response;
            angular.forEach($scope.docTypes, function (n, m)
            {
                if(n.Id === $scope.doc.StandardRequirementTypeId)
                {
                    $scope.doc.DocumentType = n;
                }
            });

            $scope.doc.ValidFrom = response.ValidFromStr;
            $scope.doc.ValidTo = response.ValidToStr;
            $scope.edit = true;
            $scope.add = false;

            $scope.doc.Header = 'Update Standard Requirement';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/StandardRequirements/ProcessStandardRequirement.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processDoc = function ()
        {
            if ($scope.doc == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }
          
            if ($scope.doc.DocumentType.Id < 1)
            {
                alert('Please select Document Type.');
                return;
            }
            if ($scope.doc.Title == null || $scope.doc.Title.length < 1)
            {
                alert('Please provide Document Title.');
                return;
            }

            if ($scope.doc.ValidFrom == null || $scope.doc.ValidFrom.length < 1)
            {
                alert('Please provide Date Obtained.');
                return;
            }

            if ($scope.doc.DocumentType.Id === 3)
            {
                if ($scope.doc.ValidTo == null || $scope.doc.ValidTo.length < 1)
                {
                    alert('Please provide Expiry Date for the Tax Clearance.');
                    return;
                }
            }

            var doc =
            {
                'Id': $scope.doc.Id,
                'ImporterId': $scope.doc.ImporterId,
                'StandardRequirementTypeId': $scope.doc.DocumentType.Id,
                'DocumentPath': $scope.doc.DocumentPath,
                'ValidFrom': $scope.doc.ValidFrom,
                'ValidTo': $scope.doc.ValidTo,
                'LastUpdated': $scope.doc.LastUpdated,
                'Title': $scope.doc.Title,
                'TempPath': $scope.doc.TempPath,
               'DocumentTypeName': $scope.doc.DocumentType.Name
           };

           $scope.processing = true;
            
            if ($scope.add)
            {
                companyDocumentService.addCompanyDocument(doc, $scope.processcompanyDocumentCompleted);
            }
            else
            {
                companyDocumentService.editCompanyDocument(doc, $scope.processcompanyDocumentCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Company Documents|DPR-PPIPS');
            $scope.processing = false;
            companyDocumentService.getGenericList($scope.getGenericListCompleted);
        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.docTypes = data;

        };

        $scope.initializeReq = function () {
            $scope.doc =
            {
                'Id': '',
                'ImporterId': '',
                'StandardRequirementTypeId': '',
                'TempPath': '',
                'DocumentPath': '',
                'ValidFrom': '',
                'ValidTo': '',
                'LastUpdated': '',
                'Title': '',
                'DocumentTypeName': '',
                'DocumentType': { 'Id': '', 'Name': '-- Select Document Type --' }
            };

            $scope.add = true;
            $scope.edit = false;
            $scope.doc.Header = 'Add Standard Requirement';
            $scope.buttonText = "Add";
        };

        $scope.processcompanyDocumentCompleted = function (data)
        {
            $scope.processing = false;
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);

                ngDialog.close('/App/StandardRequirements/ProcessStandardRequirement.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deletecompanyDocument = function (id)
        {
            if (parseInt(id) > 0)
            {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }

                companyDocumentService.deleteCompanyDocument(id, $scope.deletecompanyDocumentCompleted);

            }
            else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deletecompanyDocumentCompleted = function (data)
        {
            if(data.Code < 1)
            {
                alert(data.Error);

            }
            else
            {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };
     
        $scope.ProcessDocument = function (e)
        {
            var el = (e.srcElement || e.target);
            if (el.files == null)
            {
                return;
            }
            var file = el.files[0];
            $scope.processing = true;
            $upload.upload({
                url: "/CompanyDocument/SaveTempFile",
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt)
           {
                $scope.trendy = 'verifying';
           }).success(function (data)
           {
               $scope.processing = false;
               if (data.Code < 1)
               {
                   $scope.trendy = 'notVerified';
               }

               else {
                   $scope.trendy = 'verified';
                   $scope.doc.TempPath = data.Path;
               }

           });


        };

    }]);

});




