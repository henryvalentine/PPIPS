$(function () {
    var xcvb = new Date();
    var year = xcvb.getFullYear();
    var month = xcvb.getMonth() + 1;
    var day = xcvb.getDate();
    $('.datepicker').datepicker({
        format: 'yyyy/mm/dd',
        endDate: year + '/' + month + '/'+day,
        language: 'en',
        //startDate:"1914-01-31"
        //weekStart: 1,
        //todayBtn: 1,
        autoclose: 1,
        //todayHighlight: 1,
        //startView: 1,
        //minView: 0,
        //maxView: 1,
        //forceParse: 0
       
    });
    
    $('.futureDatepicker').datepicker({
        format: 'yyyy/mm/dd',
        startDate: year + '/' + month + '/' + day,
        language: 'en',
        //startDate:"1914-01-31"
        //weekStart: 1,
        //todayBtn: 1,
        autoclose: 1,
        //todayHighlight: 1,
        //startView: 1,
        //minView: 0,
        //maxView: 1,
        //forceParse: 0

    });

    var year2 = xcvb.getFullYear() - 18;
    var month2 = 12;
    var day2 = 31;

    $('.birthdatepicker').datepicker({
        format: 'yyyy/mm/dd',
        endDate: year2 + '/' + month2 + '/' + day2,
        language: 'en',
        //startDate:"1914-01-31"
        //weekStart: 1,
        //todayBtn: 1,
        autoclose: 1,
        //todayHighlight: 1,
        //startView: 1,
        //minView: 0,
        //maxView: 1,
        //forceParse: 0

    });
    
    $('.effectiveDate').datepicker({
        format: 'yyyy/mm/dd',
        language: 'en',
        startDate: year + '/' + month + '/' + day,
        //weekStart: 1,
        //todayBtn: 1,
        autoclose: 1,
        //todayHighlight: 1,
        //startView: 1,
        //minView: 0,
        //maxView: 1,
        //forceParse: 0

    });
});