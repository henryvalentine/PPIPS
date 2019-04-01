define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('faqService', ['ajaxServices', function (ajaxServices)
    {
        this.addFaq = function (faq, callbackFunction)
        {
            return ajaxServices.AjaxPost({ faq: faq }, "/Faq/AddFaq", callbackFunction);
        };

        this.getFaq = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Faq/GetFaq?id=" + id, callbackFunction);
        };
        
        this.getFaqCategories = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/Faq/GetFaqCategories", callbackFunction);
        };

        this.getFaqs = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Faq/GetFaqs", callbackFunction);
        };

        this.editFaq = function (faq, callbackFunction)
        {
            return ajaxServices.AjaxPost({ faq: faq }, "/Faq/EditFaq", callbackFunction);
        };

        this.deleteFaq = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Faq/DeleteFaq?id=" + id, callbackFunction);
        };

    }]);
});