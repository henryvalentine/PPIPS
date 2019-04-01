define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('messageTemplateService', ['ajaxServices', function (ajaxServices)
    {
        this.addMessageTemplate = function (msgTemplate, callbackFunction)
        {
            return ajaxServices.AjaxPost({ messageTemplate: msgTemplate }, "/MessageTemplate/AddMessageTemplate", callbackFunction);
        };

        this.getMessageTemplate = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/MessageTemplate/GetMessageTemplate?id=" + id, callbackFunction);
        };
        
        this.getMessageEvents = function (callbackFunction)
        {
            return ajaxServices.AjaxGet("/MessageTemplate/GetMessageEvents", callbackFunction);
        };

        this.editMessageTemplate = function (msgTemplate, callbackFunction)
        {
            return ajaxServices.AjaxPost({ messageTemplate: msgTemplate }, "/MessageTemplate/EditMessageTemplate", callbackFunction);
        };

        this.deleteMessageTemplate = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/MessageTemplate/DeleteMessageTemplate?id=" + id, callbackFunction);
        };
    

    }]);
});