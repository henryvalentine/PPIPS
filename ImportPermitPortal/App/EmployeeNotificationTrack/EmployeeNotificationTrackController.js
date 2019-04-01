
"use strict";

define(['application-configuration', 'notificationTrackService', 'ngDialog', 'angularFileUpload', 'fileReader'], function (app) {
    app.register.directive('ngNotTrack', function ($compile) {
        return function ($scope, ngNotTrack) {
            var tableOptions = {};
            tableOptions.sourceUrl = "/EmployeeProfile/GetEmployeeNotificationObjects";
            tableOptions.itemId = 'Id';
            tableOptions.columnHeaders = ['ReferenceCode', 'CompanyName', 'AssignedTimeStr', 'DueTimeStr'];

            var ttc = adminApplicationtableManager($scope, $compile, ngNotTrack, tableOptions);
            ttc.removeAttr('width').attr('width', 'auto');
            $scope.jtable = ttc;
        };
    });

    app.register.controller('notificationTrackController', ['ngDialog', '$scope', '$rootScope', '$routeParams', '$upload', 'fileReader', '$http', 'notificationTrackService', '$route', '$window', '$location', '$timeout',
    function (ngDialog, $scope, $rootScope, $routeParams, $upload, fileReader, $http, notificationTrackService, $route, $window, $location, $timeout) {

        $scope.today = function () {
            $scope.dt = new Date();
        };
        //$scope.today();

        $scope.clear = function () {
            $scope.dt = null;
        };

        // Disable weekend selection
        $scope.disabled = function (date, mode) {
            return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
        };

        $scope.toggleMin = function () {
            $scope.minDate = $scope.minDate ? null : new Date();
        };
        //$scope.toggleMin();

        $scope.openInspectionDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened1 = true;
        };

        $scope.openDischargeCommencementDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened2 = true;
        };

        $scope.openDischargeCompletionDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened3 = true;
        };

        $scope.openVesselArrivalDate = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.opened4 = true;
        };

      


        $scope.dateOptions = {
            formatYear: 'yy',
            startingDay: 1
        };

        $scope.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];


        $scope.initializeApp = function () {
            $scope.viewApp = false;
            $scope.viewChecklist = false;
            $scope.viewDR = false;
            $scope.viewD = false;
            $scope.viewDI = false;
            $scope.viewDischargeData = false;
            $scope.viewRecertification = false;
            $scope.viewH = false;
            $scope.viewReport = false;
            $scope.viewJettyReport = false;
            $scope.viewDepot = false;
            $scope.viewTeamLeader = false;
            $scope.viewTeamLeaderApprove = false;
            $scope.viewAddParameters = false;
            $scope.viewFirstTank = false;
            $scope.viewFinal = false;
            $scope.viewValid = false;
            $scope.viewInValid = false;
            $scope.viewDoc = false;
            $scope.true = false;
            $scope.false = false;
            $scope.viewSummary = false;
            $scope.viewComFile = true;
            $scope.viewTempFile = true;
            $scope.viewEditParameters = false;
            $scope.viewDry = true;
            $scope.viewROM = true;

        }

            $scope.application =
            {

                'NotificationId': '', 'CompanyId': '', 'ReferenceCode': '', 'DerivedTotalQUantity': '',
                'DerivedValue': '', 'StatusCode': '', 'DateApplied': '', 'LastModified': '', 'CompanyName': '',
                'Header': 'New Application', 'ImportItemObjects': []


            };



          


        $scope.BackToDetail = function () {
            $scope.viewD = true;
            $scope.viewChecklist = false;


        };

        $scope.BackDetailCheckList = function () {
            $scope.viewD = true;
            $scope.viewChecklist = false;

        };

        $scope.BackDetailRecertification = function (id) {
           
            $scope.viewRecertification = false;
            $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationBack?id=' + id }).success(function (back) {
                if (back == null || back.Id < 1) {
                    alert('Information could not be retrieved.');
                    return;
                }
                $scope.application = back;

                $scope.viewDoc = false;
                $scope.viewApp = true;
                $scope.viewD = true;
                $scope.viewI = true;
            });

        };

        $scope.BackDetailFirstTank = function (id) {

            $scope.viewFirstTank = false;
            $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationBack?id=' + id }).success(function (back) {
                if (back == null || back.Id < 1) {
                    alert('Information could not be retrieved.');
                    return;
                }
                $scope.application = back;

                $scope.viewSummary = false;
                $scope.viewDoc = false;
                $scope.viewApp = true;
                $scope.viewD = true;
                $scope.viewI = true;
            });

        };

        $scope.BackDetailDocument = function ()
        {
            $scope.application = $scope.local_application;
            $scope.viewDoc = false;
            $scope.viewD = true;
        };

        $scope.BackDetailDischargeData = function ()
        {
            $scope.viewD = true;
            $scope.viewDischargeData = false;

        };


        $scope.BackDetailHistory = function () {
            $scope.viewD = true;
            $scope.viewH = false;

        };

        $scope.BackDetailReport = function (id) {
            //$scope.viewD = true;
            $scope.viewReport = false;


            $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationBack?id=' + id }).success(function (back) {
                if (back == null || back.Id < 1) {
                    alert('Information could not be retrieved.');
                    return;
                }
                $scope.application = back;

                $scope.viewDoc = false;
                $scope.viewApp = true;
                $scope.viewD = true;
                $scope.viewI = true;
            });

        };



        $scope.getAppView = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            //$scope.initializeApp();
            $scope.selNotificationId = appId;
            notificationTrackService.getNotification(appId, $scope.processGetNotificationCompleted);



        };


        //process get notification completed
        $scope.processGetNotificationCompleted = function (response)
        {
            if (response == null || response.Id < 1) {
                alert('Notification Information could not be retrieved.');
                return;
            }
            $scope.application = response;
            $scope.local_application = response;
            $scope.viewApp = true;
            $scope.viewD = true;

            if (response.Activity === 'Review')
            {
                $scope.viewR = true;

            }


            else if (response.Activity === 'Approval') {

                $scope.viewA = true;


            }


            else if (response.Activity === 'Inspection') {

               
                $scope.viewI = true;


            }


            else if (response.Activity === 'JettyInspection') {


                $scope.viewJetty = true;


            }
            else if (response.Activity === 'DepotInspection') {


                $scope.viewDepot = true;


            }

            else if (response.Activity === 'TeamLead' && !response.IsVesselReportSaved) {


                $scope.viewTeamLeader = true;


            }
            else if (response.Activity === 'TeamLead' && response.IsVesselReportSaved) {


                $scope.viewTeamLeaderApprove = true;


            }
        };


        //process cancel
        $scope.cancel = function () {

            ngDialog.close('/App/EmployeeNotificationTrack/ProcessReview.html', '');
        };



        //process review
        $scope.processReview = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $scope.employee =
          {


              'Reason': ''

          };

            $scope.employee.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeNotificationTrack/ProcessReview.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processDialogReview = function () {



            if ($scope.employee == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var req =
           {

               'ApplicationId': $scope.employee.Id,

               'Description': $scope.employee.Description


           };

            notificationTrackService.processDialogReview(req, $scope.processDialogReviewCompleted);


        };



        $scope.processDialogReviewCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }


           

            else if (response.IsAccepted === true) {
                alert('Your application has been accepted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
               
            }
            else if (response.IsCertificateGenerated === true) {
                alert('Your application has been accepted and the certifcate has been generated, Click to continue');
               
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.IsFinal === true) {
                alert('This is the final step and certificate has been generated, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.CantGeneratePermit === true) {
                alert('This is the final step but certificate is not generated, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.NoEmployee === true) {
                alert('There is no Employee to assign the task, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.IsValid === false) {

                alert('Not all the notification documents are validated.');

                return;

            }

        };


        //process notification approve
        $scope.processNotificationApprove = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $scope.employee =
          {


              'Reason': ''

          };

            $scope.employee.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeNotificationTrack/ProcessApprove.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };

        $scope.processNotificationDialogApprove = function () {



            if ($scope.employee == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var req =
           {

               'ApplicationId': $scope.employee.Id,

               'Description': $scope.employee.Description


           };

            notificationTrackService.processNotificationDialogApprove(req, $scope.processNotificationDialogApproveCompleted);


        };

        $scope.processNotificationDialogApproveCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }

            else if (response.IsValid === false) {

                alert('Not all the notification documents are validated.');

                return;

            }
            else if (response.IsAccepted === true) {
                alert('Your application has been accepted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();

            }
            else if (response.IsCertificateGenerated === true) {
                alert('Your application has been accepted and the certifcate has been generated, Click to continue');

                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.IsFinal === true) {
                alert('This is the final step and certificate has been generated, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.CantGeneratePermit === true) {
                alert('This is the final step but certificate is not generated, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.NoEmployee === true) {
                alert('There is no Employee to assign the task, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }

        };


        $scope.rejectApp = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.employee =
            {

                'Name': { 'Id': '', 'Name': 'Select Issue Type' },
                'Reason': ''

            };

            $http({ method: 'GET', url: '/EmployeeProfile/GetGenericList' }).success(function (response) {
                if (response == null) {
                    alert('Issue Types could not be retrieved.');
                    return;
                }
                $scope.issuetypes = response.IssueTypes;

            });

            $scope.employee.Id = appId;

            ngDialog.open({
                template: '/App/EmployeeNotificationTrack/ProcessRejection.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });

        };


        $scope.processAppRejection = function () {



            if ($scope.employee == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }



            var issue = '';
            if ($scope.employee.IssueType !== null && $scope.employee.IssueType.Id !== undefined) {
                issue = $scope.employee.IssueType.Id;
            }

            var req =
           {

               'ApplicationId': $scope.employee.Id,
               'IssueTypeId': issue,
               'Description': $scope.employee.Description


           };

            notificationTrackService.addIssue(req, $scope.processRejectionCompleted);


        };


        $scope.processRejectionCompleted = function (response)
        {
            if (response == null || response.IsError === true)
            {
                alert('There is a problem with the network.');
                return;
            }
            else if (response.IsAccepted === true) {
                alert('Your comment has been noted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }

        };


       
        $scope.initializeReq = function () {
            $scope.employee =
            {

                'Issuetype': { 'Id': '', 'Name': 'Select Issue Type' },
                'Reason': ''

            };

        };


        //checklist

        $scope.GetDoc = function (docId) 
        {
            if (parseInt(docId) < 1)
            {
                alert('Invalid selection!');
                return;
            }
            notificationTrackService.getDoc(docId, $scope.GetDocCompleted);
        };

        $scope.GetDocCompleted = function (response)
        {
            if (response == null || response.Id < 1)
            {
                alert('Information could not be retrieved.');
                return;
            }

            if (response.IsError === true)
            {
                $scope.appError = 'There is a problem with the network, try again later';
                $scope.negativefeedback = true;
                return;
            }

            if (response.IsAccepted === true)
            {
                $scope.document = response;
                $scope.viewD = false;
                $scope.viewDoc = true;
            }

        };

        $scope.DocumentValid = function (docId)
        {
            if (parseInt(docId) < 1) {
                alert('Invalid selection!');
                return;
            }
           
            notificationTrackService.validDocument(docId, $scope.validateDocumentComplete);
        };

        $scope.validateDocumentComplete = function (response)
        {
            if (response == null)
            {
                    alert('Information could not be retrieved.');
                    return;
                }

                if (response.IsError === true) {


                    $scope.appError = 'There is a problem with the network, try again later';
                    $scope.negativefeedback = true;
                    return;
                }

               
                    $http({ method: 'GET', url: '/EmployeeProfile/GetNotification?id=' + response.TrackId }).success(function (back) {
                        if (back == null || back.Id < 1) {
                            alert('Information could not be retrieved.');
                            return;
                        }
                        $scope.application = back;

                        $scope.viewDoc = false;
                        $scope.viewApp = true;
                        $scope.viewD = true;
                        //$scope.viewI = true;



                        if (back.Activity === 'Review') {


                            angular.forEach($scope.application.NotificationDocumentObjects, function (o, i) {
                                if (o.IsValid === "True") {
                                    $scope.viewValid = true;
                                }
                                else if (o.IsValid == null) {
                                    $scope.viewValid = false;
                                    $scope.viewNotValid = false;
                                }
                            });

                            $scope.viewR = true;

                        }


                        else if (back.Activity === 'Approval') {


                            angular.forEach($scope.application.NotificationDocumentObjects, function (o, i) {
                                if (o.IsValid === "True") {
                                    $scope.viewValid = true;
                                }
                                else if (o.IsValid == null) {
                                    $scope.viewValid = false;
                                    $scope.viewNotValid = false;
                                }
                            });


                            $scope.viewA = true;


                        }


                        else if (back.Activity === 'Inspection') {

                            angular.forEach($scope.application.NotificationDocumentObjects, function (o, i) {
                                if (o.IsValid === "True") {
                                    $scope.viewValid = true;
                                }
                                else if (o.IsValid == null) {
                                    $scope.viewValid = false;
                                    $scope.viewNotValid = false;
                                }
                            });

                            $scope.viewI = true;


                        }
                    });


        };

        $scope.DocumentInValid = function (docId) {
            if (parseInt(docId) < 1) {
                alert('Invalid selection!');
                return;
            }
           


            $http({ method: 'GET', url: '/EmployeeProfile/InValidDocument?id=' + docId }).success(function (response) {
                if (response == null) {
                    alert('Information could not be retrieved.');
                    return;
                }

                if (response.IsError === true) {


                    $scope.appError = 'There is a problem with the network, try again later';
                    $scope.negativefeedback = true;
                    return;
                }


                $http({ method: 'GET', url: '/EmployeeProfile/GetNotification?id=' + response.TrackId }).success(function (back) {
                    if (back == null || back.Id < 1) {
                        alert('Information could not be retrieved.');
                        return;
                    }
                    $scope.application = back;

                    $scope.viewDoc = false;
                    $scope.viewApp = true;
                    $scope.viewD = true;


                    if (back.Activity === 'Review') {


                        angular.forEach($scope.application.NotificationDocumentObjects, function (o, i) {
                            if (o.IsValid === "True") {
                                $scope.viewValid = true;
                            }
                            else if (o.IsValid == null) {
                                $scope.viewValid = false;
                                $scope.viewNotValid = false;
                            }
                        });

                        $scope.viewR = true;

                    }


                    else if (back.Activity === 'Approval') {


                        angular.forEach($scope.application.NotificationDocumentObjects, function (o, i) {
                            if (o.IsValid === "True") {
                                $scope.viewValid = true;
                            }
                            else if (o.IsValid == null) {
                                $scope.viewValid = false;
                                $scope.viewNotValid = false;
                            }
                        });


                        $scope.viewA = true;


                    }


                    else if (back.Activity === 'Inspection') {

                        angular.forEach($scope.application.NotificationDocumentObjects, function (o, i) {
                            if (o.IsValid === "True") {
                                $scope.viewValid = true;
                            }
                            else if (o.IsValid == null) {
                                $scope.viewValid = false;
                                $scope.viewNotValid = false;
                            }
                        });

                        $scope.viewI = true;


                    }
                   
                });


            });



        };


        $scope.checklist = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }
            $scope.viewD = false;



            $http({ method: 'GET', url: '/EmployeeProfile/MarkCheckList?id=' + appId }).success(function (response) {
                if (response == null || response.Id < 1) {
                    alert('Notification Information could not be retrieved.');
                    return;
                }
                $scope.list = response;

                $scope.viewChecklist = true;

            });



        };


        //print checklist
        $scope.printCheckList = function (notId) {
            if (parseInt(notId) < 1) {
                alert('Invalid selection!');
                return;
            }




            notificationTrackService.printCheckList(notId, $scope.printCheckListCompleted);




        };

        $scope.printCheckListCompleted = function (response) {


            window.open(response.SmallPath);



            //$timeout(function () {
            //    $http({ method: 'POST', url: '/EmployeeProfile/DeleteFile?way=' + response.BigPath }).success(function (response2) {

            //        return;

            //    });
            //}, 5000);

        };

        $scope.yes = function (chId) {
            if (parseInt(chId) < 1) {
                alert('Invalid selection!');
                return;
            }



            var req =
           {
               'Id': chId,
               'NotificationId': $scope.application.Id,
               'MySelection': 'Yes'

           };
            notificationTrackService.addReportCheckList(req, $scope.yesCompleted);
        };

        $scope.yesCompleted = function (response) {


            if (response.IsAccepted === true) {


                //$scope.sucess = 'Good';
                //$scope.positivefeedback = true;




            }


        };



        $scope.no = function (chId) {
            if (parseInt(chId) < 1) {
                alert('Invalid selection!');
                return;
            }



            var req =
           {
               'Id': chId,
               'NotificationId': $scope.application.Id,
               'MySelection': 'No'

           };
            notificationTrackService.addReportCheckList(req, $scope.noCompleted);
        };

        $scope.noCompleted = function (response) {


            if (response.IsAccepted === true) {





            }


        };


        //save checklist
        $scope.saveCheckList = function () {

            $scope.success = 'Check List has been saved for future reference';
            $scope.positivefeedback = true;



        };


        //Product Quantity and Quality Analysis
        //vessel report
        $scope.vesselReport = function (appId) {

            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $http({ method: 'GET', url: '/EmployeeProfile/IsCheckListComplete?id=' + appId }).success(function (response) {
                if (response == null) {
                    alert('Information could not be retrieved.');
                    return;
                }

                if (response.IsAccepted === true) {


                    $scope.viewD = false;




                    $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationFromDetail?id=' + appId }).success(function (response) {
                        if (response == null || response.NotificationId < 1) {
                            alert('Notification Information could not be retrieved.');
                            return;
                        }
                        $scope.application = response;


                        //vessel report saved
                        if ($scope.application.IsVesselReportSaved === true) {



                            notificationTrackService.retrieveSavedVesselReport(appId, $scope.retrieveSavedVesselReportCompleted);



                            //$http({ method: 'GET', url: '/EmployeeProfile/GetDepotList' }).success(function (response) {
                            //    if (response == null) {
                            //        alert('Information could not be retrieved.');
                            //        return;
                            //    }
                            //    $scope.vessel.ProductName = response.ProductName;
                            //    $scope.vessel.DepotName = response.DepotName;


                            //});

                            $scope.viewReport = true;
                        }



                            //vessel report not submitted
                        else if ($scope.application.IsVesselReportSubmitted !== true) {


                            $scope.viewReport = true;

                            //$http({ method: 'GET', url: '/EmployeeProfile/GetDepotList' }).success(function (response) {
                            //    if (response == null) {
                            //        alert('Information could not be retrieved.');
                            //        return;
                            //    }
                            //    $scope.Depots = response.Depots;
                            //    $scope.Products = response.Products;


                            //});




                            $scope.vessel =
                             {

                                 'NotificationId': '',
                                 'QuantityOnVessel': '',
                                 'QuantityDischarged': '',
                                 'ProductName': $scope.application.ProductName,
                                 'DepotName': $scope.application.DepotName,
                                 'InspectionDate': '',
                                 'InspectorComment': '',
                                 'DischargeCommencementDate': '',
                                 'DischargeCompletionDate': '',
                                 'QuantityOnBillOfLading': '',
                                 'QuantityAfterSTS': '',
                                 'VesselArrivalDate': '',
                                 'LoadPortCoQAvailable': '',
                                 'RecommendationId': ''

                             };
                        }

                    });


               
                }
                else if (response.IsValid === false) {

                    alert('Not all the notification documents are validated.');

                    return;

                }


            });


         
           

        };


        //store vessel report
        $scope.saveVesselReport = function (notId) {
            if (parseInt(notId) < 1) {
                alert('Invalid selection!');
                return;
            }

            //if ($scope.vessel.Product == null || $scope.vessel.Product === undefined || parseInt($scope.vessel.Product) === NaN || parseInt($scope.vessel.Product) < 1) {
            //    alert("ERROR: Please select Product.");
            //    return;
            //}

            //if ($scope.vessel.Depot == null || $scope.vessel.Depot === undefined || parseInt($scope.vessel.Depot) === NaN || parseInt($scope.vessel.Depot) < 1) {
            //    alert("ERROR: Please select Disharge Depot.");
            //    return;
            //}



            var req =
                     {


                         'NotificationId': notId,
                         'QuantityOnVessel': $scope.vessel.QuantityOnVessel,
                         'QuantityDischarged': $scope.vessel.QuantityDischarged,
                        
                         'InspectionDate': $scope.vessel.InspectionDate,

                         'InspectorComment': $scope.vessel.InspectorComment,
                         'DischargeCommencementDate': $scope.vessel.DischargeCommencementDate,
                         'DischargeCompletionDate': $scope.vessel.DischargeCompletionDate,
                         'QuantityOnBillOfLading': $scope.vessel.QuantityOnBillOfLading,
                         'QuantityAfterSTS': $scope.vessel.QuantityAfterSTS,

                         'VesselArrivalDate': $scope.vessel.VesselArrivalDate,
                         'LoadPortCoQAvailable': $scope.vessel.LoadPortCoQAvailable,
                         'RecommendationId': $scope.vessel.RecommendationId

                     };

            notificationTrackService.saveVesselReport(req, $scope.saveVesselReportCompleted);
        };


        $scope.saveVesselReportCompleted = function (response) {


            if (response.IsAccepted === true) {


                $scope.success = 'Your Information have been stored for future reference';
                $scope.positivefeedback = true;




            }
            if (response.IsError === true) {


                alert('There is a problem with the network, try again later');
                return;




            }


        };



        // retrieve stored recertification  
        $scope.retrieveSavedVesselReportCompleted = function (response) {


            if (response !== null) {

                $scope.vessel = response;






                $scope.vessel =
                    {


                        'NotificationId': $scope.vessel.notId,
                        'QuantityOnVessel': $scope.vessel.QuantityOnVessel,
                        'QuantityDischarged': $scope.vessel.QuantityDischarged,
                        'ProductName': $scope.vessel.ProductName,
                        'DepotName': $scope.vessel.DepotName,
                        'InspectionDate': $scope.vessel.InspectionDateStr,
                        'InspectorComment': $scope.vessel.InspectorComment,
                        'DischargeCommencementDate': $scope.vessel.DischargeCommencementDateStr,
                        'DischargeCompletionDate': $scope.vessel.DischargeCompletionDateStr,
                        'QuantityOnBillOfLading': $scope.vessel.QuantityOnBillOfLading,
                        'QuantityAfterSTS': $scope.vessel.QuantityAfterSTS,

                        'VesselArrivalDate': $scope.vessel.VesselArrivalDateStr,
                        'LoadPortCoQAvailable': $scope.vessel.LoadPortCoQAvailable,
                        'RecommendationId': $scope.vessel.RecommendationId

                    };

            }

            //angular.forEach($scope.Products, function (o, i) {
            //    if (o.ProductId === response.ProductId) {
            //        $scope.vessel.Product = o;
            //    }
            //});


        };



        //Product Quantity Analysis
        $scope.vesselJettyReport = function (appId) {

            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $http({ method: 'GET', url: '/EmployeeProfile/IsCheckListComplete?id=' + appId }).success(function (response) {
                if (response == null) {
                    alert('Information could not be retrieved.');
                    return;
                }

                if (response.IsAccepted === true) {


                    $scope.viewD = false;




                    $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationFromDetail?id=' + appId }).success(function (response) {
                        if (response == null || response.NotificationId < 1) {
                            alert('Notification Information could not be retrieved.');
                            return;
                        }
                        $scope.application = response;


                        //vessel report saved
                        if ($scope.application.IsVesselReportSaved === true) {



                            notificationTrackService.retrieveSavedVesselReport(appId, $scope.retrieveSavedVesselReportCompleted);


                            $scope.viewJettyReport = true;
                        }



                            //vessel report not submitted
                        else if ($scope.application.IsVesselReportSubmitted !== true) {


                            $scope.viewJettyReport = true;

                         

                            $scope.vessel =
                             {

                                 'NotificationId': '',
                                 'QuantityOnVessel': '',
                                 'QuantityDischarged': '',
                                 'ProductName': $scope.application.ProductName,
                                 'DepotName': $scope.application.DepotName,
                                 'InspectionDate': '',
                                 'InspectorComment': '',
                                 'DischargeCommencementDate': '',
                                 'DischargeCompletionDate': '',
                                 'QuantityOnBillOfLading': '',
                                 'QuantityAfterSTS': '',
                                 'VesselArrivalDate': '',
                                 'LoadPortCoQAvailable': '',
                                 'RecommendationId': ''

                             };
                        }

                    });



                }
                else if (response.IsValid === false) {

                    alert('Not all the notification documents are validated.');

                    return;

                }


            });





        };


       



        //recertification
        $scope.recertification = function (appId) {

            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.saveVesselReport(appId);

            $scope.viewReport = false;
            $scope.viewD = false;




            $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationFromDetail?id=' + appId }).success(function (response) {
                if (response == null || response.NotificationId < 1) {
                    alert('Notification Information could not be retrieved.');
                    return;
                }
                $scope.application = response;


                //recertification stored
                if ($scope.application.IsRecertificationSaved === true) {

                    //$scope.viewChecklist = false;

                    notificationTrackService.retrieveStoredRecertification(appId, $scope.retrieveStoredRecertificationCompleted);



                    //$http({ method: 'GET', url: '/EmployeeProfile/GetGenericList' }).success(function (response) {
                    //    if (response == null) {
                    //        alert('Information could not be retrieved.');
                    //        return;
                    //    }
                    //    $scope.DischargeApprovals = response.DischargeApprovals;

                    //});

                    $http({ method: 'GET', url: '/EmployeeProfile/GetProductColourList' }).success(function (response) {
                        if (response == null) {
                            alert('Information could not be retrieved.');
                            return;
                        }
                        $scope.ProductColours = response.ProductColours;

                    });
                    $scope.viewReport = false;
                    $scope.viewRecertification = true;
                }



                    //recertification not submitted
                else if ($scope.application.IsRecertificationSubmitted !== true) {

                    //$scope.viewChecklist = false;
                    $scope.viewReport = false;
                    $scope.viewRecertification = true;

                    //$http({ method: 'GET', url: '/EmployeeProfile/GetGenericList' }).success(function (response) {
                    //    if (response == null) {
                    //        alert('Issue Types could not be retrieved.');
                    //        return;
                    //    }
                    //    $scope.DischargeApprovals = response.DischargeApprovals;

                    //});

                    $http({ method: 'GET', url: '/EmployeeProfile/GetProductColourList' }).success(function (response) {
                        if (response == null) {
                            alert('Information could not be retrieved.');
                            return;
                        }
                        $scope.ProductColours = response.ProductColours;

                    });

                    $scope.recert =
                     {

                         'Id': '',
                         'NotificationId': '',
                         'Density': '',
                         'Flashpoint': '',
                         'InitialBoilingPoint': '',
                         'TotalSulphur': '',
                         'ResearchOctaneNumber': '',
                         'REIDVapourPressure': '',
                         'Benzene': '',
                         'Ethanol': '',
                         'DieselIndex': '',
                         'FreezingPoint': '',
                         'MSEP': '',
                         'DoctorTest': '',
                         'OffLimitVariance': '',
                         'WithinLimitVariance': '',
                         'EmployeeId': '',
                         'CaptainName': '',
                         'DischargeApproval': { 'Id': '', 'Name': 'Select Approval' }
                     };
                }

            });

        };


        //store recertification
        $scope.storeRecertification = function (notId) {
            if (parseInt(notId) < 1) {
                alert('Invalid selection!');
                return;
            }

            var ColourId = '';
            if ($scope.recert.Colour !== null && $scope.recert.Colour !== undefined) {
                ColourId = $scope.recert.Colour.Id;
            }

            if ($scope.recert.Density === "") {
                alert('You must provide the product density!');
                return;
            }

            if ($scope.recert.InitialBoilingPoint === "") {
                alert('You must provide the product initialBoilingPoint!');
                return;
            }

            if ($scope.recert.FinalBoilingPoint === "") {
                alert('You must provide the product finalBoilingPoint!');
                return;
            }
            if ($scope.recert.Colour === "") {
                alert('You must provide the product colour!');
                return;
            }

            var req =
                     {


                         'NotificationId': notId,
                         'Density': $scope.recert.Density,
                         'Flashpoint': $scope.recert.Flashpoint,
                         'InitialBoilingPoint': $scope.recert.InitialBoilingPoint,
                         'FinalBoilingPoint': $scope.recert.FinalBoilingPoint,
                         'TotalAcidity': $scope.recert.TotalAcidity,
                         'TotalSulphur': $scope.recert.TotalSulphur,
                         'ResearchOctaneNumber': $scope.recert.ResearchOctaneNumber,
                         'REIDVapourPressure': $scope.recert.REIDVapourPressure,
                         'Benzene': $scope.recert.Benzene,
                         'Colour': $scope.recert.Colour,
                         'ProductColour': ColourId,
                         'Ethanol': $scope.recert.Ethanol,
                         'DieselIndex': $scope.recert.DieselIndex,
                         'FreezingPoint': $scope.recert.FreezingPoint,
                         'MSEP': $scope.recert.MSEP,
                         'DoctorTest': $scope.recert.DoctorTest,
                         'LimitVariance': $scope.recert.LimitVariance,
                         'Spec': $scope.recert.Spec,
                         'RecommendationId': $scope.recert.Recommendation,
                         'CaptainName': $scope.recert.CaptainName,
                         'DischargeApproval': $scope.recert.DischargeApproval
                     };

            notificationTrackService.storeRecertification(req, $scope.storeRecertificationCompleted);
        };


        $scope.storeRecertificationCompleted = function (response) {


            if (response.IsAccepted === true) {


                $scope.success = 'Your Information have been stored for future reference';
                $scope.positivefeedback = true;

                $scope.BackDetailRecertification(response.Id);


            }


        };


        // retrieve stored recertification  
        $scope.retrieveStoredRecertificationCompleted = function (response) {


            if (response !== null) {

                $scope.recert = response;
                $scope.recert =
                    {


                        'NotificationId': $scope.recert.notId,
                        'Density': $scope.recert.Density,
                        'Flashpoint': $scope.recert.Flashpoint,
                        'InitialBoilingPoint': $scope.recert.InitialBoilingPoint,
                        'FinalBoilingPoint': $scope.recert.FinalBoilingPoint,
                        'TotalAcidity': $scope.recert.TotalAcidity,
                        'TotalSulphur': $scope.recert.TotalSulphur,
                        'ResearchOctaneNumber': $scope.recert.ResearchOctaneNumber,
                        'REIDVapourPressure': $scope.recert.REIDVapourPressure,
                        'Benzene': $scope.recert.Benzene,
                        'Ethanol': $scope.recert.Ethanol,
                        'DieselIndex': $scope.recert.DieselIndex,
                        'FreezingPoint': $scope.recert.FreezingPoint,
                        'MSEP': $scope.recert.MSEP,
                        'DoctorTest': $scope.recert.DoctorTest,
                        'LimitVariance': $scope.recert.LimitVariance,
                        'Spec': $scope.recert.Spec,
                        'Recommendation': $scope.recert.RecommendationId,
                        'CaptainName': $scope.recert.CaptainName,
                        'DischargeApproval': $scope.recert.DischargeApproval
                    };

                angular.forEach($scope.ProductColours, function (o, i) {
                    if (o.Id === response.ProductColour) {
                        $scope.recert.Colour = o;
                    }
                });

               



            }

           


        };


        //submit recertification
        $scope.submitRecertification = function (notId) {
            if (parseInt(notId) < 1) {
                alert('Invalid selection!');
                return;
            }


            var req =
                    {


                        'NotificationId': notId,
                        'Density': $scope.recert.Density,
                        'Flashpoint': $scope.recert.Flashpoint,
                        'InitialBoilingPoint': $scope.recert.InitialBoilingPoint,
                        'FinalBoilingPoint': $scope.recert.FinalBoilingPoint,
                        'TotalAcidity': $scope.recert.TotalAcidity,
                        'TotalSulphur': $scope.recert.TotalSulphur,
                        'ResearchOctaneNumber': $scope.recert.ResearchOctaneNumber,
                        'REIDVapourPressure': $scope.recert.REIDVapourPressure,
                        'Benzene': $scope.recert.Benzene,
                        'Colour': $scope.recert.Colour,
                        'Ethanol': $scope.recert.Ethanol,
                        'DieselIndex': $scope.recert.DieselIndex,
                        'FreezingPoint': $scope.recert.FreezingPoint,
                        'MSEP': $scope.recert.MSEP,
                        'DoctorTest': $scope.recert.DoctorTest,
                        'OffLimitVariance': $scope.recert.OffLimitVariance,
                        'WithinLimitVariance': $scope.recert.WithinLimitVariance,

                        'CaptainName': $scope.recert.CaptainName,
                        'DischargeApproval': $scope.recert.DischargeApproval.Id
                    };

            notificationTrackService.submitRecertification(req, $scope.submitRecertificationCompleted);


        };



        $scope.submitRecertificationCompleted = function (response) {


            if (response.IsAccepted === true) {


                $scope.success = 'Your Information have been submitted';
                $scope.positivefeedback = true;




            }


        };


        //print recertification
        $scope.printRecertification = function (notId) {
            if (parseInt(notId) < 1) {
                alert('Invalid selection!');
                return;
            }




            notificationTrackService.printRecertification(notId, $scope.printRecertificationCompleted);


        };



        $scope.printRecertificationCompleted = function (response) {


            window.open(response);

        };



        //discharge data
        $scope.dischargedata = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $http({ method: 'GET', url: '/EmployeeProfile/IsVesselReportComplete?id=' + appId }).success(function (response) {
                if (response.IsNull === true) {
                    alert('There are fields that are not yet provided on the Product QA.');
                    return;
                }
                else if (response.IsAccepted === true) {
                   
            $scope.viewD = false;

            $http({ method: 'GET', url: '/EmployeeProfile/GetTankList?id=' + appId }).success(function (response) {
                if (response == null) {
                    alert('Information could not be retrieved.');
                    return;
                }
                $scope.Tanks = response.Tanks;
                $scope.viewDischargeData = true;
            });
                }

                });

            
        };



        $scope.showTankForm = function () {
            $scope.viewEditParameters = false;

            $scope.viewFinal = false;
            $scope.viewAddParameters = true;

        };

        //view edit form
        $scope.editTank = function (tankid) {

            if (parseInt(tankid) < 1) {
                alert('Invalid selection!');
                return;
            }
           

            var req =
           {
               'NotificationId': $scope.Id,
               'TankId': tankid


           };
            notificationTrackService.editTank(req, $scope.editTankInfoCompleted);
            //$scope.viewParameters = true;

        };



        $scope.editTankInfoCompleted = function (response) {


            if (response == null) {
                alert('Discharge Data could not be retrieved.');
                return;
            }
            //discharge data saved
            if (response.IsDischargeDataCreated === true) {

                $scope.enter =
                {
                    'TankNo': response.TankName,

                    'TankGuageBefore': response.DischargeParameterBeforeObject.TankGuage,
                    'TankGuageAfter': response.DischargeParameterAfterObject.TankGuage,

                    'TankTCAfter': response.DischargeParameterAfterObject.TankTC,
                    'TankTCBefore': response.DischargeParameterBeforeObject.TankTC,

                    'CrossVol_TkPcLTRSBefore': response.DischargeParameterBeforeObject.CrossVol_TkPcLTRS,
                    'CrossVol_TkPcLTRSAfter': response.DischargeParameterAfterObject.CrossVol_TkPcLTRS,


                    'SGtC_LabBefore': response.DischargeParameterBeforeObject.SGtC_Lab,
                    'SGtC_LabAfter': response.DischargeParameterAfterObject.SGtC_Lab,


                    'VolOfWaterLTRSBefore': response.DischargeParameterBeforeObject.VolOfWaterLTRS,
                    'VolOfWaterLTRSAfter': response.DischargeParameterAfterObject.VolOfWaterLTRS,

                    'VolCorrFactorBefore': response.DischargeParameterBeforeObject.VolCorrFactor,
                    'VolCorrFactorAfter': response.DischargeParameterAfterObject.VolCorrFactor,


                    //'NetVolOfOil_TkTcBefore': response.DischargeParameterBeforeObject.NetVolOfOil_TkTc,
                    //'SG_515CBefore': response.DischargeParameterBeforeObject.SG_515C,
                    //'NetVol_1515CBefore': response.DischargeParameterBeforeObject.NetVol_1515C,
                    //'EquivVolInM_1515CBefore': response.DischargeParameterBeforeObject.EquivVolInM_1515C,

                    //'NetVolOfOil_TkTcAfter': response.DischargeParameterAfterObject.NetVolOfOil_TkTc,
                    //'SG_515CAfter': response.DischargeParameterAfterObject.SG_515C,
                    ////'NetVol_1515CAfter': response.DischargeParameterAfterObject.NetVol_1515C,
                    ////'EquivVolInM_1515CAfter': response.DischargeParameterAfterObject.EquivVolInM_1515C
                };
                $scope.viewEditParameters = true;
                $scope.viewAddParameters = false;

                $scope.viewFinal = false;
                $scope.viewDry = false;
                $scope.viewROM = false;

            }
            else if (response.IsDischargeDataCreated !== true) {

                $scope.store =
            {

                'TankGuageBefore': '',
                'TankTCBefore': '',
                'TankGuageAfter': '',
                'TankTCAfter': '',
                'CrossVol_TkPcLTRSBefore': '',
                'SGtC_LabBefore': '',
                'VolOfWaterLTRSBefore': '',
                'NetVolOfOil_TkTcBefore': '',
                'SG_515CBefore': '',
                'VolCorrFactorBefore': '',
                'NetVol_1515CBefore': '',
                'EquivVolInM_1515CBefore': '',
                'CrossVol_TkPcLTRSAfter': '',
                'SGtC_LabAfter': '',
                'VolOfWaterLTRSAfter': '',
                'NetVolOfOil_TkTcAfter': '',
                'SG_515CAfter': '',
                'VolCorrFactorAfter': ''
                //'NetVol_1515CAfter': '',
                //'EquivVolInM_1515CAfter': ''
            };

                $scope.viewParameters = true;

            }




        }



        //show tanks
        $scope.showTanks = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $http({ method: 'GET', url: '/EmployeeProfile/ShowTanks?id=' + appId }).success(function (response) {
                if (response == null || response.IsError === true) {
                    alert('There is a problem with the network.');
                    return;
                }

                    //check for error
                else if (response.IsError === true) {

                    alert('There is a problem with the network, try again later.');
                    return;


                }


                    //check if checklist is submitted
                else if (response.IsChecklistSaved === false) {

                    alert('You have not filled the Checklist.');
                    return;


                }

                    //check if vessel report is submitted
                else if (response.IsVesselReport === false) {

                    alert('You have not filled the Product QA.');
                    return;


                }


                    //check if recertification is submitted
                else if (response.IsRecertificationSaved === false) {

                    alert('You have not filled the Product QA.');
                    return;


                }

                    //check if discharge data is submitted
                else if (response.IsDischargeDataSaved === false) {

                    $scope.viewD = false;
                    $scope.application = response;
                    $scope.Id = appId;
                    $scope.viewFirstTank = true;


                } else {
                    $scope.viewD = false;
                    $scope.viewEditParameters = false;
                    $scope.viewAddParameters = false;

                    $scope.application = response;
                    $scope.Id = appId;
                    $scope.viewSummary = true;
                    $scope.viewFinal = true;
                    $scope.viewDry = true;
                    $scope.viewROM = true;

                }

            });

        };


      



        //save discharge data
        $scope.saveDischargeData = function (notid) {

            if (parseInt(notid) < 1) {
                alert('Invalid selection!');
                return;
            }
          

            var req =
           {
               'NotificationId': notid,
               'TankNo': $scope.store.TankNo,

               'TankGuageBefore': $scope.store.TankGuageBefore,
               'TankGuageAfter': $scope.store.TankGuageAfter,

               'TankTCBefore': $scope.store.TankTCBefore,
               'TankTCAfter': $scope.store.TankTCAfter,

               'CrossVol_TkPcLTRSBefore': $scope.store.CrossVol_TkPcLTRSBefore,
               'CrossVol_TkPcLTRSAfter': $scope.store.CrossVol_TkPcLTRSAfter,

               'SGtC_LabBefore': $scope.store.SGtC_LabBefore,
               'SGtC_LabAfter': $scope.store.SGtC_LabAfter,

               'VolOfWaterLTRSBefore': $scope.store.VolOfWaterLTRSBefore,
               'VolOfWaterLTRSAfter': $scope.store.VolOfWaterLTRSAfter,

               'VolCorrFactorBefore': $scope.store.VolCorrFactorBefore,
               'VolCorrFactorAfter': $scope.store.VolCorrFactorAfter

           };
            notificationTrackService.saveDischargeData(req, $scope.saveDischargeDataCompleted);

        };




        $scope.saveDischargeDataCompleted = function (response) {

            if (response == null || response.IsError) {
                alert('There is a problem with the network.');
                return;
            }

            else if (response.IsAccepted === true) {

               

                $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationBack?id=' + response.notid }).success(function (back) {
                    if (back == null || back.Id < 1) {
                        alert('Information could not be retrieved.');
                        return;
                    }
                    $scope.application = back;

                    $scope.viewFirstTank = false;
                    $scope.viewSummary = false;
                    $scope.viewAddParameters = false;
                    $scope.viewD = true;
                    $scope.success = 'Your Information have been submitted';
                    $scope.positivefeedback = true;
                });


            }

           
           

        };


        //save discharge data
        $scope.updateDischargeData = function (notid) {

            if (parseInt(notid) < 1) {
                alert('Invalid selection!');
                return;
            }


            var req =
           {
               'NotificationId': notid,
               'TankNo': $scope.enter.TankNo,

               'TankGuageBefore': $scope.enter.TankGuageBefore,
               'TankGuageAfter': $scope.enter.TankGuageAfter,

               'TankTCBefore': $scope.enter.TankTCBefore,
               'TankTCAfter': $scope.enter.TankTCAfter,

               'CrossVol_TkPcLTRSBefore': $scope.enter.CrossVol_TkPcLTRSBefore,
               'CrossVol_TkPcLTRSAfter': $scope.enter.CrossVol_TkPcLTRSAfter,

               'SGtC_LabBefore': $scope.enter.SGtC_LabBefore,
               'SGtC_LabAfter': $scope.enter.SGtC_LabAfter,

               'VolOfWaterLTRSBefore': $scope.enter.VolOfWaterLTRSBefore,
               'VolOfWaterLTRSAfter': $scope.enter.VolOfWaterLTRSAfter,

               'VolCorrFactorBefore': $scope.enter.VolCorrFactorBefore,
               'VolCorrFactorAfter': $scope.enter.VolCorrFactorAfter

           };
            notificationTrackService.updateDischargeData(req, $scope.updateDischargeDataCompleted);

        };




        $scope.updateDischargeDataCompleted = function (response) {

            if (response == null || response.IsError) {
                alert('There is a problem with the network.');
                return;
            }

            else if (response.IsAccepted === true) {



                $http({ method: 'GET', url: '/EmployeeProfile/GetNotificationBack?id=' + response.notid }).success(function (back) {
                    if (back == null || back.Id < 1) {
                        alert('Information could not be retrieved.');
                        return;
                    }
                    $scope.application = back;

                    $scope.viewFirstTank = false;
                    $scope.viewSummary = false;
                    $scope.viewEditParameters = false;
                    $scope.viewD = true;
                    $scope.success = 'Your Information have been submitted';
                    $scope.positivefeedback = true;
                });


            }




        };



        //print discharge data
        $scope.printDischargeData = function (notid) {

            if (parseInt(notid) < 1) {
                alert('Invalid selection!');
                return;
            }
           

            var req =
           {
               'NotificationId': notid
              




           };
            notificationTrackService.printDischargeData(req, $scope.printDischargeDataCompleted);

        };


        $scope.printDischargeDataCompleted = function (response) {

            window.open(response.SmallPath);



            //$timeout(function () {
            //    $http({ method: 'POST', url: '/EmployeeProfile/DeleteFile?way=' + response.BigPath }).success(function (response2) {

            //        return;

            //    });
            //}, 5000);


        };



        //report
        $scope.report = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $http({ method: 'GET', url: '/EmployeeProfile/SubmitFinalReport?id=' + appId }).success(function (response) {
                if (response == null || response.IsError === true) {
                    alert('There is a problem with the network.');
                    return;
                }

                    //check for error
                else if (response.IsNull === true) {

                    alert('There is a problem with the network, try again later.');
                    return;


                }

             
                else if (response.IsAccepted === true) {

                    alert('Your application has been accepted, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();

                }


                else if (response.IsCertificateGenerated === true) {

                    alert('Your application has been accepted, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();

                }

                //$scope.viewD = false;
                //$scope.application = response;
                //$scope.NotificationId = response[0].NotificationId;
                //$scope.viewSummary = true;

            });

        };

        //report Jetty
        $scope.reportJetty = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $http({ method: 'GET', url: '/EmployeeProfile/SubmitJettyReport?id=' + appId }).success(function (response) {
                if (response == null || response.IsError === true) {
                    alert('There is a problem with the network.');
                    return;
                }

                    //check for error
                else if (response.IsError === true) {

                    alert('There is a problem with the network, try again later.');
                    return;


                }


                    //check if checklist is submitted
                else if (response.IsChecklistSaved === false) {

                    alert('You have not filled the Checklist.');
                    return;


                }

                    //check if vessel report is submitted
                else if (response.IsVesselReport === false) {

                    alert('You have not filled the Product Quantity Analysis.');
                    return;


                }


                    //check if recertification is submitted
                else if (response.IsRecertificationSaved === false) {

                    alert('You have not filled the Product Quality Analysis.');
                    return;


                }

                alert('Your application has been accepted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();

            });

        };



        // final report
        $scope.reportFinal = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }


            $http({ method: 'GET', url: '/EmployeeProfile/SubmitFinalReport?id=' + appId }).success(function (response) {
                if (response == null || response.IsError === true) {
                    alert('There is a problem with the network.');
                    return;
                }

                   
                else if (response.IsAccepted === true) {

                    alert('Your application has been accepted, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();

                }


                else if (response.IsCertificateGenerated === true) {

                    alert('Your application has been accepted, Click to continue');
                    $scope.initializeApp();
                    $route.reload();
                    $window.location.reload();

                }

            });

        };




        $scope.viewHistory = function (appId) {
            if (parseInt(appId) < 1) {
                alert('Invalid selection!');
                return;
            }

            $scope.viewD = false;

            notificationTrackService.getHistory(appId, $scope.processHistoryCompleted);



        };


        //process history completed
        $scope.processHistoryCompleted = function (response) {
            if (response == null) {
                alert('History could not be retrieved.');
                return;
            }
            $scope.history = response;

            $scope.viewH = true;

        };


        $scope.showDryTank = function () {
            $scope.viewComFile = true;
            //$scope.viewTempFile = false;
            $scope.check = true;
        };


        //$scope.showROM = function () {
        //    $scope.viewTempFile = true;
        //    $scope.viewComFile = false;
        //    $scope.check = false;
        //};

        $scope.showROM = function () {
            $scope.viewComFile = true;
            $scope.check = false;
        };

        $scope.uploadFile = function(files) {
            var fd = new FormData();
            //Take the first selected file
            fd.append("file", files[0]);
            var url = "";

            if ($scope.check === true) {
                url = "/EmployeeProfile/SaveTankFile?notificationId=" + $scope.Id;
                $scope.viewDry = false;
            }

            else if ($scope.check === false) {
                url = "/EmployeeProfile/SaveTankFile2?notificationId=" + $scope.Id;
                $scope.viewROM = false;
            }

            //var url = "/EmployeeProfile/SaveTankFile?notificationId=" + $scope.application[0].NotificationId;


            $http.post(url, fd, {
                withCredentials: true,
                headers: {'Content-Type': undefined },
                transformRequest: angular.identity
            }).success( $scope.viewComFile = false);
            
        };

        $scope.uploadFile2 = function (files) {
            var fd = new FormData();
            //Take the first selected file
            fd.append("file2", files[0]);

            var url = "/EmployeeProfile/SaveTankFile2?notificationId=" + $scope.Id;

            $http.post(url, fd, {
                withCredentials: true,
                headers: { 'Content-Type': undefined },
                transformRequest: angular.identity
            }).success($scope.viewTempFile = true);
            $scope.viewROM = false;
        };



        //team leader job assignment
       
        $scope.assignJob = function (notid) {

            if (parseInt(notid) < 1) {
                alert('Invalid selection!');
                return;
            }
            if ($scope.leader == null) {
                alert('An error was encountered. Please refresh the page and try again.');
                return;
            }

            var req =
           {
               'ApplicationId': notid,
               'EmployeeId': $scope.leader.Info.Id


           };
            notificationTrackService.assignJob(req, $scope.assignJobCompleted);
            //$scope.viewParameters = true;

        };



        $scope.assignJobCompleted = function (response) {
            if (response == null || response.IsError === true) {
                alert('There is a problem with the network.');
                return;
            }
            else if (response.IsAccepted === true) {
                alert('Your application has been accepted, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();

            }
            else if (response.IsCertificateGenerated === true) {
                alert('Your application has been accepted and the certifcate has been generated, Click to continue');

                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.IsFinal === true) {
                alert('This is the final step and certificate has been generated, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.CantGeneratePermit === true) {
                alert('This is the final step but certificate is not generated, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }
            else if (response.NoEmployee === true) {
                alert('There is no Employee to assign the task, Click to continue');
                $scope.initializeApp();
                $route.reload();
                $window.location.reload();
            }

        };


      

        $scope.initializeController = function() {
            $rootScope.setPageTitle('Notification Tracking');
        };
    }]);

});



