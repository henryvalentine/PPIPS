"use strict";

define(['application-configuration', 'messageTemplateService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
    app.register.directive('ngTemplate', function ($compile)
    {
        return function ($scope, ngTemplate)
        {
            var tableOptions = {};
            tableOptions.sourceUrl = "/MessageTemplate/GetMessageTemplateObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['Subject', 'EventTypeName'];
            var ttc = tableManager($scope, $compile, ngTemplate, tableOptions, 'Add Message Template', 'prepareMessageTemplate', 'getMessageTemplate', 'deleteMessageTemplate', 185);
            ttc.removeAttr('width').attr('width', '100%');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('messageTemplateController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'messageTemplateService', '$location',
    function (ngDialog, $scope, $rootScope, $routeParams, messageTemplateService, $location)
    {
        $scope.prepareMessageTemplate = function ()
        {
            $scope.initializeReq();
            ngDialog.open({
                template: '/App/MessageTemplates/ProcessMessageTemplate.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getMessageTemplate = function (impId)
        {
            if (parseInt(impId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            messageTemplateService.getMessageTemplate(impId, $scope.getMessageTemplateCompleted);
        };

        $scope.getMessageTemplateCompleted = function (response)
        {
            if (response.Id < 1)
            {
                alert('Invalid selection!');
                return;
            }
            
            $scope.initializeReq();

            $scope.messageTemplate = response;

            angular.forEach($scope.messageEvents, function(k,i) {
                if (k.Id === response.EventTypeId) {
                    $scope.messageTemplate.EventType = k;
                }
            });

            $scope.edit = true;
            $scope.add = false;

            $scope.messageTemplate.Header = 'Update MessageTemplate';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/MessageTemplates/ProcessMessageTemplate.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processMessageTemplate = function ()
        {
            if ($scope.messageTemplate == null)
            {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            if ($scope.messageTemplate.EventType.Id < 1)
            {
                alert('Please select Message Event.');
                return;
            }

            if ($scope.messageTemplate.Subject == null || $scope.messageTemplate.Subject.length < 1)
            {
                alert('Please Message Template Subject.');
                return;
            }

            if ($scope.messageTemplate.MessageContent == null || $scope.messageTemplate.MessageContent.length < 1)
            {
                alert('Please Message Template Content.');
                return;
            }

           var req =
           {
               'Id': $scope.messageTemplate.Id,
               'EventTypeId': $scope.messageTemplate.EventType.Id,
               'Subject': $scope.messageTemplate.Subject,
               'MessageContent': $scope.messageTemplate.MessageContent,
               'Footer': $scope.messageTemplate.Footer
           };
            
            if ($scope.add)
            {
                messageTemplateService.addMessageTemplate(req, $scope.processMessageTemplateCompleted);
            }
            else
            {
                messageTemplateService.editMessageTemplate(req, $scope.processMessageTemplateCompleted);
            }
            
        };
       
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Message Templates|DPR-PPIPS');
            $scope.initializeReq();
            messageTemplateService.getMessageEvents($scope.getMessageEventsCompleted);
        };

        $scope.getMessageEventsCompleted = function (data)
        {
            $scope.messageEvents = data;
        };

        $scope.initializeReq = function ()
        {
            $scope.messageTemplate =
                {
                    'Id': '',
                    'EventTypeId': '',
                    'EventType': { 'Id': '', 'Name': 'Message Event' },
                    'Subject': '',
                    'MessageContent': '',
                    'Footer': ''
                };
            $scope.add = true;
            $scope.edit = false;
            $scope.messageTemplate.Header = 'Add Message Template';
            $scope.buttonText = "Add";
        };

        $scope.processMessageTemplateCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
            }
            else {
                alert(data.Error);
                ngDialog.close('/App/MessageTemplates/ProcessMessageTemplate.html', '');
                $scope.jtable.fnClearTable();
                $scope.initializeReq();
            }
        };

        $scope.deleteMessageTemplate = function (id)
        {
            if (parseInt(id) > 0) {
                if (!confirm("This Item will be deleted permanently. Continue?"))
                {
                    return;
                }
                messageTemplateService.deleteMessageTemplate(id, $scope.deleteMessageTemplateCompleted);

            } else
            {
                alert('Invalid selection.');
            }
        };

        $scope.deleteMessageTemplateCompleted = function (data)
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




