(function ($) {
    $(document).ready(function () {
        $('.datepicker').daterangepicker({
            singleDatePicker: true,
            calender_style: "picker_2",
            maxDate: new Date(),
            ////==========
            showDropdowns: true,
            locale: {
                daysOfWeek: _daysOfWeek,
                monthNames: _monthNames,
                //daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                //monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']
            }
        });
        $(".select2_single").select2({
            placeholder: _SState,
            allowClear: true
        });
        //===========
        $("#chkDate").attr('checked', false);
        $("#divBusDate").hide();

        //===========
        //$("#storeDdl").select2({
        //    placeholder: _SStore,
        //    allowClear: true
        //});
        //$("#companyDdl").select2({
        //    placeholder: _SCompany,
        //    allowClear: true
        //});
        //$("#ddlSeleted").val(1);
        //$("#divStore").hide();
        $("#ddlSeleted").val(_typeState);
        if (_typeState === 1) {
            $("#divStore").hide();
            if (_listCompanys.length > 0) {
                var data = JSON.parse(_listCompanys);
                $("#companyDdl").val(data);
            }
        }
        else {
            $("#divCompany").hide();
            if (_listStores.length > 0) {
                var data = JSON.parse(_listStores);
                $("#storeDdl").val(data);
            }
        }

        $("#storeDdl").select2({
            placeholder: _SStore,
            allowClear: true
        });
        $("#companyDdl").select2({
            placeholder: _SCompany,
            allowClear: true
        });

    });

    $('#chkDate').click(function () {
        if (this.checked) {
            $("#divDefDate").hide();
            $("#divBusDate").show();
        } else {
            $("#divDefDate").show();
            $("#divBusDate").hide();
        }
    });

    $("#ddlSeleted").change(function () {
        var val = this.value;
        if (val == 1) { //Compay
            $("#divCompany").show();
            $("#divStore").hide();
            $('#storeDdl').select2('val', null)
        } else if (val == 2) {//Store
            $("#divStore").show();
            $("#divCompany").hide();
            $('#companyDdl').select2('val', null)
        }
        $(".validation-summary-errors").empty();
        $(".validateLevel").empty();
    });

    $("#btn-Export").on('click', function () {
        $(".validation-summary-errors").empty();
        $(".validateLevel").empty();
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
                $('.validateLevel').html(_SCompany);
                return false;
            }
            $(".validateEmp").empty();
            return true;
        } else if (type == 2) {//Store
            var storeId = $('#storeDdl').val();
            if (storeId == null) {
                $('.validateLevel').html(_SStore);
                return false;
            }
            $(".validateEmp").empty();
            return true;
        }
    });

})(jQuery);

function BackToLoginWhenSessionEnd(data) {
    var isloginForm = false;
    var res = $(data);
    if (res != null && res.length > 11 && res[11].childNodes.length >0) {
        var text = res[11].childNodes[0].data;
        //alert(text);
        if (text != null && text == "NüWeb F&B - Login") {
            //window.location = "/Login/Index";
            isloginForm = true;
        }
    }
    return isloginForm;
}