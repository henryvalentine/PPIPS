"use strict";

define(['application-configuration', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngMsg', function ($compile)
    {
        return function ($scope, ngMsg) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/Account/GetMyMessageObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['EventTypeName', 'DateSentStr'];
            var ttc = dashBoardTableManager($scope, $compile, ngMsg, tableOptions, 'getMsg');
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };

    });

    app.register.controller('myMessagesController', ['ngDialog', '$scope', '$rootScope', '$routeParams',
    function (ngDialog, $scope, $rootScope)
    {
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('My Messages|DPR-PPIPS');
            
        };


        $scope.getMsg = function (id) {
            if (id < 1) {
                alert('Invalid selection');
            }

            $rootScope.getMsg(id, $scope.getMsgCompleted);

        };


        $scope.getMsgCompleted = function (message) {
            if (message.Id < 1) {
                alert('Message information could not be retrievd. Please try again');
            }

            $scope.mssg = message;
            $scope.html = message.MessageContent;
            ngDialog.open({
                template: '/App/Dashboard/ViewMessageDetail.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.closeMsg = function () {
            ngDialog.close('/App/Dashboard/ViewMessage.html', '');
        };
    }]);

});




