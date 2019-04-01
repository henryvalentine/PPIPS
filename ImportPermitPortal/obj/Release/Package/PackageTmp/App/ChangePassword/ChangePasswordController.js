"use strict";

define(['application-configuration'], function (app)
{
    app.register.controller('chngPssController', ['ngDialog', '$scope', '$rootScope', '$routeParams',
    function (ngDialog, $scope, $rootScope)
    {
       
        $scope.changePss = function ()
        {
            if ($scope.pss.NewPassword == null || $scope.pss.NewPassword.length < 1)
            {
                alert('Please provide your new Password.');
                return;
            }

            if ($scope.pss.ConfirmPassword == null || $scope.pss.ConfirmPassword.length < 1) {
                alert('Please confirm your new Password.');
                return;
            }
            
            if ($scope.pss.ConfirmPassword.length < 8) {
                alert('The password should be at least 8 characters in length.');
                return;
            }

            if ($scope.pss.NewPassword.length !== $scope.pss.ConfirmPassword.length)
            {
                alert('The Passwords do not match.');
                return;
            }
            $rootScope.AjaxPost2({ model: $scope.pss }, "/Account/ChangePassword", $scope.changePssCompleted);
        };

        $scope.changePssCompleted = function (data) {
            if (data.Code < 1) {
                alert(data.Error);
                return;
            }
            alert(data.Error);
            $rootScope.logout();
        };
       
        $scope.initializeController = function ()
        {
           $rootScope.setPageTitle('Account Types');

           $scope.pss =
           {
               'NewPassword': '', 'ConfirmPassword': ''
           };

            //changePss
            //pss.NewPassword
           //pss.ConfirmPassword

        };

     
        
    }]);
  
});




