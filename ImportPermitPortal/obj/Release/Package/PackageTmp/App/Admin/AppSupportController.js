"use strict";

define(['application-configuration', 'bnkAdminService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app)
{
   
    app.register.controller('appBankerDetailController', ['ngDialog', '$scope', '$rootScope', '$routeParams', 'bnkAdminService', '$location', '$upload', 'fileReader', '$http',
    function (ngDialog, $scope, $rootScope, $routeParams, bnkAdminService, $location, $upload, fileReader, $http)
    {

        $scope.initializeController = function ()
        {
            $rootScope.setPageTitle('Application Support|DPR-PPIPS');
        };
       
        $scope.getAppByReference = function ()
        {
            if ($scope.referenceCode == null || $scope.referenceCode.length < 1)
            {
                alert('Please provide Application Reference Code.');
                return;
            }

            $scope.gettingApp = true;

            bnkAdminService.searchBankApp($scope.referenceCode.trim(), $scope.getAppCompleted);
        };
        
        $scope.getAppCompleted = function (application)
        {
            if (application.Id < 1)
            {
                alert('An error was encountered. The Application Infromation could not be retrieved.');
            }
            
            $scope.application = application;
            $scope.bnkDocs = [];
            $scope.suppliedDocs = [];
            $scope.nextDocs = [];
            $scope.throughPuts = [];
            $scope.unsuppliedthroughPuts = [];

            angular.forEach($scope.application.DocumentTypeObjects, function (n, m)
            {
                if (n.Uploaded === true)
                {
                    n.index = m + 1;
                    $scope.suppliedDocs.push(n);
                }
                else
                {
                    if (n.StageId === 1 || n.IsDepotDoc === true)
                    {
                       $scope.bnkDocs.push(n);
                    }
                    else
                    {
                        $scope.nextDocs.push(n);
                    }
                }
            });

            var s = $scope.suppliedDocs.length;

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i) {
                if (g.ThroughPutObjects !== null && g.ThroughPutObjects.length > 0) {

                    angular.forEach(g.ThroughPutObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0) {
                            y.index = s + 1;
                            s++;
                            y.IsUploaded = true;
                            $scope.throughPuts.push(y);
                        }

                        else {
                            y.IsUploaded = false;
                            $scope.unsuppliedthroughPuts.push(y);
                        }
                    });
                }

            });

            var uo = s + $scope.throughPuts.length;

            $scope.refLetters = [];
            $scope.unsuppliedRefLetters = [];

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i) {
                if (g.ProductBankerObjects !== null && g.ProductBankerObjects.length > 0) {
                    angular.forEach(g.ProductBankerObjects, function (y, p) {
                        if (y.DocumentId !== null && y.DocumentId > 0) {
                            y.index = uo + 1;
                            uo++;
                            y.IsUploaded = true;
                            $scope.refLetters.push(y);
                        }

                        else {
                            y.IsUploaded = false;
                            $scope.unsuppliedRefLetters.push(y);
                        }
                    });
                }

            });

            if ($scope.unsuppliedRefLetters > 0)
            {
                angular.forEach(unsuppliedRefLetters, function (g, i) {
                    g.hasDocs = 0;
                    angular.forEach($scope.application.ApplicationItemObjects, function (k, m)
                    {
                        if (g.ApplicationItemId === k.Id) {
                            k.hasDocs += 1;
                        }

                    });

                });
            }
        };
        
        $scope.numberCollection = function ()
        {
            angular.forEach($scope.suppliedDocs, function (n, m)
            {
                n.index = m + 1;
            });
            var s = $scope.suppliedDocs.length;
            angular.forEach($scope.refLetters, function (y, m)
            {
                y.index = s + 1;
                s++;
            });
        };
        
        $scope.getBnkDoc = function (thId)  
        {
            if (thId == null || thId < 1)
            {
                alert('Invalid selection!');
                return;
            }

            angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
            {
                angular.forEach(g.ProductBankerObjects, function (y, p)
                {
                    if (y.Id === thId && y.DocumentId > 0)
                    {
                        y.IsUploaded = false;
                        $scope.refLetters.splice(p, 1);
                        $scope.unsuppliedRefLetters.push(y);
                    } 
                });
            });
           
            $scope.numberCollection();

            if ($scope.unsuppliedRefLetters > 0) {
                angular.forEach(unsuppliedRefLetters, function (g, i) {
                    g.hasDocs = 0;
                    angular.forEach($scope.application.ApplicationItemObjects, function (k, m) {
                        if (g.ApplicationItemId === k.Id) {
                            k.hasDocs += 1;
                        }

                    });

                });
            }
        };
        
        $scope.ProcessBankDoc = function (e, d)
        {   
            var el = (e.srcElement || e.target);
            if (el.files == null) {
                return;
            }
            var file = el.files[0];
            if (d.Id < 1) {
                alert('There is an error on the page. Please refresh the page and try again.');
                return;
            }

            if (d.Id < 1)
            {
                alert('Error : Invalid operation detected. Please refresh the page and try again.');
                return;
            }

           
            $scope.bnk = d;
            var url = '';
            if (d.DocumentId === null)
            {
                url = "/Document/SaveRefLetter?applicationItemId=" + d.ApplicationItemId + '&applicationId=' + d.ApplicationId + '&bankId=' + d.BankId + '&importerId=' + d.ImporterId;
            }
            else {
                url = "/Document/UpdateRefLetter?documentId=" + d.DocumentId;
            }

           $scope.processing = true;
          

           $upload.upload({
               url: url,
               method: "POST",
               data: { file: file }
           })
          .progress(function (evt)
          {
              d.status = 'verifying';
          }).success(function (data) {
              $scope.processing = false;
              if (data.Code < 1)
              {
                  d.status = 'notVerified';
                  alert(data.Error);
                  return;
              }

              else {

                  d.status = 'verified';
                 d.DocumentPath = data.Path;
                  d.DocumentId = data.Code;

                  angular.forEach($scope.application.ApplicationItemObjects, function (g, i)
                  {
                      angular.forEach(g.ProductBankerObjects, function (y, p)
                      {
                          if (d.Id === y.Id)
                          {
                              d.IsUploaded = true;
                              y.DocumentStatus = 'Pending';
                              $scope.refLetters.push(y);
                              $scope.unsuppliedRefLetters.splice(p, 1);
                          }
                      });  
                  });

                  if ($scope.unsuppliedRefLetters > 0) {
                      angular.forEach(unsuppliedRefLetters, function (g, i) {
                          g.hasDocs = 0;
                          angular.forEach($scope.application.ApplicationItemObjects, function (k, m) {
                              if (g.ApplicationItemId === k.Id) {
                                  k.hasDocs += 1;
                              }

                          });

                      });
                  }

                  $scope.numberCollection();
                  alert(data.Error);
              }

          });
        }
        
    }]);
    
});


