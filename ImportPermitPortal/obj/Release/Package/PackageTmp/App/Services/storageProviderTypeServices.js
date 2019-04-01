define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('storageProviderTypeService', ['ajaxServices', function (ajaxServices)
    {
        this.addStorageProviderType = function (storageProviderType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ storageProviderType: storageProviderType }, "/StorageProviderType/AddStorageProviderType", callbackFunction);
        };

        this.getStorageProviderType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/StorageProviderType/GetStorageProviderType?id=" + id, callbackFunction);
        };
        
        this.editStorageProviderType = function (storageProviderType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ storageProviderType: storageProviderType }, "/StorageProviderType/EditStorageProviderType", callbackFunction);
        };

        this.deleteStorageProviderType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/StorageProviderType/DeleteStorageProviderType?id=" + id, callbackFunction);
        };
    
        this.getDocTypes = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/StorageProviderType/GetDocTypes", callbackFunction);
        };

    }]);
});