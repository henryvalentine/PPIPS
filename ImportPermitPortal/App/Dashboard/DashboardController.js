"use strict";

define(['application-configuration', 'ngDialog','userAppService', 'angularFileUpload', 'fileReader'], function (app) {
    
    app.register.controller('defaultController', ['ngDialog', '$scope', '$rootScope','$location',
    function (ngDialog, $scope, $rootScope, $location)
    {
        $scope.initializeController = function ()
        {
            var path = $location.absUrl();
            $scope.path = path;
            $rootScope.setPageTitle('Dashboard|DPR-PPIPS');
            $scope.initializeIssue();
            $scope.IsAdmin = false;
            $rootScope.isUser = false;
            $scope.IsUser = false;
            $scope.IsSupport = false;
            $scope.IsBanker = false;
            $scope.IsApplicant = false;
            $scope.IsEmployee = false;
            $scope.IsDepotOwner = false;
            $scope.IsAccountant = false;
            $scope.IsApprover = false;

            if (path.indexOf("ngx.html") <= -1)
            {
                $rootScope.isUser = true;
                $scope.IsUser = true;
            }
           
            if (path.indexOf("ngx.html") > -1)
            {
                $scope.IsAdmin = true;
                $scope.apphref = 'ngx.html#AdminApplications/AdminApplications';
                $scope.ntfhref = 'ngx.html#AdminNotifications/AdminNotifications';
                $scope.ptfhref = 'ngx.html#Permit/Permit';
                $scope.rtfhref = 'ngx.html#AdminRecertifications/AdminRecertifications';
                
                $rootScope.getAppCount("/Application/GetAdminCounts", $scope.getAppCountCompleted);
            }
            
            if (path.indexOf("ngs.html") > -1)
            {
                $scope.IsSupport = true;
                $scope.apphref = 'ngs.html#AdminApplications/AdminApplications';
                $scope.ntfhref = 'ngs.html#AdminNotifications/AdminNotifications';
                $scope.ptfhref = 'ngs.html#Permit/Permit';
                $scope.rtfhref = 'ngs.html#AdminRecertifications/AdminRecertifications';
                $rootScope.getAppCount("/Application/GetAdminCounts", $scope.getAppCountCompleted);
            }
            

            if (path.indexOf("ngy.html") > -1)
            {
                $scope.IsApplicant = true;

                $scope.apphref = 'ngy.html#Application/MyApplications';
                $scope.ntfhref = 'ngy.html#Notification/MyNotifications';
                $scope.ptfhref = 'ngy.html#ApplicantPermit/ApplicantPermit';
                $scope.rtfhref = 'ngy.html#Recertify/Recertify';
                $rootScope.getAppCount("/Application/GetAppCount", $scope.getOtherAppCountCompleted);
            }

            if (path.indexOf("bnkAd.html") > -1 || path.indexOf("bnkUsr.html") > -1)
            {
                $scope.IsBanker = true;

                if (path.indexOf("bnkAd.html") > -1)
                {
                    $scope.apphref = 'bnkAd.html#BnkAdmin/BnkAdmin';
                    $scope.ntfhref = 'bnkAd.html#BnkAdmin/BnkAdminNotifications';
                    $scope.ptfhref = '#';
                    $scope.rtfhref = '#';
                }
                else
                {
                    $scope.apphref = 'bnkUsr.html#BnkAppUsr/BnkAppUsr';
                    $scope.ntfhref = 'bnkUsr.html#BnkAppUsr/Notifications';
                    $scope.ptfhref = '#';
                    $scope.rtfhref = '#';
                }

                $rootScope.getAppCount("/Application/GetBankerCounts", $scope.getOtherAppCountCompleted);
            }
            
            if (path.indexOf("depot_Owner.html") > -1)
            {
                $scope.IsDepotOwner = true;

                $scope.apphref = 'depot_Owner.html#DepotOwner/DepotOwner';
                $scope.ntfhref = '#';
                $scope.ptfhref = 'depot_Owner.html#DepotOwner/ThroughputHistory';
                $scope.rtfhref = '';

                $rootScope.getAppCount("/Application/GetDepotOwnerCounts", $scope.getOtherAppCountCompleted);
            }

            if (path.indexOf("ngVf.html") > -1) {
                $scope.IsVerifier = true;

                $rootScope.getAppCount("/Application/GetVerifierCounts", $scope.getVerifyingCountCompleted);
            }


            if (path.indexOf("nge.html") > -1)
            {

                $scope.IsEmployee = true;

                $scope.apphref = 'nge.html#EmployeeProfile/EmployeeProfile';
                $scope.ntfhref = 'nge.html#EmployeeNotificationTrack/EmployeeNotificationTrack';
                $scope.ptfhref = '#';
                $scope.rtfhref = 'nge.html#EmployeeRecertification/EmployeeRecertification';
                $rootScope.getAppCount("/EmployeeProfile/Dashboard", $scope.getEmployeeCountCompleted);
            }


            if (path.indexOf("ngappr.html") > -1)
            {

                $scope.IsEmployee = true;
                $scope.IsApprover = true;
                $scope.apphref = 'ngappr.html#EmployeeProfile/EmployeeProfile';
                $scope.ntfhref = 'ngappr.html#EmployeeRecertification/EmployeeRecertification';
                $scope.ptfhref = '#';
                $scope.rtfhref = 'ngappr.html#EmployeeRecertification/EmployeeRecertification';
                $rootScope.getAppCount("/EmployeeProfile/Dashboard", $scope.getEmployeeCountCompleted);

            }
            
            if (path.indexOf("nga.html") > -1)
            {
                $scope.IsAccountant = true;

               $rootScope.getAppCount("/Application/GetAppCount", $scope.getAppCountCompleted);
            }

            if ($scope.IsUser === true)
            {
                $rootScope.getAppCount("/Account/GetMyLatestMessages", $scope.getLatestMsgsCompleted);
            }
            
        };

        $scope.initializeIssue = function ()
        {
            
            $scope.IssueObject = { 'Id': '', 'IssueLogId': '', 'AffectedUserId': '', 'ResolvedById': '', 'Status': '' }
            $scope.IssueObject.IssueCategoryObject = { 'Id': '', 'Name': '-- Select Request Category --' }
            $scope.IssueObject.IssueLogObject = { 'Id': '', 'IssueCategoryId': '', 'Issue': '', 'DateCreated': '', 'DateResolved': '' , 'IssueCategoryName' : ''}
        }

        $scope.getMsg = function (id)  
        {
            if (id < 1)
            {
                alert('Invalid selection');
            }

            $rootScope.getMsg(id, $scope.getMsgCompleted);

        };

        $scope.getLatestMsgsCompleted = function (data)
        {
            $scope.latestMsgs = data;

        };

        $scope.getMsgCompleted = function (message)
        {
            if (message.Id < 1)
            {
                alert('Message information could not be retrievd. Please try again');
            }

            $scope.mssg = message;
            $scope.html = message.MessageContent;
            ngDialog.open({
                template: '/App/Dashboard/ViewMessage.html',
                className: 'ngdialog-theme-flat',
                scope: $scope
            });
        };

        $scope.getAppCountCompleted = function (data)
        {
            angular.forEach(data, function(a, k) 
            {
                if ($scope.IsSupport === true)
                {
                    a.PaidAppHref = 'ngs.html#' + a.PaidAppHref;
                    a.SubmittedAppCountHref = 'ngs.html#' + a.SubmittedAppCountHref;
                    a.ApprovedAppHref = 'ngs.html#' + a.ApprovedAppHref;
                    a.ProcessingAppHref = 'ngs.html#' + a.ProcessingAppHref;
                    a.RejectedHref = 'ngs.html#' + a.RejectedHref;
                    a.TotalAppHref = 'ngs.html#' + a.TotalAppHref;
                } else
                {
                    {
                        if ($scope.IsAdmin === true)
                        {
                            a.PaidAppHref = 'ngx.html#' + a.PaidAppHref;
                            a.SubmittedAppCountHref = 'ngx.html#' + a.SubmittedAppCountHref;
                            a.ApprovedAppHref =  'ngx.html#' + a.ApprovedAppHref;
                            a.ProcessingAppHref =  'ngx.html#' + a.ProcessingAppHref;
                            a.RejectedHref = 'ngx.html#' + a.RejectedHref;
                            a.TotalAppHref = 'ngx.html#' + a.TotalAppHref;
                        }
                    }
                }
            });
            $scope.reports = data;

        };

        $scope.closeMsg = function ()
        {
            ngDialog.close('/App/Dashboard/ViewMessage.html', '');
        };
        

        $scope.getEmployeeCountCompleted = function (data)
        {
            $scope.appCount = data.ApplicationCount;
            $scope.ntfCount = data.NotificationCount;
            $scope.rctCount = data.RecertificationCount;
            $scope.expCount = data.ExpiringPermitCount;
            $rootScope.getAppCount("/Application/GetJobDistributions", $scope.getJobDistributionsCompleted);
        };

        $scope.getJobDistributionsCompleted = function (data)
        {
            $scope.jobDistributions = data;
        };

        $scope.getApps = function (employeeId)
        {
            if (employeeId < 1)
            {
                alert('Invalid selection');
                return;
            }
            $scope.hasTrack = false;
            $scope.employeeName = null;
            $scope.getAppProcesses = null;

            $rootScope.getAppCount("/Application/GetEmployeeApplicationProcesses?employeeId=" + employeeId, $scope.getAppsCompleted);
        };

        $scope.getAppsCompleted = function (data)
        {
            if (data == null || data.length < 1)
            {
                alert('Item information could not be retrieved');
                return;
            }

            $scope.employeeName = data[0].EmployeeName;
            $scope.getAppProcesses = data;
            $scope.hasTrack = true;
        };

        $scope.reset = function ()
        {
            $scope.hasTrack = false;
            $scope.employeeName = null;
            $scope.getAppProcesses = null;
        };
        
        $scope.getOtherAppCountCompleted = function (data) 
        {
            $scope.appCount = data.ApplicationCount;
            $scope.ntfCount = data.NotificationCount;
            $scope.expCount = data.ExpiringPermitCount;
            $scope.rctCount = data.RecertificationCount;
        };
        
        $scope.getVerifyingCountCompleted = function (data)
        {
            $scope.approvedAppCount = data.ApprovedApplicationCount;
            $scope.verifyingAppCount = data.VerificationApplicationCount;
        };

        $scope.closeMsg = function ()
        {
            ngDialog.close('/App/Dashboard/ViewMessage.html', '');
        };

        $scope.sendMsg = function ()
        {
            
            var issueObject = { 'Id': $scope.IssueObject.Id, 'IssueLogId': $scope.IssueObject.IssueLogId, 'AffectedUserId': $scope.IssueObject.AffectedUserId, 'ResolvedById': $scope.IssueObject.ResolvedById, 'Status': $scope.IssueObject.Status, 'IssueCategoryName': $scope.IssueObject.IssueCategoryObject.Name }
           
            issueObject.IssueLogObject = { 'Id': $scope.IssueObject.IssueLogObject.Id, 'IssueCategoryId': $scope.IssueObject.IssueCategoryObject.Id, 'Issue': $scope.IssueObject.IssueLogObject.Issue, 'DateCreated': $scope.IssueObject.IssueLogObject.DateCreated, 'DateResolved': $scope.IssueObject.IssueLogObject.DateResolved }
            
             if (issueObject.IssueLogObject == null)
            {
                alert('An error was encountered on the page. Please try again.');
                return;
            }

            if (issueObject.IssueLogObject.IssueCategoryId < 1)
            {
                alert('Please select Support Category.');
                return;
            }

            if (issueObject.IssueLogObject.Issue.length < 1)
            {
                alert('Please provide your request/complaint.');
                return;
            }

            $rootScope.postData(issueObject, "/Issue/SendSupportRequest", $scope.sendRequestCompleted);
        };
       
        $scope.sendRequestCompleted = function (data)
        {
            if (data.Code < 1)
            {
                $scope.appError = data.Error;
                $scope.isError = true;
                $scope.appSuccess = '';
                $scope.isSuccess = false;
                return;
            }
            $scope.initializeIssue();
            $scope.appSuccess = data.Error;
            $scope.isSuccess = true;
            $scope.appError = '';
            $scope.isError = false;
            $scope.msgBody = '';
            $scope.msgTitle = '';
        };
       
    }]);
       
});




