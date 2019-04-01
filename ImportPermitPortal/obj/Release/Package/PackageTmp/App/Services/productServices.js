define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('productService', ['ajaxServices', function (ajaxServices)
    {
        this.addProduct = function (product, callbackFunction)
        {
            return ajaxServices.AjaxPost({ product: product }, "/Product/AddProduct", callbackFunction);
        };

        this.getProduct = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/Product/GetProduct?id=" + id, callbackFunction);
        };
        
        this.editProduct = function (product, callbackFunction)
        {
            return ajaxServices.AjaxPost({ product: product }, "/Product/EditProduct", callbackFunction);
        };

        this.deleteProduct = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/Product/DeleteProduct?id=" + id, callbackFunction);
        };
    
        this.getDocTypes = function (callbackFunction) {
            return ajaxServices.AjaxGet("/Product/GetDocTypes", callbackFunction);
        };

    }]);
});