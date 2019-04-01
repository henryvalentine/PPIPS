function refreshForm(formId, str)
{
    $('#' + formId + ' :text, textarea, date, datetime, tel, email, password').each(function ()
    {
        $(this).val(''); if ($('#' + formId + ' #sp' + $(this).attr('id')).length > 0)
        {
            $('#' + formId + ' #sp' + $(this).attr('id')).css({ 'display': 'none' });
        }
    });

    $('#' + formId).find('select').each(function ()
    {
        $(this).val("");

        if ($('#sp' + $(this).attr('id')).length > 0)
        {
            $('#' + formId + ' #sp' + $(this).attr('id')).css({ 'display': 'none' });
        }
    });
    
    $('#' + formId + ' input:checkbox').each(function ()
    {
         $(this).prop("checked", false);
    });
    $('#' + formId + ' input:radio').each(function () {
        $(this).prop("checked", false);
    });
    if (str.trim().length < 1)
    {
        $('#' + formId + ' :submit').val("Submit");
    }
    else
    {
         $('#' + formId + ' :submit').val(str);
    }
}

function toggleValidators(formId)
{
    $('#' + formId + ' :text, textarea, date, datetime, tel, email, password').each(function ()
    {
        $(this).on('change', function() {
            if ($(this).val().trim().length > 0 && $('#sp' + $(this).attr('id')).length > 0)
            {
                $('#' + formId + ' #sp' + $(this).attr('id')).fadeOut();
            }
            if ($(this).val().trim().length < 1)
            {
                $('#' + formId + ' #sp' + $(this).attr('id')).fadeIn();
            }
        });
    });
    $('#' + formId).find('select').each(function ()
    {
        $(this).on('change', function ()
        {
            if (parseInt($(this).val()) > 0 && $('#' + formId + ' #sp' + $(this).attr('id')).length > 0)
            {
                $('#' + formId + ' #sp' + $(this).attr('id')).fadeOut();
            }
            if (parseInt($(this).val()) < 1 && $('#sp' + $(this).attr('id')).length > 0)
            {
                $('#' + formId + ' #sp' + $(this).attr('id')).fadeIn();
            }
        });
    });
}

function validateTemplate(formId)
{
    var txh = ''; $('#' + formId + ' :text, textarea, tel, email, password').each(function ()
    {
        if (($(this).val().trim().length < 1 || parseInt($(this).val()) < 1) && $('#' + formId + ' #sp' + $(this).attr('id')).length > 0)
        {
            $('#' + formId + ' #sp' + $(this).attr('id')).fadeIn(); txh = 1;
        }
    });
    
    $('#' + formId).find('select').each(function ()
    {
        if (parseInt($(this).val()) < 1 && $('#' + formId + ' #sp' + $(this).attr('id')).length > 0)
        {
            $('#' + formId + ' #sp' + $(this).attr('id')).fadeIn(); txh = 1;
        }
    });
    
    if (txh === 1)
    {
        return false;
    }
    return true;
}