$("#storeDdl").change(function (e) {
    $("#employee-grid").empty();
});

$("#btn-load-data").on('click', function () {
    ResetValue();

    var listData = null;
    var type = $("#ddlSeleted").val();
    if (type == 1)//Company
    {
        listData = $('#companyDdl').val();
        if (listData == null) {
            //$('.validateLevel').html('Please choose Company');
            $('.validateLevel').html(_SCompany);
            return false;
        }
    } else if (type == 2) {//Store
        listData = $('#storeDdl').val();
        if (listData == null) {
            //$('.validateLevel').html('Please choose store');
            $('.validateLevel').html(_SStore);
            return false;
        }
    }
    if (listData != null) {
        $.ajax({
            url: BaseUrl + 'BaseReport/LoadEmployee',
            data: { listData: listData, Type: type },
            type: "post",
            traditional: true,
            dataType: "html",
            beforeSend: function () {
                $("#loadingSave").show();
            },
            success: function (data) {
                $("#employee-grid").html(data);
                $("#loadingSave").hide();
                if (data === "") {
                    //$('.validateEmp').html('Employees is Null.');
                    $('.validateEmp').html(_SEmployee);
                }
            }
        });
    }
    return false;
});

$("#btn-Export").on('click', function () {
    ResetValue();

    //var _storeId = $('#storeDdl').val();
    //if (_storeId == null) {
    //    $('.validateLevel').html('Please choose store');
    //    return false;
    //}
    //======Check validation For Date
    var _valFromDate = $("#fromDate").val();
    var _valToDate = $("#toDate").val();
    if (_valFromDate == '') {
        $('.validateLevel').html(_SfromDate);
        return false;
    }
    if (_valToDate == '') {
        $('.validateLevel').html(_StoDate);
        return false;
    }
    //====== End Check validation For Date

    var type = $("#ddlSeleted").val();
    if (type == 1)//Company
    {
        var companyId = $('#companyDdl').val();
        if (companyId == null) {
            //$('.validateLevel').html('Please choose Company');
            $('.validateLevel').html(_SCompany);
            return false;
        }
    } else if (type == 2) {//Store
        var storeId = $('#storeDdl').val();
        if (storeId == null) {
            //$('.validateLevel').html('Please choose store');
            $('.validateLevel').html(_SStore);
            return false;
        }
    }

    //===================
    //if ($('#employee-grid').html() === "") {
    //    $('.validateEmp').html('Employees is Null.');
    //    return false;
    //}

    var selectedEmp = [];
    $('#employee-grid tbody input:checked').each(function () {
        selectedEmp.push($(this).parent().find("input[name*='Index']").val());
    });
    //if (selectedEmp.length == 0) {
    //    $('.validateEmp').html('Please choose employee.');
    //    return false;
    //}
    $(".validateEmp").empty();
    return true;
});

function ResetValue() {
    $(".validation-summary-errors").empty();
    $(".validateLevel").empty();
    $('.validateEmp').empty();
}

$("#ddlSeleted").change(function () {
    var val = this.value;
    if (val == 1) { //Compay
        $("#divCompany").show();
        $("#divStore").hide();
        $('#storeDdl').select2('val', null);
    } else if (val == 2) {//Store
        $("#divStore").show();
        $("#divCompany").hide();
        $('#companyDdl').select2('val', null);
    }
    $(".validation-summary-errors").empty();
    $(".validateLevel").empty();
});