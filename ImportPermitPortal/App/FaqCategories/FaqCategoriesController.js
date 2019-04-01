"use strict";

define(['application-configuration', 'faqCategoryService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngFaqCategory', function ($compile)
    {
        return function ($scope, ngFaqCategory)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/FaqCategory/GetFaqCategoryObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Name'];
            var ttc = tableManager($scope, $compile, ngFaqCategory, tableOptions, 'Add Faq Category', 'prepareFaqCategoryTemplate', 'getFaqCategory', 'deleteFaqCategory', 165);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('faqCategoryController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'faqCategoryService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, faqCategoryService, $location)
    {
        $scope.prepareFaqCategoryTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/FaqCategories/ProcessFaqCategory.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getFaqCategory = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            faqCategoryService.getFaqCategory(impId, $scope.getFaqCategoryCompleted);
        };

        $scope.getFaqCategoryCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

             $scope.faqCategory  = response;

             $scope.faqCategory  =
            {
                'Id':  $scope.faqCategory .Id,
                'Name':  $scope.faqCategory .Name
            };

            $scope.edit = true;
            $scope.add = false;

             $scope.faqCategory .Header = 'Update Faq Category';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/FaqCategories/ProcessFaqCategory.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processFaqCategory = function ()
        {
            if ( $scope.faqCategory  == null ||  $scope.faqCategory .Name.length < 1)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req = {
                Id:  $scope.faqCategory .Id,
                Name:  $scope.faqCategory .Name
            };
            
            if ($scope.add)
            {
                faqCategoryService.addFaqCategory(req, $scope.processFaqCategoryCompleted);
            }
            else
            {
               faqCategoryService.editFaqCategory(req, $scope.processFaqCategoryCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Faq Categories|DPR-PPIPS');

        };

        $scope.initializeReq = function ()
        {
            $scope.faqCategory  =
           {
               'Id': '',
               'Name': ''
           };
            $scope.add = true;
            $scope.edit = false;
             $scope.faqCategory .Header = 'Add Faq Category';
            $scope.buttonText = "Add";
        };

        $scope.processFaqCategoryCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/FaqCategories/ProcessFaqCategory.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteFaqCategory = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                faqCategoryService.deleteFaqCategory(id, $scope.deleteFaqCategoryCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteFaqCategoryCompleted = function (data)
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







