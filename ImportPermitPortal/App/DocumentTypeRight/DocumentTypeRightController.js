"use strict";

define(['application-configuration', 'documentTypeRightService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngDoc', function ($compile)
    {
        return function ($scope, ngDoc)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/DocumentTypeRight/GetDocumentsRequirementObjects";
            tableOptions.itemId = 'RoleId';
            tableOptions.columnHeaders = ['RoleName', 'DocumentTypeName'];
            var ttc = tableManagerNoDelete($scope, $compile, ngDoc, tableOptions, 'Add Document Right', 'prepareDocTypeRightTemplate', 'getDocTypeRight', 'deleteDocTypeRight', 170);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
 
    app.register.controller('documentTypeRightController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'documentTypeRightService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, documentTypeRightService, $location)
    {
        $scope.prepareDocTypeRightTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/DocumentTypeRight/ProcessDocTypeRight.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getDocTypeRight = function (roleId)
        {
            if (roleId == null || roleId.length < 1)
            {
                alert('Invalid selection!');
                return;
            }
            documentTypeRightService.getDocumentTypeRight(roleId, $scope.getDocTypeRightCompleted);
        };

        $scope.getDocTypeRightCompleted = function (response)
        {
            if (response == null || response.length < 1)
            {
                alert('Items could not be retrieved. Please try again.');
                return;
            }

            $scope.aspNetRole = { 'Id': response[0].RoleId, 'Name': response[0].RoleName }
            angular.forEach(response, function (doc, i)
            {
                angular.forEach($scope.docTypes, function (g, p)
                {
                    if (doc.DocumentTypeId === g.DocumentTypeId)
                    {
                        g.ticked = true;
                        g.RoleId = doc.RoleId;
                    }
                });
            });


            //if (response.Permission.indexOf("Write") > -1)
            //{
            //    $scope.write = true;
            //}

            //if (response.Permission.indexOf("Read") > -1)
            //{
            //    $scope.read = true;
            //}

            $scope.edit = true;
            $scope.add = false;

            $scope.header = 'Update Document Right';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/DocumentTypeRight/ProcessDocTypeRight.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processDocTypeRight = function ()
        {
            if ($scope.doctTypeRights.length < 1)
            {
                alert('Please select at least one Document Type.');
                return;
            }
            
            if ($scope.aspNetRole.Id < 1)
            {
                alert('Please select a Role.');
                return;
            }
            var permission = '';
            if ($scope.read) {
                permission = 'Read';
            }

            if ($scope.write) {
                permission += ', Write';
            }

            if (!$scope.read && !$scope.write) {
                alert('Please provide at least one permission.');
                return;
            }

            var reqs = [];
            angular.forEach($scope.doctTypeRights, function (g, p) {
                reqs.push({
                    'DocumentTypeRightId': '',
                    'RoleId': $scope.aspNetRole.Id,
                    'Permission': permission,
                    'DocumentTypeId': g.DocumentTypeId
                });
            });
            
            if ($scope.add)
            {
                documentTypeRightService.addDocumentTypeRight(reqs, $scope.processDocTypeRightCompleted);
            }
            else
            {
                documentTypeRightService.editDocumentTypeRight(reqs, $scope.processDocTypeRightCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Documents Rights');
            documentTypeRightService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data)
        {
            $scope.roles = data.Roles;
            $scope.docTypes = data.DocumentTypes;

        };

        $scope.initializeReq = function ()
        {
            $scope.doctTypeRights = {};
            $scope.aspNetRole = { 'Id': '', 'Name': '-- Select Role --' }
            $scope.add = true;
            $scope.edit = false;
            $scope.header = 'Add Document Right(s)';
            $scope.buttonText = "Add";
            angular.forEach($scope.docTypes, function (g, p) {
                g.ticked = false;

            });
        };

        $scope.processDocTypeRightCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/DocumentTypeRight/ProcessDocTypeRight.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
                
                angular.forEach($scope.docTypes, function (g, p)
                {
                     g.ticked = false;
                    
                });
            }
        };

        $scope.deleteDocTypeRight = function (id)
        {
            if (parseInt(id) > 0)
            {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                documentTypeRightService.deleteDocumentTypeRight(id, $scope.deleteDocTypeRightCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteDocTypeRightCompleted = function (data)
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




