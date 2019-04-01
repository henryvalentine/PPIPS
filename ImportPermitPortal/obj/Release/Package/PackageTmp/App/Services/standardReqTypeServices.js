define(['application-configuration', 'ajaxServices'], function (app)
{
    app.register.service('standardReqTypeService', ['ajaxServices', function (ajaxServices)
    {
        this.addStandardRequirementType = function (sReqType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ sReqType: sReqType }, "/StandardRequirementType/AddStandardRequirementType", callbackFunction);
        };

        this.getStandardRequirementType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxGet("/StandardRequirementType/GetStandardRequirementType?id=" + id, callbackFunction);
        };
        
        this.editStandardRequirementType = function (sReqType, callbackFunction)
        {
            return ajaxServices.AjaxPost({ sReqType: sReqType }, "/StandardRequirementType/EditStandardRequirementType", callbackFunction);
        };
        
        this.deleteStandardRequirementType = function (id, callbackFunction)
        {
            return ajaxServices.AjaxDelete("/StandardRequirementType/DeleteStandardRequirementType?id=" + id, callbackFunction);
        };

    }]);
});