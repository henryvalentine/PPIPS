define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('productColumnService', ['ajaxServices', function (ajaxServices)
    {
        this.addProductColumn = function (productColumn, callbackFunction)
        {
            return ajaxServices.AjaxPost({ productColumn: productColumn }, "/ProductColumn/AddProductColumn", callbackFunction);
        };

        this.getProductColumn = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ProductColumn/GetProductColumn?id=" + id, callbackFunction);
        };
       
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ProductColumn/GetGenericList", callbackFunction);
        };

        this.editProductColumn = function (productColumn, callbackFunction)
        {
            return ajaxServices.AjaxPost({ productColumn: productColumn }, "/ProductColumn/EditProductColumn", callbackFunction);
        };

        this.deleteProductColumn = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ProductColumn/DeleteProductColumn?id=" + id, callbackFunction);
        };
    

    }]);
});