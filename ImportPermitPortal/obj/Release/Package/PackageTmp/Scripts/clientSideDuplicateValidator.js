

$(document).ready(function()
{
    var cssrules =  $("<style type='text/css'> </style>").appendTo("head");
    cssrules.append(".duplicateErr{border: 1px solid red;}");
});   

function checkDuplicates(xcid, formId, message)
{
    var tglClss = $('#' + formId).find('.duplicateErr');
    if (tglClss.length > 0) {
        tglClss.each(function () {
            var dds = [];
            var dtCcl = $(this);
            var dtCl = $(this).val().replace(/ /g, '').trim().toLowerCase();
            
            $('#' + formId + ' :text, textarea').each(function ()
            {
                if ($(this).val().replace(/ /g, '').trim().toLowerCase() === dtCl && $(this).attr('id') !== $(dtCcl).attr('id'))
                {
                    $(dtCcl).attr('title', message);
                    $(dtCcl).addClass('duplicateErr');
                    dds.push(dtCcl);
                }
            });

            if (dds.length < 1)
            {
                $(dtCcl).attr('title', '');
                $(dtCcl).removeClass('duplicateErr');
            }
        });

    }

    var tdxf = $(xcid).val().replace(/ /g, '').trim().toLowerCase();

    $('#' + formId + ' :text, textarea').each(function ()
    {
        if ($(this).val().replace(/ /g, '').trim().toLowerCase() === tdxf && $(this).attr('id') !== $(xcid).attr('id'))
        {
            $(xcid).attr('title', message);
            $(xcid).addClass('duplicateErr');
        }
    });

}
