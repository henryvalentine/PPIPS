
function setModal(refdiv)
{
    if ($("#backFs").length < 1)
    {
        $('html :first-child').after($('<div id="backFs" style=" position: fixed; border: none; display: none; z-index: 10000; "> </div>'));
        //var wd = $(window).width();
        //var ht = $(window).height();
        $('#backFs').addClass('overlay3');
    }
    var ftvx = $("#backFs");
   var height = $(window).scrollTop() + 150;
   refdiv.css({ "z-index": "21", "position": "absolute", "top": height, "left": "430px", 'border': '1px solid green', 'box-shadow': '1px 1px 1px 1px #888888' });
    ftvx.fadeIn("fast");
    refdiv.show(300);
}

function closePopModal(refdiv)
{
    var xxvx = $("#backFs");
    refdiv.hide("slow");
    xxvx.fadeOut("fast");
    //xxvx.remove();
}

function setModal2(refdiv) {
    if ($("#backFs").length < 1) {
        $('html :first-child').after($('<div id="backFs" style=" position: fixed; border: none; display: none; z-index: 10000; "> </div>'));
        //var wd = $(window).width();
        //var ht = $(window).height();
        $('#backFs').addClass('overlay3');
    }
    var ftvx = $("#backFs");
    ftvx.fadeIn("fast");
    refdiv.show("fast");
}

function setModal3(refdiv) {
    var ftvx = $("#backFs");
    ftvx.fadeIn("fast");
    //var height = $(window).scrollTop() + 150;
    //refdiv.css({ "top": height });
    refdiv.fadeIn("fast");
}


function xSetModal(refdiv) {
    if ($("#backFs").length < 1) {
        $('html :first-child').after($('<div id="backFs" style=" position: fixed; border: none; display: none; z-index: 10000; "> </div>'));
        //var wd = $(window).width();
        //var ht = $(window).height();
        $('#backFs').addClass('overlay3');
    }
    var ftvx = $("#backFs");
    var height = $(window).scrollTop() + 60;
    refdiv.css({ "z-index": "21", "position": "absolute", "top": height, "left": "260px", 'border': '1px solid green', 'box-shadow': '1px 1px 1px 1px #888888' });
    ftvx.fadeIn("fast");
    refdiv.show(300);
}