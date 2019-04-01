"use strict";

define(['application-configuration', 'recertificationService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngRecertification', function ($compile) {
        return function ($scope, ngRecertification) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Recertification/GetUserRecertificationObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'DateApplied', 'StatusStr'];
            var ttc = RecertiyManager($scope, $compile, ngRecertification, tableOptions, 'Recertify', 'prepareRecertificationTemplate', 'getRecertification', 110);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    
    app.register.controller('recertificationController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'recertificationService', '$http', '$route', '$window', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, recertificationService, $http, $route, $window, $location) {

        $scope.initializeApp = function () {
            $scope.viewApp = false;
            
            $scope.viewD = false;
           
            $rootScope.setPageTitle('Product Certification');

            $scope.application =
            {

                'Id': '', 'ReferenceCode': '', 'DateApplied': '',
                'StatusStr': ''


            };



        };
        $scope.prepareRecertificationTemplate = function () {
            $scope.recertification =
           {

               'ReferenceCode': { 'Id': '', 'ReferenceCode': '-- Select Reference Code --' }
           };


            $http({ method: 'GET', url: '/Recertification/GetGenericList' }).success(function (response)
            {
                if (response == null) {
                    alert('Issue Types could not be retrieved.');
                    return;
                }
                $scope.Recerts = response.Recerts;

            });

            ngDialog.open({
                template: '/App/Recertify/ProcessRecertify.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.processRecertification = function () {

            if ($scope.recertification == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var req =
           {

               'Id': $scope.recertification.ReferenceCode.Id

             
           };

            recertificationService.addRecertification(req, $scope.processRecertificationCompleted);


        };

        $scope.processRecertificationCompleted = function (response) {
            if (response.Code < 1 || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }
           
            alert('Your recertification has been accepted, Click to continue');
            $scope.initializeApp();
            $route.reload();
            $window.location.reload();

           ;
            
            

        };

        $scope.getRecertification = function (impId)
        {
            if (parseInt(impId) < 1) {
                alert('Invalid selection!');
                return;
            }
            recertificationService.getRecertification(impId, $scope.getRecertificationCompleted);
        };

        $scope.getRecertificationCompleted = function (response) {
               if (response == null || response.Id < 1) {
                alert('Notification Information could not be retrieved.');
                return;
            }
            $scope.docs = response;
            $scope.viewApp = true;
            $scope.viewD = true;

           
        };

  
        $scope.initializeController = function () {
            $rootScope.setPageTitle('Recertifications');
            recertificationService.getGenericList($scope.getGenericListCompleted);

        };

        $scope.getGenericListCompleted = function (data) {

            $scope.Recerts = data.Recerts;
           

        };

        $scope.initialize = function () {
            $scope.recertification =
            {
               
                'ReferenceCode': { 'Id': '', 'Name': '-- Select Reference Code --' },
               
                
            };
           
        };

 

  



        $scope.deleteRecertification = function (id) {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?")) {
                    return;
                }
                recertificationService.deleteRecertification(id, $scope.deleteRecertificationCompleted);

            }

            else {
                alert('Invalid selection.');
            }
        };


        $scope.deleteRecertificationCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);

            }
            else {
                $scope.jtable.fnClearTable();
                alert(data.Error);
            }
        };


     



    }]);

});




