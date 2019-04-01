"use strict";

define(['application-configuration', 'groupService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngGroup', function ($compile)
    {
        return function ($scope, ngGroup)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Group/GetGroupObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngGroup, tableOptions, 'Add Group', 'prepareGroupTemplate', 'getGroup', 'deleteGroup', 175);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });
    
    app.register.controller('groupController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'groupService', '$upload', 'fileReader', '$http', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, groupService, $upload, fileReader, $location)
    {

        $scope.prepareGroupTemplate = function () {
            $scope.initializeController();
            ngDialog.open({
                template: '/App/Group/ProcessGroup.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getGroup = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            groupService.getGroup(impId, $scope.getGroupCompleted);
        };

        $scope.getGroupCompleted = function (response)
        {
            if (response.GroupId < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeController();

            $scope.Group = response;
            $scope.edit = true;
            $scope.add = false;

            $scope.Group.Header = 'Update Group';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Group/ProcessGroup.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processGroup = function ()
        {
            if ($scope.Group == null || $scope.Group.Name.length < 1)
            {
                alert('Please provide Group Name');
                return;
            }
            
            if ($scope.add)
            {
                groupService.addGroup($scope.Group, $scope.processGroupCompleted);
            }
            else
            {
                groupService.editGroup($scope.Group, $scope.processGroupCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('User Groups');
            $scope.Group =
           {
               'Id': '', 'Name': ''
           };
            $scope.Group.Header = "New Group";
            $scope.buttonText = "Add";
            $scope.add = true;
            $scope.edit = false;
        };

        $scope.processGroupCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Group/ProcessGroup.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeController();
            }
        };

        $scope.deleteGroup = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Group will be deleted permanently. Continue?"))
                {
                    return;
                }
                groupService.deleteGroup(id, $scope.deleteGroupCompleted);
            } else {
                alert('Invalid selection.');
            }
        };

        $scope.deleteGroupCompleted = function (data)
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




