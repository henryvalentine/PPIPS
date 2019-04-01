define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('countryService', ['ajaxServices', function (ajaxServices) {
        this.addCountry = function (country, callbackFunction) {
            return ajaxServices.AjaxPost({ country: country }, "/Country/AddCountry", callbackFunction);
        };

        this.getCountry = function (id, callbackFunction) {
            return ajaxServices.AjaxGet("/Country/GetCountry?id=" + id, callbackFunction);
        };

        this.getGenericList = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Country/GetGenericList", callbackFunction);
        };

        this.editCountry = function (country, callbackFunction) {
            return ajaxServices.AjaxPost({ country: country }, "/Country/EditCountry", callbackFunction);
        };

        this.deleteCountry = function (id, callbackFunction) {
            return ajaxServices.AjaxDelete("/Country/DeleteCountry?id=" + id, callbackFunction);
        };


    }]);
});