"use strict";

define(['application-configuration', 'applicationContentService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngApplicationContent', function ($compile)
    {
        return function ($scope, ngApplicationContent)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/ApplicationContent/GetApplicationContentObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Title', 'Href', 'IsInUseStr'];
            var ttc = tableManager($scope, $compile, ngApplicationContent, tableOptions, 'Add Content', 'prepareApplicationContentTemplate', 'getApplicationContent', 'deleteApplicationContent', 117);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
  
    app.register.controller('applicationContentController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'applicationContentService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, applicationContentService, $location)
    {
        $scope.initializeReq = function () {
            $scope.applicationContent =
                        {
                            'Id': '',
                            'Title': '',
                            'BodyContent': '',
                            'Href': '',
                            'IsInUse': ''
                        };

            $scope.add = true;
            $scope.edit = false;
            $scope.applicationContent.Header = 'Add Content';
            $scope.buttonText = "Add";
        };

        $scope.prepareApplicationContentTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/ApplicationContents/ProcessApplicationContent.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getApplicationContent = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            applicationContentService.getApplicationContent(impId, $scope.getApplicationContentCompleted);
        };
        
        $scope.getApplicationContentCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.applicationContent = response;
            
            $scope.edit = true;
            $scope.add = false;

            $scope.applicationContent.Header = 'Update Content';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/ApplicationContents/ProcessApplicationContent.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.processApplicationContent = function ()
        {
            if ($scope.applicationContent == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.applicationContent.Title == null || $scope.applicationContent.Title.trim().length < 1)
            {
                alert('Please provide Content Title.');
                return;
            }
            if ($scope.applicationContent.BodyContent == null || $scope.applicationContent.BodyContent.trim() < 1) {
                alert('Please provide Content Body');
                return;
            }
            if ($scope.applicationContent.Href == null || $scope.applicationContent.Href.trim() < 1)
            {
                alert('Please provide Content Link/Url');
                return;
            }
            
           var applicationContent =
           {
               'Id': $scope.applicationContent.Id,
               'Title': $scope.applicationContent.Title,
               'BodyContent': $scope.applicationContent.BodyContent,
               'Href': $scope.applicationContent.Href,
               'IsInUse': $scope.applicationContent.IsInUse
           }; 
           
            if ($scope.add)
            {
                applicationContentService.addApplicationContent(applicationContent, $scope.processApplicationContentCompleted);
            }
            else
            {
                applicationContentService.editApplicationContent(applicationContent, $scope.processApplicationContentCompleted);
            }
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Content Management|DPR-PPIPS');
            $scope.initializeReq();
        };

       $scope.processApplicationContentCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/ApplicationContents/ProcessApplicationContent.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteApplicationContent = function (id)
        {
            if (parseInt(id) > 0)
            {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                applicationContentService.deleteApplicationContent(id, $scope.deleteApplicationContentCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteApplicationContentCompleted = function (data)
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




