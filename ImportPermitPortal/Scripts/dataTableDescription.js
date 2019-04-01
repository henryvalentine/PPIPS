
function tableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, deleteMethodRecordName, addBtnWidth) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e)
    {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        
        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var deleteStr = '<a class="bankDelTx" title="Delete" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + deleteMethodRecordName + '(' + aData[0] + ')"><img src="/Images/delete.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;' + deleteStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function issueTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, deleteMethodRecordName, addBtnWidth) 
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var deleteStr = '<a class="bankDelTx" title="Delete" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + deleteMethodRecordName + '(' + aData[0] + ')"><img src="/Images/delete.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;' + deleteStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function employeeDeskTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, deleteMethodRecordName, addBtnWidth, changePassword) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var deleteStr = '<a class="bankDelTx" title="Delete" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + deleteMethodRecordName + '(' + aData[0] + ')"><img src="/Images/delete.png" /></a>';
            var chngPsswrd = '<a class="bankDelTx" title="Update User Password" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + changePassword + '(' + aData[0] + ')"><img src="/Images/changePassword.jpg" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;' + chngPsswrd + '&nbsp;' + deleteStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function appUsertableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, addBtnWidth, changePassword) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e)
    {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        
        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var chngPsswrd = '<a class="bankDelTx" title="Update User Password" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + changePassword + '(' + aData[0] + ')"><img src="/Images/changePassword.jpg" /></a>';
            var template = '<td style="width: 5%">&nbsp;' + editStr + '&nbsp;' + chngPsswrd + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function RecertiyManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, addBtnWidth) {
    
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="' + retrieveRecordMethodName + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var template = '<td style="width: 5%">' + details + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function RecertiyManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, addBtnWidth) {


    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="' + retrieveRecordMethodName + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var template = '<td style="width: 5%">' + details + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function permitTableManager($scope, $compile, tableDirective, tableOptions, retrieveRecordMethodName) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="'+ retrieveRecordMethodName +'(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var template = '<td style="width: 5%">' + details+ '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function dashBoardTableManager($scope, $compile, tableDirective, tableOptions, getMethod) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bLengthChange": true,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e)
    {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-1"><"col-md-11"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="' + getMethod + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
              

            var template = '<td style="width: 5%">' + details  + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function depotTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, addBtnWidth) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function tableManagerNoDelete($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, deleteMethodRecordName, addBtnWidth) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function productsTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, deleteMethodRecordName, addBtnWidth) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',

        "processing": true,
        "serverSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bankUserTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, addBtnWidth, changePassword) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e)
    {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language":
        {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,

        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var chngPsswrd = '<a class="bankDelTx" title="Update User Password" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + changePassword + '(' + aData[0] + ')"><img src="/Images/changePassword.jpg" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;' +chngPsswrd + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }

    });

    var tth = '<div style="margin-top: 7.3%;"><a ng-click = "bulkUpload()" class="btn-default btn" style=" float: left;margin-left : 15%">Bulk Upload</a>&nbsp;&nbsp;&nbsp;<a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bankBranchTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, addBtnWidth)
{

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,

        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }

    });
   
    var tth = '<div style="margin-top: 7.3%;"><a ng-click = "bulkUpload()" class="btn-default btn" style=" float: left;margin-left : 15%">Bulk Upload</a>&nbsp;&nbsp;&nbsp;<a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function applicationtableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bLengthChange": true,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var editStr = '';
            var continueStr = '';

            if (aData[5] === 'Paid' || aData[5] === 'Rejected' || aData[6] === 'Declined')
            {
                continueStr = '<a title="Continue Application" ng-click="continueApp(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/updatedocs.png" /></a>';
            }

            if (aData[5] === 'Pending')
            {
                editStr = '<a title="Pay" ng-click="getAppInfo(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/pay.png" /></a> &nbsp;';
            }

            details = '<a title="View Details" ng-click="getAppView(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + details + editStr + continueStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<label style="float: right;"><br/> <a ng-click="createApp()" style="float: right; width: 116%; text-align: right" class="btnAdd btn">Create Application</a><br></label>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function depotOwnertableManager($scope, $compile, tableDirective, tableOptions, getAppDetail)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bLengthChange": true,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var editStr = '';
            var continueStr = '';
            var template = '';
            if (aData[6] === 'Not_Available')
            {
                continueStr = '<a title="Provide Throughput" ng-click="'+getAppDetail+'(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/continue.png" /></a>';
                template = '<td style="width: 5%">' + continueStr + '</td>';
            }

            if (aData[6] !== 'Not_Available' && aData[6] !== 'Declined')
            {
                details = '<a title="View Details" ng-click="'+getAppDetail+'(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
                template = '<td style="width: 5%">' + details + '</td>';
            }

            if (aData[6] === 'Declined')
            {
                editStr = '<a title="Edit Throughput" ng-click="'+getAppDetail+'(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/edit.png" /></a> &nbsp;';
                template = '<td style="width: 5%">' + editStr + '</td>';
            }
            
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<label style="float: right;"><br/> <a ng-click="createApp()" style="float: right; width: 116%; text-align: right" class="btnAdd btn">Create Application</a><br></label>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function importerstableManager($scope, $compile, tableDirective, tableOptions, getAppDetail)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bLengthChange": true,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var template = '<a title="View Details" ng-click="' + getAppDetail + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
          
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });
   
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function receiptTableManager($scope, $compile, tableDirective, tableOptions, getItemAction) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="' + getItemAction +'(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + details + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function paidAppTableManager($scope, $compile, tableDirective, tableOptions, getItemAction) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="' + getItemAction + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + details + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function adminInvoiceTableManager($scope, $compile, tableDirective, tableOptions, getItemAction, generateRrr, getTransactionDetails)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];
   
    $.each(tableOptions.columnHeaders, function (i, e)
    {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="' + getItemAction + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var rrRrr = '';
            var trnsDetails = '';
            if (aData[1] !== null && aData[1].length > 0 && aData[7] === 'Pending')
            {
                trnsDetails = '<a title="Get Transaction Details" id="' + aData[1] + '" ng-click="' + getTransactionDetails + '($event)" style="cursor: pointer"><img src="/Images/continue.png" id="' + aData[1] + '"/></a>';
            }

            if ((aData[1] == null || aData[1].length < 1) && aData[7] === 'Pending')
            {
                rrRrr = '<a title="Generate RRR" ng-click="' + generateRrr + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/edit.png" /></a>';
            }
            
            var template = '<td style="width: 5%">' + details + '&nbsp;' + rrRrr + '&nbsp;' + trnsDetails + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function userNotificationTableManager($scope, $compile, tableDirective, tableOptions) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
            
            var details = '';
            var editStr = '';
            var recertifyStr = '';
            var continueStr = '';

            if (aData[7] === 'Paid')
            {
                continueStr = '<a title="Continue Notification" ng-click="continueNotification(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/updatedocs.png" /></a>';
            }
            else
            {
                if (aData[7] === 'Pending' || aData[7] === 'Declined')
                {
                    editStr = '<a title="Edit" ng-click="editNotification(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/edit.png" /></a> &nbsp;';
                }
                else
                {
                    if (aData[7] === 'Completed')
                    {
                        recertifyStr = '<a title="Recertify" ng-click="recertify(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/addNew.png" /></a> &nbsp;';
                    }
                }
            }
            
            details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var template = '<td style="width: 5%">' + details + '&nbsp;' + editStr + '&nbsp;' + continueStr + '&nbsp;' + recertifyStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<label style="float: right;"><br/> <a href="ngy.html#Notification/Notification" style="float: right; width: 112%; text-align: right" class="btnAdd btn">New Notification</a><br></label>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function adminNotificationTableManager($scope, $compile, tableDirective, tableOptions) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);


            var details = '';
            var approved = '';
            if (aData[8] === 'Approved') {
                approved = '<a title="Print Notification" ng-click="printNotification(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/printer.jpg" /></a> &nbsp;';
                details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            }
            else {
                details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            }
            
            var template = '<td style="width: 5%">' + details + '&nbsp;' + approved + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function adminRecertificationTableManager($scope, $compile, tableDirective, tableOptions) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);


            var details = '';
            var approved = '';
            if (aData[3] === 'Approved') {
                approved = '<a title="Print Notification" ng-click="printNotification(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/printer.jpg" /></a> &nbsp;';
                details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            }
            else {
                details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            }

            var template = '<td style="width: 5%">' + details + '&nbsp;' + approved + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bnkUserNotificationTableManager($scope, $compile, tableDirective, tableOptions) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
            var details = '';
            var editStr = '';
            var continueStr = '';
            
            if (aData[8] === 'Paid' || aData[8] === 'Rejected' || aData[8] === 'Declined')
            {
                editStr = '<a title="Edit" ng-click="uploadNotificationDocs(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/edit.png" /></a> &nbsp;';
                var details = '<a title="Update Documents" style="cursor: pointer"><img src="/Images/updatedocs.png" id="' + aData[1] + '" ng-click="getNotificationByReference($event)"/></a> &nbsp;';
            }

            details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + details + '&nbsp;' + editStr + '&nbsp;' + continueStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bnkAdminNotificationTableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
            var details = '<a title="View Details" ng-click="getNotificationDetails(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var template = '<td style="width: 5%">' + details + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });
   
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bnkUserUnprocessedNotificationTableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var view = '';
            if (aData[6] === 'Paid' || aData[6] === 'Declined' || aData[6] === 'Rejected')
            {
                details = '<a title="Update Documents" style="cursor: pointer"><img src="/Images/updatedocs.png" id="' + aData[0] + '" ng-click="getNotification(' + aData[0] + ')"/></a> &nbsp;';
            } else
            {
                view = '<a title="View Details" style="cursor: pointer"><img src="/Images/view.png" id="' + aData[0] + '" ng-click="viewNotification(' + aData[0] + ')"/></a> &nbsp;';
            }

            var template = '<td style="width: 5%">' + view + details + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });
   
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function employeeAssignedProcessesTableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
            //Set Employee Name
            $scope.employeeName = aData[7];
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function adminApplicationtableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '<a title="View Details" ng-click="getAppView(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">&nbsp;' + details + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function applicationsInVerificationTableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);
            var signOff = '';
            var appVerifiers = '';
            if (aData[7] === 'True' || aData[7] === 'true')
            {
                signOff = '<a title="Sign Off Application" ng-click="signOff(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/ok-16.jpg" /></a> &nbsp;';
            } else
            {
                appVerifiers = '<a title="View Application Verifiers" ng-click="getAppVerifiers(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/adminUser.png" /></a> &nbsp;';
            }
            var template = '<td style="width: 5%">' + '&nbsp;' + appVerifiers + '&nbsp;' + signOff + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function importerApplicationtableManager($scope, $compile, tableDirective, tableOptions) {
    $scope.companyName = '';
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            if ($scope.companyName == null || $scope.companyName.length < 1)
            {
                $scope.companyName = aData[7];
            }

            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var docs = '';
            details = '<a title="View Details" ng-click="getAppView(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + details + '&nbsp;' + docs + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
       
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return { 'jTable': jTable, 'companyName': $scope.companyName };
}

