define(['application-configuration', 'ajaxServices'], function (app) {
    app.register.service('faqCategoryService', ['ajaxServices', function (ajaxServices)
    {
        this.addFaqCategory = function (faqCategory, callbackFunction)
        {
            return ajaxServices.AjaxPost({ faqCategory: faqCategory }, "/FaqCategory/AddFaqCategory", callbackFunction);
        };

        this.getFaqCategory = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/FaqCategory/GetFaqCategory?id=" + id, callbackFunction);
        };

        this.editFaqCategory = function (faqCategory, callbackFunction)
        {
            return ajaxServices.AjaxPost({ faqCategory: faqCategory }, "/FaqCategory/EditFaqCategory", callbackFunction);
        };

        this.deleteFaqCategory = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/FaqCategory/DeleteFaqCategory?id=" + id, callbackFunction);
        };

    }]);
});