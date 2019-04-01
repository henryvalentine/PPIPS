var bankManager = angular.module('bankManagement', ['ngResource']);

process_errors = function(rejection) {
    // Error processing code
};

bankManager.factory('bankService', ['$resource',
  function ($resource) {
      return $resource('/Bank/GetBank/:id', {
          id: '@id'
      });
  }
]);

bankManager.directive('bankTable', function () {
    return function ($scope, bankTable) {
        var oAllLinksTable = bankTable.dataTable({
            "dom": '<"row"<"#topContainer.col-md-12"<"col-md-4"l><"col-md-4"f><"#newBnkLnk.col-md-4">>>rt<"#bttmContainer.row"<"col-md-12"<"col-md-4"i><"col-md-8"p>>>',
            "bServerSide": true,
            "sAjaxSource": "/Bank/GetBankObjects",
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
            "aoColumns":
            [
                {
                    "sName": "BankId",
                    "bSearchable": false,
                    "bSortable": false
                },
                { "sName": "FullName" },
                { "sName": "ShortName" },
                { "sName": "SortCode" }
            ],
            'fnRowCallback': setRowFunctionCallback($scope)
        });

        $scope.dataTable = oAllLinksTable;
        $scope.banks = banklist;
        $('#newBnkLnk').append($('<div style="margin-top: 7.5%;"><button id="btnAddNewCompanyBank" ng-click = "prepareBankTemplate()" class="btnAdd btn" style="width: 140px; float: right; text-align:center">New Bank</button></div>'));
        $('.dataTables_filter input').addClass('form-control').attr('type', 'text').css({ 'width': '80%' });
        $('.dataTables_length select').addClass('form-control');
    };
});

var banklist = [];
function setRowFunctionCallback($scope) {
    //alert($scope.data.length);
    return function (nRow, aData, iDisplayIndex) {
        //alert('Hit');
        var oSettings = $scope.dataTable.fnSettings();
        $("td:first", nRow).html(oSettings._iDisplayStart + iDisplayIndex + 1);

        var editStr = '<a title="Edit" id="' + aData[0] + '" style="cursor: pointer" class="bankEdTx" ng-click ="getBank(' + aData[0] + ')"><img src="/Content/images/edit.png" /></a>';
        var deleteStr = '<a class="bankDelTx" title="Delete" id="trf' + aData[0] + '" style="cursor: pointer" ng-click="deleteBank(' + aData[0] + ')"><img src="/Content/images/delete.png" /></a>';

        $('td:last', nRow).after($('<td style="width: 5%">' + editStr + '&nbsp;' + deleteStr + '</td>'));
        banklist.push(aData);
        return nRow;
    };
}

bankManager.controller('manageBankCntroller', ['$scope', 'bankService', function ($scope, bankService)
  {
      $scope.examples = bankService.query();
      $scope.newbankService = new bankService();
      $scope.addbankService = function () {
          /*$scope.newbankService.$save().then(function(result) {
            $scope.examples.push(result);
          }).then(function() {
            $scope.newbankService = new bankService();
          }, process_errors);*/
          $scope.examples.push({ "notes": new Date() });
      };

      $scope.editbankService = function (example) {
          console.log('editing');
          /*idx = $scope.examples.indexOf(example);
          $scope.examples[idx].$save().then(function(result) {}, function(rejection) {
            process_errors(rejection);
            $scope.editMode = true;
          });*/
      };

      $scope.deletebankService = function (example) {
          console.log('deletion');
          $scope.examples.splice(0, 1);
          /*example.$delete().then(function() {
            idx = $scope.examples.indexOf(example);
            $scope.examples.splice(idx, 1);
          });*/
      };
      
      $scope.selectedBank = {};
      $scope.selectedBank.BankId = '';
      $scope.selectedBank.SortCode = '';
      $scope.selectedBank.ShortName = '';
      $scope.selectedBank.FullName = '';

      $scope.prepareBankTemplate = function () {
          $scope.selectedBank = {};
          $scope.selectedBank = {};
          $scope.selectedBank.BankId = '';
          $scope.selectedBank.SortCode = '';
          $scope.selectedBank.ShortName = '';
          $scope.selectedBank.FullName = '';
          setModal($('#popDiv'));
      };

      $scope.getBank = function (id) {

          if (id != undefined) {
              alert($scope.banks.length);
          }
          if (id == undefined || id == NaN || id < 1) {
              return;
          }
          for (i in $scope.banks) {
              if ($scope.banks[i].BankId == id) {
                  var resultPromise = bankService.bankEdit(id);

                  resultPromise.success(function (data) {
                      if (data.BankId > 0) {
                          $scope.selectedBank = data;
                          setModal($('#popDiv'));
                      } else {
                          alert("ERROR: Retweeted failed! ");
                      }
                  });
              }

              //popDiv
          }

      };
      //delete cluster
      $scope.remove = function (id) {
          //search cluster with given id and delete it
          for (i in $scope.clusters) {
              if ($scope.clusters[i].id == id) {
                  confirm("This Cluster will get deleted permanently");
                  $scope.clusters.splice(i, 1);
                  $scope.clust = {};
              }
          }
      };
  }]);