function bankAdminApplicationtableManager($scope, $compile, tableDirective, tableOptions)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e)
    {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex)
        {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var docs = '';
            details = '<a title="View Details" ng-click="getAppDetail(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + details + '&nbsp;' + docs + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bankUserApplicationtableManager($scope, $compile, tableDirective, tableOptions) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var docs = '';
            if (aData[7] !== 'Approved' && aData[7] !== 'Processing' && aData[7] !== 'Completed' && aData[7] !== 'Approved')
            {
                details = '<a title="Get Details" style="cursor: pointer" ><img src="/Images/updatedocs.png" id="' + aData[1] + '" ng-click="getAppByReference($event)"/></a> &nbsp;';
            }
           //var view = '<a title="View Details" style="cursor: pointer" ><img src="/Images/view.png" id="' + aData[1] + '" ng-click="viewAppByReference($event)"/></a> &nbsp;';
           var template = '<td style="width: 5%">' + details + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bnkAppHistoryTableManager($scope, $compile, tableDirective, tableOptions, getAppView) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var docs = '';
            details = '<a title="View Details" ng-click="' + getAppView + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
            var template = '<td style="width: 5%">' + details + '&nbsp;' + docs + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function AdminPermitTableManager($scope, $compile, tableDirective, tableOptions, retrieveRecordMethodName, deleteMethodRecordName) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="View Permit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/view.png" /></a>';
            var deleteStr = '<a class="bankDelTx" title="Delete" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + deleteMethodRecordName + '(' + aData[0] + ')"><img src="/Images/delete.png" /></a>';
            var template = '<td style="width: 5%">' + editStr + '&nbsp;' + deleteStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function bnkUserAppHistoryTableManager($scope, $compile, tableDirective, tableOptions, getAppView, viewDocuments)
{
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var details = '';
            var docs = '';
            if (aData[6] === 'Pending' || aData[6] === 'Declined') {
                details = '<a title="View Details" ng-click="getAppView(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/view.png" /></a> &nbsp;';
                docs = '<a title="Edit" ng-click="getAppInfo(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/edit.png" /></a> &nbsp;';
            }
            
            var template = '<td style="width: 5%">' + details + '&nbsp;' + docs + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bankApplicationtableManager($scope, $compile, tableDirective, tableOptions, viewDocuments) {
    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },

        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var docs = '<a title="Documents" ng-click="' + viewDocuments + '(' + aData[0] + ')" style="cursor: pointer"><img src="/Images/Generic(1).png" /></a> &nbsp;';

            var template = '<td style="width: 5%">' + docs + '&nbsp;</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}

function bankTableManager($scope, $compile, tableDirective, tableOptions, newRecordButtonValue, prepareTemplateMethodName, retrieveRecordMethodName, manageUserMethod, addBtnWidth) {

    var columnOptions = [{
        "sName": tableOptions.itemId,
        "bSearchable": false,
        "bSortable": false
    }];

    $.each(tableOptions.columnHeaders, function (i, e) {
        columnOptions.push({ 'sName': e });
    });

    var jTable = tableDirective.dataTable({
        dom: '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newItemLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"><"col-md-8"p>>>',
        "bServerSide": true,
        sAjaxSource: tableOptions.sourceUrl,
        "bProcessing": true,
        "language": {
            "lengthMenu": 'Items per Page<select id="pgLenghtInfo">' +
                '<option value="10">10</option>' +
                '<option value="20">20</option>' +
                '<option value="30">30</option>' +
                '<option value="40">40</option>' +
                '<option value="50">50</option>' +
                '<option value="100">100</option>' +
                '</select><br/>'
        },
        "sPaginationType": "full_numbers",
        aoColumns: columnOptions,
        fnRowCallback: function (nRow, aData, iDisplayIndex) {
            var oSettings = jTable.fnSettings();
            $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

            var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click = "' + retrieveRecordMethodName + '(' + aData[0] + ')"><img src="/Images/edit.png" /></a>';
            var manageStr = '';
            if (aData[3] == null || aData[3].length < 1)
            {
                manageStr = '<a title="Add Admin User" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + manageUserMethod + '(' + aData[0] + ')"><img src="/Images/addUser.png" /></a>';
            }
            else
            {
                manageStr = '<a title="Manage Admin User" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="' + manageUserMethod + '(' + aData[0] + ')"><img src="/Images/editUser.png" /></a>';
            }
            var template = '<td style="width: 5%">' + editStr + '&nbsp;' + manageStr + '</td>';
            var ttd = $('td:last', nRow);
            ttd.after($compile(template)($scope));
            return nRow;
        }
    });

    var tth = '<div style="margin-top: 7.5%;"><a ng-click = "' + prepareTemplateMethodName + '()" class="btnAdd btn" style="width: ' + addBtnWidth + 'px; float: right; text-align:right">' + newRecordButtonValue + '</a></div>';
    var kkh = $compile(tth)($scope);
    angular.element('#newItemLnk').append(kkh);
    $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
    $('.dataTables_length select').addClass('form-control');
    return jTable;
}


function setControlDate($scope, minDate, maxDate)
{
    $scope.todayx = function () {
        return new Date();
    };

    $scope.todayx();

    $scope.clearEndDatex = function () {
        $scope.expiryDatex = null;
    };

    // Disable weekend selection
    $scope.disabledx = function (date, mode) {
        return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
    };

    $scope.toggleEndMinx = function () {
        $scope.minEndDatex = minDate; //$scope.minDate ? null : new Date();
    };

    $scope.toggleEndMaxx = function () {
        $scope.maxEndDatex = maxDate; //$scope.minDate ? null : new Date();
    };

    $scope.toggleEndMinx();
    $scope.toggleEndMaxx();

    $scope.openEnDatex = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.endDateOpenedx = true;
    };

    $scope.endDateOptionsx =
    {
        formatYear: 'yyyy',
        startingDay: 1
    };

    $scope.endDateFormatsx = ['dd/MM/yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.endDateformatx= $scope.endDateFormatsx[0];
}

function setMaxDate($scope, minDate, maxDate)
{
    $scope.today = function () {
        return new Date();
    };

    $scope.today();

    $scope.clearEndDate = function ()
    {
        $scope.expiryDate = null;
    };

    // Disable weekend selection
    $scope.disabled = function (date, mode) {
        return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
    };

    $scope.toggleEndMin = function () {
        $scope.minEndDate = minDate; 
    };

    $scope.toggleEndMax = function () {
        $scope.maxEndDate = maxDate; 
    };

    $scope.toggleEndMin();
    $scope.toggleEndMax();

    $scope.openEnDate = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.endDateOpened = true;
    };

    $scope.endDateOptions =
    {
        formatYear: 'yyyy',
        startingDay: 1
    };

    $scope.endDateFormats = ['dd/MM/yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.endDateformat = $scope.endDateFormats[0];
}

function setMaxDateWithWeekends($scope, minDate, maxDate)
{
    $scope.today = function ()
    {
        return new Date();
    };

    $scope.today();

    $scope.clearEndDatep = function () {
        $scope.expiryDatep = null;
    };

    $scope.toggleEndMinp = function () {
        $scope.minEndDatep = minDate; //$scope.minDate ? null : new Date();
    };

    $scope.toggleEndMaxp = function () {
        $scope.maxEndDatep = maxDate; //$scope.minDate ? null : new Date();
    };

    $scope.toggleEndMinp();
    $scope.toggleEndMaxp();

    $scope.openEnDatep = function ($event)
    {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.endDateOpenedp = true;
    };

    $scope.endDateOptionsp =
    {
        formatYear: 'yyyy',
        startingDay: 1
    };

    $scope.endDateFormatsp = 'dd/MM/yyyy';
    $scope.endDateformatp = $scope.endDateFormatsp;
}

function setEndDateWithWeekends($scope, minDate, maxDate)
{
    $scope.today = function ()
    {
        return new Date();
    };

    $scope.today();

    $scope.clearEndDate = function () {
        $scope.expiryDate = null;
    };

    $scope.toggleEndMin = function () {
        $scope.minEndDate = minDate; //$scope.minDate ? null : new Date();
    };

    $scope.toggleEndMax = function () {
        $scope.maxEndDate = maxDate; //$scope.minDate ? null : new Date();
    };

    $scope.toggleEndMin();
    $scope.toggleEndMax();

    $scope.openEnDate = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.endDateOpened = true;
    };

    $scope.endDateOptions =
    {
        formatYear: 'yyyy',
        startingDay: 1
    };

    $scope.endDateFormats = 'dd/MM/yyyy';
    $scope.endDateformat = $scope.endDateFormats;
}