
define(['application-configuration'], function (app)
{

    app.register.service('ajaxServices', ['$http', '$rootScope', function ($http, $rootScope)
    {
        this.AjaxPost = function (data, route, callbackFunction) {
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

        this.AjaxPostNoData = function (route, callbackFunction)
        {
            setUIBusy();
            setTimeout(function () {
                $http.post(route).success(function (response) {
                    stopUIBusy();
                    callbackFunction(response);
                });
            }, 1000);

        };
        
        this.AjaxDelete = function (route, callbackFunction)
        {
            setUIBusy();
            setTimeout(function () {
                $http.post(route).success(function (response)
                {
                    stopUIBusy();
                    callbackFunction(response, status);
                });
            }, 1000);

        };
        
        this.AjaxGet = function (route, callbackFunction)
        {
            setUIBusy();
            setTimeout(function () {
                $http({ method: 'GET', url: route }).success(function (response) {
                    stopUIBusy();
                    callbackFunction(response, status);
                });
               
            }, 1000);

        };

        this.AjaxGetWithData = function (data, route, successFunction) {
            setUIBusy();
            setTimeout(function ()
            {
                $http({ method: 'GET', url: route, params: data }).success(function (response)
                {
                    stopUIBusy();
                    successFunction(response);
                });
            }, 1000);
        };

        this.AjaxGetWithOutLoader = function (route, callbackFunction) {
            setTimeout(function () {
                $http({ method: 'GET', url: route }).success(function (response) {
                   callbackFunction(response, status);
                });

            }, 1000);

        };

        function setUIBusy() {
            $rootScope.busy = true;
        };

        function stopUIBusy() {
            $rootScope.busy = false;
        };

    }]);
});


