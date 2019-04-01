"use strict";

define(['application-configuration', 'faqService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.controller('faqRController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'faqService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, faqService, $location)
    {
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('FAQs|DPR-PPIPS');
            faqService.getFaqs($scope.getFaqsCompleted);
        };

        $scope.getFaqsCompleted = function (data)
        {
            $scope.faqs = data;
        };
    }]);

});




