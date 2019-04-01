"use strict";

define(['application-configuration', 'faqService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngFaq', function ($compile)
    {
        return function ($scope, ngFaq)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Faq/GetFaqObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['FaqCategoryName', 'Question', 'Answer'];
            var ttc = tableManager($scope, $compile, ngFaq, tableOptions, 'Add Faq', 'prepareFaqTemplate', 'getFaq', 'deleteFaq', 95);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        }; 
    });
  
    app.register.controller('faqController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'faqService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, faqService, $location)
    {
        $scope.initializeReq = function () {
            $scope.faq =
                        {
                            'FaqCategoryObject': { 'Id': '', 'Name': '-- Select FAQ Category --' },
                            'Id': '',
                            'CategoryId': '',
                            'LastUpdated': '',
                            'Answer': '',
                            'Question': ''
                        };

            $scope.add = true;
            $scope.edit = false;
            $scope.faq.Header = 'Add FAQ';
            $scope.buttonText = "Add";
        };

        $scope.prepareFaqTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/Faqs/ProcessFaq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getFaq = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            faqService.getFaq(impId, $scope.getFaqCompleted);
        };
        
        $scope.getFaqCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.faq = response;

            angular.forEach($scope.categories, function (o, i)
            {
                if (o.Id === response.Id)
                {
                    $scope.faq.FaqCategoryObject = o;
                }
            });
            
            $scope.edit = true;
            $scope.add = false;

            $scope.faq.Header = 'Update FAQ';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Faqs/ProcessFaq.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.processFaq = function ()
        {
            if ($scope.faq == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

           var faq =
           {
               'Id': $scope.faq.Id,
               'CategoryId': $scope.faq.FaqCategoryObject.Id,
               'LastUpdated': $scope.faq.LastUpdated,
               'Answer': $scope.faq.Answer,
               'Question': $scope.faq.Question
           }; 
           
            if ($scope.add)
            {
                faqService.addFaq(faq, $scope.processFaqCompleted);
            }
            else
            {
                faqService.editFaq(faq, $scope.processFaqCompleted);
            }
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('FAQs|DPR-PPIPS');
            faqService.getFaqCategories($scope.getFaqCategoriesCompleted);
        };

        $scope.getFaqCategoriesCompleted = function (data)
        {
            $scope.categories = data;
        };

        $scope.processFaqCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/Faqs/ProcessFaq.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteFaq = function (id)
        {
            if (parseInt(id) > 0)
            {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                faqService.deleteFaq(id, $scope.deleteFaqCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteFaqCompleted = function (data)
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




