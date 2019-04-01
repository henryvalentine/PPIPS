"use strict";

define(['application-configuration','documentServices', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.directive("ngPermitDocumentSelect", function ()
    {
        return {
            link: function ($scope, el) {
                el.bind("change", function (e) {
                    $scope.file = (e.srcElement || e.target).files[0];
                    $scope.processFile();
                });
            }
        };
    });
    
    app.register.controller('documentController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'documentServices', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, documentServices, $upload, fileReader)
    {
        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Documents|DPR-PPIPS');
            $scope.document =
            {
                'DocumentId': '',
                'DocumentTypeId': '',
                'Id': '',
                'DateUploaded': '',
                'Status': '',
                'UploadedById': '',
                'DocumentPath': '',
                'DateUploadedStr': '',
                'ImageResult': '/Content/images/noImage.png',
                'StatusStr': '',
                'Header' : 'Upload Document',
                'DocumentTypeName': '',
                'DocumentType': { 'DocumentTypeId': '', 'Name': '-- Select Document Type --' },
                'Application': { 'ApplicationId': '', 'ReferenceCode': '-- Select --' },
                'ApplicationId' : ''
            };
        };

        $scope.getDocs = function ()
        {
            documentServices.getDocuments($scope.getDocumentsCompleted);
        };

        $scope.getDocumentsCompleted = function (data)
        {
            $scope.docs = data;
            
            documentServices.getApplications($scope.getApplicationsCompleted);
            
        };

        $scope.getApplicationsCompleted = function (data)
        {
            $scope.appList = data;

        };

        $scope.initializeDocs = function ()
        {
            $scope.docs = [];
            $scope.getDocs();
            $scope.documentTypes = [];
            $scope.getDocumentTypes();
        };

        $scope.PrepareDocTemplate = function ()
        {
            $scope.initializeController();
            $scope.edit = false;
            $scope.add = true;
            $scope.buttonText = "Add";
            ngDialog.open({
                template: '/App/Application/ProcessDocument.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.ProcessDocument = function ()
        {
            if (parseInt($scope.document.DocumentType.DocumentTypeId) < 1)
            {
                alert("ERROR: Please select Document Type.");
                return;
            }
            
            if ($scope.document.Application.ApplicationId < 1) {
                alert("ERROR: Please select an Application reference code.");
                return;
            }
            
            var document =
                {
                    'DocumentId': $scope.document.DocumentId,
                    'DocumentTypeId': $scope.document.DocumentType.DocumentTypeId,
                    'Id': $scope.document.Id,
                    'DateUploaded': $scope.document.DateUploaded,
                    'Status': 1,
                    'UploadedById': $scope.document.UploadedById,
                    'DocumentPath': $scope.document.DocumentPath,
                    'DateUploadedStr': $scope.document.DateUploadedStr,
                    'StatusStr': 'Pending',
                    'DocumentTypeName': $scope.document.DocumentType.Name,
                    'DocumentType': { 'DocumentTypeId': $scope.document.DocumentType.DocumentTypeId, 'Name': $scope.document.DocumentType.Name },
                    'Application': { 'ApplicationId': $scope.document.Application.ApplicationId, 'ReferenceCode': $scope.document.Application.ReferenceCode },
                    'ApplicationId': $scope.document.Application.ApplicationId
                };

            $scope.document = document;

            if ($scope.edit)
            {
                documentServices.editDocument(document, $scope.ProcessDocumentCompleted);
            }

            else
            {
                documentServices.addDocument(document, $scope.ProcessDocumentCompleted);
            }

        };
        
        $scope.ProcessDocumentCompleted = function (data)
        {
            if (data.Code < 1)
            {
                alert(data.Error);
                return;
            }
            
            if ($scope.edit)
            {
                angular.forEach($scope.docs, function (item, i)
                {
                    if (item.DocumentId === $scope.document.DocumentId)
                    {
                        item.DocumentPath = data.Error;
                        item.DateUploadedStr = data.Email;
                    }
                });
            }

            if ($scope.add)
            {
                $scope.document.DocumentPath = data.Error;
                $scope.document.DateUploadedStr = data.Email;
                $scope.document.DocumentId = data.Code;
                $scope.docs.push($scope.document);
            }
            
            ngDialog.close('/App/Application/ProcessDocument.html', '');
            $scope.initializeController();
            alert('Document was successfully uploaded.');
        };
        
        $scope.removeDoc = function (itemId)
        {
            if (itemId < 1)
            {
                alert('Invalid selection');
                return;
            }
            documentServices.deleteDocument(itemId, $scope.removeDocCompleted);
            $scope.id = itemId;
        };
        
        $scope.removeDocCompleted = function ()
        {
            angular.forEach($scope.docs, function (item, index)
            {
                if (item.variantId == $scope.id)
                {
                    if (!confirm("This Document information will be deleted permanently. Continue?"))
                    {
                        return;
                    }

                    $scope.docs.splice(index, 1);
                    $scope.id = '';
                    $scope.initializeController();
                    alert('Document was successfully deleted.');
                }
            });
        };

        $scope.getDocument = function (documentId)
        {
            if (parseInt(documentId) < 1)
            {
                alert('Invalid selection!');
                return;
            }

            documentServices.getDocument(documentId, $scope.getDocumentCompleted);
        }; 

        $scope.getDocumentCompleted = function (data)
        {
            if(data.DocumentId < 1)
            {
                alert('Document Information could not be retrieved.');
                return;
            }

            $scope.initializeController();
           
            $scope.document =
            {
                'DocumentId': data.DocumentId,
                'DocumentTypeId': data.DocumentTypeId,
                'Id': data.Id,
                'DateUploaded': data.DateUploaded,
                'Status': data.Status,
                'UploadedById': data.UploadedById,
                'DocumentPath': data.DocumentPath,
                'DateUploadedStr': data.DateUploadedStr,
                'ImageResult': '',
                'StatusStr': data.StatusStr,
                'DocumentTypeName': data.DocumentTypeName,
                'DocumentType': { 'DocumentTypeId': data.DocumentTypeId, 'Name': data.DocumentTypeName },
                'Application': { 'ApplicationId': data.ApplicationId, 'ReferenceCode': data.AppReference }
            };
            
            if (data.DocumentPath != null)
            {
                $scope.document.ImageResult = data.DocumentPath.replace('~', '');
            }
            else
            {
                $scope.document.ImageResult = '/Content/images/noImage.png';
            }
           
            $scope.edit = true;
            $scope.add = false;
            $scope.uppload = true;
            $scope.document.Header = 'Update Document Information';
            $scope.buttonText = "Update";
            ngDialog.open({
                template: '/App/Application/ProcessDocument.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.clearProcess = function ()
        {
            $scope.uppload = false;
            $scope.document = {};
        };

        $scope.getDocumentTypes = function ()
        {
            documentServices.getDocumentTypes($scope.getDocumentTypesCompleted);
        };
        
        $scope.getDocumentTypesCompleted = function (data)
        {
            $scope.documentTypes = data;
        };
       
        $scope.processFile = function ()
        {
            $scope.progress = 0;
            fileReader.readAsDataUrl($scope.file, $scope)
                          .then(function (result) {
                              $scope.document.ImageResult = result;
                          });

            $upload.upload({
                url: "/Document/CreateFileSession",
                method: "POST",
                data: { file: $scope.file }
            })
           .progress(function (evt) {
               $scope.progress = parseInt(100.0 * evt.loaded / evt.total);
           }).success(function (data) {
               if (data.code < 1) {
                   alert('File could not be processed. Please try again later.');
               }

           });
        };
    }]);
    
});



