"use strict";

define(['application-configuration', 'userAppService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.directive("ngPermitDocumentSelect", function ()
    {
        return {
            link: function ($scope, el) {
                el.bind("change", function (e)
                {
                    $scope.file = (e.srcElement || e.target).files[0];
                    $scope.processFile();
                });
            }
        };
    });
    
    app.register.controller('userdocumentController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'userAppService', '$upload', '$location', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, userAppService, $upload,$location, fileReader)
    {
        $scope.initializeController = function ()
        {
            $scope.fetchingInfo = true;
            $scope.ldr = '<img src="/Images/load.GIF" style="float: left" id="loder" title="Processing File"/>';
            $scope.scs = '<img src="/Images/success.png" style="float: left" id="saveSuccess" title="File was successfully processed"/>';
            $scope.sEr = '<img src="/Images/error.png" style="float: left" id="saveError" title="File processing failed. Please try again."/>';


            var appId = userAppService.getAppId();
            if (appId < 1)
            {
                alert('An error was encountered on the page. Please try again.');
                $location.path('/Application/MyApplications');
            }
           
            userAppService.getApplicationDocuments(appId, $scope.getAppDocsCompleted);
        };

        $scope.getAppDocsCompleted = function (data)
        {
            $scope.fetchingInfo = false;
            $scope.suppliedDocs = []; 
            $scope.StageDocs = []; 
            $scope.nextDocs = [];
            $scope.application = data;
            angular.forEach(data.DocumentTypeObjects, function (n, m)
            {
                if (n.Uploaded === true)
                {
                    $scope.suppliedDocs.push(n);
                }
                else
                {
                    if (n.StageId === 1)
                    {
                        $scope.StageDocs.push(n);
                    }
                    else
                    {
                       $scope.nextDocs.push(n);
                    }
                }
            });

        };

        $scope.ProcessDocument = function (e)
        {

            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];
            var doctTypeId = el.id.split('j')[0];
            if (doctTypeId < 1) {
                alert('There is an error on the page. Please refresh the page and try again.');
                return;
            }

            var divf = 'infoDiv' + doctTypeId;
            var infoDiv = angular.element('#' + divf);

            $upload.upload({
                url: "/Document/SaveStageFile?docTypeId=" + doctTypeId + '&&importAppId=' + $scope.application.ApplicationId,
                method: "POST",
                data: { file: file }
            })
           .progress(function (evt) {
               infoDiv.html($scope.ldr);
           }).success(function (data) {
               if (data.code < 1) {
                   infoDiv.html($scope.sEr);
               }

               else {
                   infoDiv.html($scope.scs);
                   
                   angular.forEach($scope.StageDocs, function (n, m) {
                       if (n.DocumentTypeId === data.Code) {
                           $scope.suppliedDocs.push(n);
                           $scope.StageDocs.splice(m, 1);
                       }
                   });
               }

           });


        };

        
    }]);
    
});



