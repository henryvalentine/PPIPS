
"use strict";

define(['angularAMD', 'angular-route', 'ui-bootstrap', 'angular-sanitize', 'ui.select', 'blockUI', 'ngDialog', 'isteven-multi-select', 'ngStorage', 'ui.utils.masks', 'ngIdle'], function (angularAMD)
{
    var app = angular.module("mainModule", ['ngRoute', 'blockUI', 'ngSanitize', 'ui.select', 'ui.bootstrap', 'ngDialog', 'isteven-multi-select', 'ngStorage', 'ui.utils.masks', 'ngIdle']);
   
    app.config(['ngDialogProvider', function (ngDialogProvider)
    {
        ngDialogProvider.setDefaults({
            className: 'ngdialog-theme-default',
            showClose: true,
            closeByDocument: false,
            closeByEscape: false
        });
    }]).config(function (IdleProvider, KeepaliveProvider) //Configure user session manager
    {
        var ticketIdle = 15 * 60;
        IdleProvider.idle(ticketIdle);
        IdleProvider.timeout(15);
        KeepaliveProvider.interval(300);
        KeepaliveProvider.http('/Notification/RefreshSession');
    });

    //app.config(['datePicker', function (datePicker)
    //{
    //    angular.extend(datePicker.defaults,
    //        {
    //        dateFormat: 'dd/MM/yyyy',
    //        startWeek: 1
    //    });
    //}]);
    
    app.filter('propsFilter', function ()
    {
        return function(items, props) {
            var out = [];

            if (angular.isArray(items)) {
                items.forEach(function(item) {
                    var itemMatches = false;

                    var keys = Object.keys(props);
                    for (var i = 0; i < keys.length; i++) {
                        var prop = keys[i];
                        var text = props[prop].toLowerCase();
                        if (item[prop].toString().toLowerCase().indexOf(text) !== -1) {
                            itemMatches = true;
                            break;
                        }
                    }

                    if (itemMatches)
                    {
                        out.push(item);
                    }
                });
            } else {
                // Let the output be the input untouched
                out = items;
            }

            return out;
        };
    });
    
    app.config(function (blockUIConfigProvider)
    {

        // Change the default overlay message
        blockUIConfigProvider.message("Processing...");
        // Change the default delay to 100ms before the blocking is visible
        blockUIConfigProvider.delay(1);
        // Disable automatically blocking of the user interface
        blockUIConfigProvider.autoBlock(false);

    });

    app.config(['$routeProvider', function ($routeProvider)
    {
        $routeProvider
              .when("/production/dashboard", angularAMD.route({
                  templateUrl: function (rp) { return '/production/dashboard-3.html'; },
                  
                  resolve: {
                      load: ['$q', '$rootScope', '$location', function ($q, $rootScope, $location) {

                          var loadController = "/App/production/dashboardController.js";

                          var deferred = $q.defer();
                          require([loadController], function ()
                          {
                              $rootScope.$apply(function () {
                                  deferred.resolve();
                              });
                          });
                          return deferred.promise;
                      }]
                  }
              }))
              .when("/History", angularAMD.route({
                  templateUrl: function (rp) { return 'App/BnkAppUsr/History.html'; },

                  resolve: {
                      load: ['$q', '$rootScope', '$location', function ($q, $rootScope, $location) {

                          var loadController = "/App/BnkAppUsr/BnkAppUsrController.js";

                          var deferred = $q.defer();
                          require([loadController], function () {
                              $rootScope.$apply(function () {
                                  deferred.resolve();
                              });
                          });
                          return deferred.promise;
                      }]
                  }
              }))
            .when("/:section/:tree", angularAMD.route({
                templateUrl: function (rp) { return 'App/' + rp.section + '/' + rp.tree + '.html'; },

                resolve: {
                    load: ['$q', '$rootScope', '$location', function ($q, $rootScope, $location) {

                        var path = $location.path();
                        var parsePath = path.split("/");
                        var parentPath = parsePath[1];
                        var controllerName = parsePath[2];

                        var loadController = "/App/" + parentPath + "/" + controllerName + "Controller.js";

                        var deferred = $q.defer();
                        require([loadController], function () {
                            $rootScope.$apply(function () {
                                deferred.resolve();
                            });
                        });
                        return deferred.promise;
                    }]
                }
            }))
            .when("/:section/:tree/:id", angularAMD.route({
                templateUrl: function (rp) { return 'App/' + rp.section + '/' + rp.tree + '.html'; },

                resolve: {
                    load: ['$q', '$rootScope', '$location', function ($q, $rootScope, $location) {

                        var path = $location.path();
                        var parsePath = path.split("/");
                        var parentPath = parsePath[1];
                        var controllerName = parsePath[2];

                        var loadController = "/App/" + parentPath + "/" + controllerName + "Controller.js";

                        var deferred = $q.defer();
                        require([loadController], function () {
                            $rootScope.$apply(function () {
                                deferred.resolve();
                            });
                        });
                        return deferred.promise;
                    }]
                }
            }))
            .otherwise({ redirectTo: '/Dashboard/Dashboard' });

    }]);

    //Directive Used on input fields that require only numeric values
    app.directive('validNumber', function () {
        return {
            require: '?ngModel',
            link: function (scope, element, attrs, ngModelCtrl) {
                if (!ngModelCtrl) {
                    return;
                }

                ngModelCtrl.$parsers.push(function (val) {
                    if (angular.isUndefined(val)) {
                        val = '';
                    }
                    var clean = val.replace(/[^0-9]+/g, '');
                    if (val !== clean) {
                        ngModelCtrl.$setViewValue(clean);
                        ngModelCtrl.$render();
                    }
                    return clean;
                });

                element.bind('keypress', function (event) {
                    if (event.keyCode === 32) {
                        event.preventDefault();
                    }
                });
            }
        };
    });

    var masterController = function ($scope, $rootScope, $http, $timeout, $localStorage, $sessionStorage, $location, Idle, Keepalive, $modal, $window) {
        
        $rootScope.setPageTitle = function(pageTitle)
        {
            $window.document.title = pageTitle;
        }

        // For session Management
        function closeModals()
        {
            if ($rootScope.warning)
            {
                $rootScope.warning.close();
                $rootScope.warning = null;
            }

            if ($rootScope.timedout) {
                $rootScope.timedout.close();
                $rootScope.timedout = null;
            }
        }

        $rootScope.$on('IdleStart', function ()
        {
            closeModals();

            $rootScope.warning = $modal.open({
                templateUrl: 'warning-dialog.html',
                windowClass: 'modal-danger'
            });
        });

        $rootScope.$on('IdleEnd', function () {
            closeModals();
        });

        $rootScope.$on('IdleTimeout', function () {
            closeModals();
            document.getElementById('logoutForm').submit();
        });

        var ticketTimeOut = 0;
        $scope.getAuthUser = function () {
            $rootScope.isUser = false;
            $rootScope.AjaxGet2("/Account/GetLoggedOnUserInfo", $scope.authenicateUserComplete);
        };
        
        $scope.authenicateUserComplete = function (response)
        {
            if (response.IsAuthenticated === false)
            {
                $scope.redirectLogin();
            }
            else
            {
                $scope.authenticated = true;

                var nms = response.UserName.split(" ");

                $scope.authUserName = nms[0];
                if (nms[1] != null && nms[1].length > 0)
                {
                    $scope.authUserName += " " + nms[1];
                }
                Idle.watch();
                var path = $location.absUrl();
                $scope.getContents();
                if (path.indexOf("ngy.html") > -1 || path.indexOf("ngx.html") > -1 || path.indexOf("ngs.html") > -1)
                {
                    $timeout($rootScope.getList, 700);
                }

            }
        };

        $scope.getContents = function ()
        {
            //Retrieve the Applications site contents
            $scope.AjaxGet(' /ApplicationContent/GetApplicationContents', $scope.getContentsCompleted);
        };

        $scope.getContentsCompleted = function (response)
        {
            //redesign the contes hrefs to reflect the master page in use
            var path = $location.absUrl();
            if (path.indexOf("ngx.html") > -1)
            {
                angular.forEach(response, function (n, i)
                {
                    n.Href = 'ngx.html#' + n.Href + '/' + n.Id;
                });
            }

            if (path.indexOf("ngs.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'ngs.html#' + n.Href + '/' + n.Id;
                });
            }
            if (path.indexOf("ngc.html") > -1) {
                angular.forEach(response, function (n, i) {
                    n.Href = 'ngc.html#' + n.Href + '/' + n.Id;
                });
            }
            if (path.indexOf("ngi.html") > -1) {
                angular.forEach(response, function (n, i) {
                    n.Href = 'ngi.html#' + n.Href + '/' + n.Id;
                });
            }
            if (path.indexOf("ngy.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'ngy.html#' + n.Href + '/' + n.Id;
                });
            }

            if (path.indexOf("bnkAd.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'bnkAd.html#' + n.Href + '/' + n.Id;
                });
            }
            if (path.indexOf("bnkUsr.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'bnkUsr.html#' + n.Href + '/' + n.Id;
                });
            }

            if (path.indexOf("depot_Owner.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'depot_Owner.html#' + n.Href + '/' + n.Id;
                });
            }

            if (path.indexOf("ngVf.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'ngVf.html#' + n.Href + '/' + n.Id;
                });
            }


            if (path.indexOf("nge.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'nge.html#' + n.Href + '/' + n.Id;
                });
            }


            if (path.indexOf("ngappr.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'ngappr.html#' + n.Href + '/' + n.Id;
                });
            }

            if (path.indexOf("nga.html") > -1)
            {
                angular.forEach(response, function (n, i) {
                    n.Href = 'nga.html#' + n.Href + '/' + n.Id;
                });
            }
            $rootScope.contents = response;
        };

        $rootScope.logout = function () {
            document.getElementById('logoutForm').submit();
        };

        $scope.redirectLogin = function ()
        {
           window.location.href = "/Account/Login";
        };

        
        $rootScope.getReqs = function ()
        {
            $rootScope.isAddressProvided = false;
            //$scope.AjaxGet("/GeneralInformation/CheckElligibility", $scope.getReqsCompleted);
            
        };

        $rootScope.getReqsCompleted = function (response)
        {
            $timeout.cancel($rootScope.getReqs);
            $rootScope.IsAddressProvided = response.IsAddressProvided;

            if (response.DocumentTypeObjects.length < 1)
            {
                
                $rootScope.documentTypes = [];
                return;
            }
           
            $rootScope.documentTypes = response.DocumentTypeObjects;
        };

        $rootScope.countdown = function () {

            var stopped = $timeout(function () {
                if (ticketTimeOut <= 1)
                {
                    $rootScope.stop(stopped);
                }
                else
                {
                    ticketTimeOut--;
                    $scope.countdown();
                }

            }, 1000);

        };
        ///
        $rootScope.getContent = function (url, callbackFunction)
        {
            $rootScope.AjaxGet2(url, callbackFunction);
              
        };
        
        $rootScope.stop = function (stopped)
        {
            $timeout.cancel(stopped);
            $scope.authenticated = false;
            $rootScope.redirectLogin();
        };
        
        $scope.AjaxGet = function (route, successFunction)
        {
            setTimeout(function() {
                $http({ method: 'GET', url: route }).success(function (response)
                {
                    successFunction(response);
                });
            }, 1);

        };

        $rootScope.AjaxGet2 = function (route, successFunction)
        {
            setUIBusy();
            setTimeout(function ()
            {
                $http({ method: 'GET', url: route }).success(function (response) {
                    stopUIBusy();
                    successFunction(response);
                });
            }, 1);

        };
        
        $scope.AjaxPost = function (data, route, callbackFunction)
        {
            setUIBusy();
            setTimeout(function ()
            {
                $http.post(route, data).success(function (response)
                {
                    stopUIBusy();
                    callbackFunction(response, status);
                });
            }, 1000);

        };

        $rootScope.AjaxPost2 = function (data, route, callbackFunction)
        {
            setUIBusy();
            setTimeout(function () {
                $http.post(route, data).success(function (response) {
                    stopUIBusy();
                    callbackFunction(response, status);
                });
            }, 1000);

        };

        $rootScope.postData = function (data, route, callbackFunction)
        {
            $scope.AjaxPost(data, route, callbackFunction);
        };

        $rootScope.getList = function ()
        {
            var countries = $localStorage.countries;
            if (countries == null)
            {
                $scope.AjaxGet("/GeneralInformation/GetCountries", $scope.getCountriestCompleted);
            }
            else
            {
                $rootScope.countries = countries;
                var structures = $localStorage.structures;
                
                if (structures == null)
                {
                    $scope.AjaxGet("/GeneralInformation/GetStructures", $scope.getStructuresCompleted);
                }
                else
                {
                    $rootScope.structures = structures;
                }
            }

            $timeout($rootScope.getIssueCategories, 500);
        };
        
        $rootScope.getIssueCategories = function () {
          
            //var issueCategories = $localStorage.issueCategoryList;
            var issueCategories = [];
            if (issueCategories == null || issueCategories.length < 1)
            {
               $scope.AjaxGet("/Issue/GetIssueCategories", $rootScope.getIssueCategoriesCompleted);
            }
            else
            {
                $rootScope.issueCategoryList = issueCategories;
            }
        };

        $rootScope.getIssueCategoriesCompleted = function (data)
        {
            if (data == null || data.length < 1) {
                $rootScope.issueCategoryList = [];
                $localStorage.issueCategoryList = null;
            }
            $timeout.cancel($rootScope.getIssueCategories);
            $rootScope.issueCategoryList = data;
            $localStorage.issueCategoryList = data;
        };
       
        $rootScope.getCountriestCompleted = function (data)
        {
            $timeout.cancel($scope.getList);
            $rootScope.countries = data;
            $localStorage.countries = data;
            $rootScope.getStructures();
        };

        $rootScope.getStructures = function ()
        {
            var structures = $localStorage.structures;
            if (structures == null)
            {
                $scope.AjaxGet("/GeneralInformation/GetStructures", $scope.getStructuresCompleted);
            }
            else
            {
                $rootScope.structures = structures;
            }
        };

        $rootScope.getStructuresCompleted = function (data)
        {
            $rootScope.structures = data;
            $localStorage.structures = data;
        };

        $rootScope.getAppCount = function (url, callbackFunction)
        {
            $rootScope.AjaxGet2(url, callbackFunction);
        };

        $rootScope.getItem = function (url, callbackFunction)
        {
            $scope.AjaxGet(url, callbackFunction);
        };

        $rootScope.getQuery = function (url, callbackFunction) {
            $rootScope.AjaxGet2(url, callbackFunction);
        };


        $rootScope.getMessages = function ()
        {
            $rootScope.AjaxGet2("/Account/GetMyMessageObjects", $rootScope.getMessagesCompleted);
        };

        $rootScope.getLatestMessages = function ()
        {
            $rootScope.AjaxGet2("/Account/GetMyLatestMessages", $rootScope.getMessagesCompleted);
        };
        
        $rootScope.getMessagesCompleted = function (messages)
        {
            $scope.msgs = messages;
        };
        
        $rootScope.getMsg = function (id, callbackFunction)
        {
            $rootScope.AjaxGet2("/Account/GetMyMessageObject?id=" + id, callbackFunction);
        };
        
        $rootScope.printForm = function (formId)
        {

            var printContents = document.getElementById(formId).innerHTML;
            var popupWin = '';
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                popupWin = window.open('', '_blank', 'width=800,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="style.css" />' +
                    '</head><body onload="window.print()"><div class="reward-body">' + printContents + '</div></html>');
                popupWin.onbeforeunload = function (event) {
                    popupWin.close();
                    return '.\n';
                };
                popupWin.onabort = function (event)
                {
                    popupWin.document.close();
                    popupWin.close();
                }
            } else {
                popupWin = window.open('', '_blank', 'width=800,height=700,scrollbars=yes,menubar=no,toolbar=no,location=no,status=yes,titlebar=yes');
                popupWin.document.open();
                popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="/Content/bootstrap.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin.document.close();
            }
            popupWin.document.close();
            return true;
        };
        
        function setUIBusy() {
            $rootScope.busy = true;
        };

        function stopUIBusy() {
            $rootScope.busy = false;
        };
    };
   
    masterController.$inject = ['$scope', '$rootScope', '$http', '$timeout', '$localStorage', '$sessionStorage', '$location', 'Idle', 'Keepalive', '$modal', '$window'];
    app.controller("masterController", masterController);
    
     // Bootstrap Angular when DOM is ready
    angularAMD.bootstrap(app);
    //.run(function(Idle){
    //    // start watching when the app runs. also starts the Keepalive service by default.
    //    Idle.watch();
    //});
    return app;
});

