
$(function () {

    /* # Bootstrap Plugins
    ================================================== */


    //===== Add fadeIn animation to dropdown =====//

    $('.dropdown, .btn-group').on('show.bs.dropdown', function (e) {
        $(this).find('.dropdown-menu').first().stop(true, true).fadeIn(100);
    });

    //===== Add fadeOut animation to dropdown =====//

    $('.dropdown, .btn-group').on('hide.bs.dropdown', function (e) {
        $(this).find('.dropdown-menu').first().stop(true, true).fadeOut(100);
    });

    //===== Prevent dropdown from closing on click =====//

    $('.popup').click(function (e) {
        e.stopPropagation();
    });

    //debugger;

    /* # Default Layout Options
    ================================================== */

    //===== Wrapping content inside .page-content =====//

    $('.page-content').wrapInner('<div class="page-content-inner"></div>');



    //===== Applying offcanvas class =====//

    $(document).on('click', '.offcanvas', function () {
        $('body').toggleClass('offcanvas-active');
    });

    //===== Default navigation =====//
    $('.navigation').find('li.active').parents('li').addClass('active');
    $('.navigation').find('li').not('.active').has('ul').children('ul').addClass('hidden-ul');
    $('.navigation').find('li').has('ul').children('a').parent('li').addClass('has-ul');


    $(document).on('click', '.sidebar-toggle', function (e) {
        e.preventDefault();

        $('body').toggleClass('sidebar-narrow');

        if ($('body').hasClass('sidebar-narrow')) {
            $('.navigation').children('li').children('ul').css('display', '');

            $('.sidebar-content').hide().delay().queue(function () {
                $(this).show().addClass('animated fadeIn').clearQueue();
            });
        }

        else {
            $('.navigation').children('li').children('ul').css('display', 'none');
            $('.navigation').children('li.active').children('ul').css('display', 'block');

            $('.sidebar-content').hide().delay().queue(function () {
                $(this).show().addClass('animated fadeIn').clearQueue();
            });
        }
    });

    $('.navigation').find('li').children('a').on('click', function(e) {

        $('.navigation').find('li.active').removeClass('active');

        if ($(this).parent('li').children('ul').length <= 0)
            $(this).parent('li').addClass('active');

        $(this).parent('li').not('.active').has('ul').children('ul').addClass('hidden-ul');
        $(this).find('li').has('ul').children('a').parent('li').addClass('has-ul');

    });


    $('.navigation').find('li').has('ul').children('a').on('click', function (e) {

        e.preventDefault();

        if ($('body').hasClass('sidebar-narrow')) {
            $(this).parent('li > ul li').not('.disabled').toggleClass('active').children('ul').slideToggle(250);
            $(this).parent('li > ul li').not('.disabled').siblings().removeClass('active').children('ul').slideUp(250);
        }

        else {
            $(this).parent('li').not('.disabled').toggleClass('active').children('ul').slideToggle(250);
            $(this).parent('li').not('.disabled').siblings().removeClass('active').children('ul').slideUp(250);
        }
    });



    //===== Panel Options (collapsing, closing) =====//

    /* Collapsing */
    $('[data-panel=collapse]').click(function (e) {
        e.preventDefault();
        var $target = $(this).parent().parent().next('div');
        if ($target.is(':visible')) {
            $(this).children('i').removeClass('icon-arrow-up9');
            $(this).children('i').addClass('icon-arrow-down9');
        }
        else {
            $(this).children('i').removeClass('icon-arrow-down9');
            $(this).children('i').addClass('icon-arrow-up9');
        }
        $target.slideToggle(200);
    });

    /* Closing */
    $('[data-panel=close]').click(function (e) {
        e.preventDefault();
        var $panelContent = $(this).parent().parent().parent();
        $panelContent.slideUp(200).remove(200);
    });



    //===== Disabling main navigation links =====//

    $('.navigation .disabled a, .navbar-nav > .disabled > a').click(function (e) {
        e.preventDefault();
    });


});