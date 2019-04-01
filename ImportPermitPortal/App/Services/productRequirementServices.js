define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('productRequirementService', ['ajaxServices', function (ajaxServices)
    {
        this.addProductRequirement = function (productRequirement, callbackFunction)
        {
            return ajaxServices.AjaxPost({ productRequirement: productRequirement }, "/ProductRequirement/AddProductRequirement", callbackFunction);
        };

        this.getProductRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/ProductRequirement/GetProductRequirement?id=" + id, callbackFunction);
        };
        
        this.getGenericList = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/ProductRequirement/GetGenericList", callbackFunction);
        };

        this.editProductRequirement = function (productRequirement, callbackFunction)
        {
            return ajaxServices.AjaxPost({ productRequirement: productRequirement }, "/ProductRequirement/EditProductRequirement", callbackFunction);
        };

        this.deleteProductRequirement = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/ProductRequirement/DeleteProductRequirement?id=" + id, callbackFunction);
        };
    

    }]);
});