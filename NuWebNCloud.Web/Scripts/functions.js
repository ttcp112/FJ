/** Global variables **/
var StoreID = "";
var ControllerName;
var SelectingCateID = "0";

var SelectingRoleID = "0";
var SelectingEmpID = "0";

var SelectingPrinterID = "0";
var ItemType = 0;
var listP = "";

// new function 23/07
function CreateAbsoluteUrl(actionName) {
    var getUrl = window.location;
    return BaseUrl + ControllerName + "/" + actionName;
}

//For Area in MVC
function CreateAbsoluteUrlArea(area, actionName) {
    var getUrl = window.location;
    return BaseUrl + area + "/" + ControllerName + "/" + actionName;
}

/** Functions **/
function PreviewImage(e, previewElementID) {
    var oFReader = new FileReader();
    oFReader.readAsDataURL(e.files[0]);

    oFReader.onload = function (oFREvent) {
        document.getElementById(previewElementID).src = oFREvent.target.result;
    };
};


/* Call View or Edit action with given url and show response (html) in .detail-view element */
function ShowViewOrEdit(action) {
    $.ajax({
        //async: false,
        cache: false,
        url: action,
        beforeSend: function () {
            $(".se-pre-con").show();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert(errorThrown);
            $(".se-pre-con").hide();
        },
        success: function (html) {
            $(".se-pre-con").hide();
            ShowDetail(html);
        }
    });
}

function ShowDetail(content) {
    $(".detail-view").html(content);
    $(".detail-view").show();
    $(".gridview").css("display", "none");

}

function CloseDetail() {
    $(".detail-view").hide();
    $(".gridview").css("display", "block");
}

function SubmitForm(form) {
    $(form).submit();
}

function Search() {
    var form = $(".search-form");
    $.ajax({
        url: $(form).attr('action'),
        type: 'post',
        data: $(form).serialize(),
        dataType: "html",
        beforeSend: function () {
            $(".se-pre-con").show();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            //$(".search-form .validateLevel").html('' + errorThrown);
            //$(".search-form .validateLevel").parents('.form-group').addClass('has-error');
        },
        success: function (data) {
            var isLogin = BackToLoginWhenSessionEnd(data);
            if (isLogin === true) {
                window.location = "/Login/Index";
            }
            else {
                $(".gridview").html(data);
                //$(".search-form .validateLevel").html('');
                //$(".search-form .validateLevel").parents('.form-group').removeClass('has-error');
            }
        },
        complete: function () {
            $(".se-pre-con").hide();
        }
    });
    return false;
}

function ToggleComponent(chb, component) {
    $(component).attr('readonly', !$(chb).prop('checked'));
}

function HandleKeyPress(e) {
    var key = e.keyCode || e.which;
    if (key == 13) {
        e.preventDefault();
    }
}

function ChangeCategory(ddl) {
    if ($(ddl).val() != '')
        SelectingCateID = $(ddl).val();
    else
        SelectingCateID = "0";
}

function LoadPrinter(elementDiv) {
    $(".se-pre-con").show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadPrinter"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(elementDiv).html(data);
            LoadTimeSlot();
            LoadTimeSlotPOS();
           
        }
    });
}

function LoadCategory(cateComponent) {
    $(".se-pre-con").show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadCategory"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID, itemType: ItemType, cateID: SelectingCateID },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(cateComponent).html(data);
           
            LoadSeason();
        }
    });
}

function LoadTimeSlot() {
    $(".se-pre-con").show();
    $(".timeslot").html('');
    $.ajax({
        url: CreateAbsoluteUrl("LoadTimeSlot"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(".timeslot").html(data);
            //loadServiceCharge
            LoadServiceCharge()
        }
    });
}
function LoadTimeSlotPOS() {
    $(".se-pre-con").show();
    $(".timeslot").html('');
    $.ajax({
        url: CreateAbsoluteUrl("LoadTimeSlotPOS"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(".timeslotPOS").html(data);           
          
        }
    });
}


function LoadServiceCharge() {
    $('#txtServiceCharge').val(0);
    $(".se-pre-con").show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadServiceCharge"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            var obj = $.parseJSON(data)[0];
            if (obj != undefined) {
                var value = obj.Value;
                var IsCurrency = eval(obj.IsCurrency); //False : %
                var IsIncludedOnBill = eval(obj.IsIncludedOnBill);
                $('#txtServiceCharge').val(value);
                if (!IsCurrency) {
                    $('#chbServiceCharge').attr('disabled', false);
                } else if (IsCurrency || IsIncludedOnBill) {
                    $('#chbServiceCharge').attr('disabled', true);
                }
            }

        }
    });
}

function LoadSeason() {
    $(".se-pre-con").show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadSeason"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID },
        dataType: 'json',
        success: function (lstSeason) {
            $(".se-pre-con").hide();
            $(".ddl-prices").each(function (e) {
                AddOptionForDDLSeason(this, lstSeason);
                SetSelectedSeason(this);
                //======
            });
           
            LoadPrinter('.printer');
        }
    });
}

function LoadZone(zoneComponent) {
    $(".se-pre-con").show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadZone"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(zoneComponent).html(data);
        }
    });
}

function LoadParentCategory(elementDiv, ProductTypeID) {
    $('#imgLoading').show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadParentCategory"),
        type: "post",
        traditional: true,
        data: { StoreID: StoreID, ProductTypeID: ProductTypeID },
        dataType: 'html',
        success: function (data) {
            $('#imgLoading').hide();
            $(elementDiv).html(data);
        }
    });
}

function LoadRole(Component) {
    $(".se-pre-con").show();
    var listStoreId = [];
    listStoreId.push(StoreID);
    $.ajax({
        url: CreateAbsoluteUrl("GetRoles"),
        type: "post",
        traditional: true,
        data: { listStoreId: listStoreId },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(Component).html(data);
        }
    });
}

///*===Role*/
//function ChangeRole(ddl) {
//    if ($(ddl).val() != '')
//        SelectingRoleID = $(ddl).val();
//    else
//        SelectingRoleID = "0";
//}

//function LoadRole(Component) {
//    $(".se-pre-con").show();
//    $.ajax({
//        url: CreateAbsoluteUrl("LoadRole"),
//        type: "post",
//        traditional: true,
//        data: { StoreID: StoreID, RoleID: SelectingRoleID },
//        dataType: 'html',
//        success: function (data) {
//            $(".se-pre-con").hide();
//            $(Component).html(data);
//            LoadEmployee('.employee');
//        }
//    });
//}

///*==Employee*/
//function ChangeEmployee(ddl) {
//    if ($(ddl).val() != '')
//        SelectingEmpID = $(ddl).val();
//    else
//        SelectingEmpID = "0";
//}
//function LoadEmployee(Component) {
//    $(".se-pre-con").show();
//    $.ajax({
//        url: CreateAbsoluteUrl("LoadEmployee"),
//        type: "post",
//        traditional: true,
//        data: { StoreID: StoreID, EmpID: SelectingEmpID },
//        dataType: 'html',
//        success: function (data) {
//            $(".se-pre-con").hide();
//            $(Component).html(data);
//        }
//    });
//}

function AddOptionForDDLSeason(ddl, lstSeason) {
    $(ddl).empty();
    $(ddl).append($('<option/>', {
        value: "",
        text: "-- Select --",
    }));

    for (var i = 0; i < lstSeason.length; i++) {
        $(ddl).append($('<option/>', {
            value: lstSeason[i].ID,
            text: lstSeason[i].Name + " [" + lstSeason[i].StoreName + "]"
        }));
    }
}

function SetSelectedSeason(ddl) {
    var currentSeasonID = $(ddl).parents('.form-group').find('input[name="SelectingSeason"]').val();
    if (currentSeasonID != "")
        $(ddl).find('option[value="' + currentSeasonID + '"]').attr("selected", "selected");
}

function ChangeSeason(ddl) {
    if ($(ddl).val() != "")
        $(ddl).parents('.form-group').find('input[name="SelectingSeason"]').val($(ddl).val());
}

function ResizeModal(element, h) {
    var heightElement = $(element).height() + 100;
    var heightMain = $(window).height();
    if (heightElement > heightMain) {
        $(element).css({ "overflow": "auto", "height": heightMain - h + "px" })
    }
}


function formatDate(date) {

    if (!isValidDate(date)) {
        //alert(!isValidDate(date));
        return "";
    }
    var hours = date.getHours();
    var minutes = date.getMinutes();
    //var ampm = hours >= 12 ? 'pm' : 'am';
    //hours = hours % 12;
    //hours = hours ? hours : 12; // the hour '0' should be '12'
    hours = hours < 10 ? '0' + hours : hours;
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes;// + ' ' + ampm;

    var month = (date.getMonth() + 1) < 10 ? '0' + (date.getMonth() + 1) : (date.getMonth() + 1);
    var day = date.getDate() < 10 ? '0' + date.getDate() : date.getDate();
    var strDate = day + "/" + month + "/" + date.getFullYear();
    //return date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear() + " " + strTime;
    
    return strDate + " " + strTime;
}
function isValidDate(value) {
    var dateWrapper = new Date(value);
    return !isNaN(dateWrapper.getDate());
}

function getMoney(price) {
    var a = new Number(price);
    var b = a.toFixed(2); //get 12345678.90
    a = parseInt(a); // get 12345678
    b = (b - a).toPrecision(2); //get 0.90
    b = parseFloat(b).toFixed(2); //in case we get 0.0, we pad it out to 0.00
    a = a.toLocaleString();//put in commas - IE also puts in .00, so we'll get 12,345,678.00
    //if IE (our number ends in .00)
    if (a < 1 && a.lastIndexOf('.00') == (a.length - 3)) {
        a = a.substr(0, a.length - 3); //delete the .00
    }
    return a + b.substr(1);//remove the 0 from b, then return a + b = 12,345,678.90
}

function OneClickButton(btn, status) {
    if (status) {
        $(btn).addClass('disabled');
    } else {
        $(btn).removeClass('disabled');
    }
}

function BackToLoginWhenSessionEnd(data) {
    var isloginForm = false;
    var res = $(data);
    if (res != null && res.length > 11) {
        if (res[11].childNodes != null && res[11].childNodes.length > 0) {
            var text = res[11].childNodes[0].data;
            //alert(text);
            if (text != null && text == "NüWeb F&B - Login") {
                //window.location = "/Login/Index";
                isloginForm = true;
            }
        }
    }
    return isloginForm;
}

// Updated 04192018
function LoadListStoreExtendTo(StoreId, Component) {
    $(".se-pre-con").show();
    $.ajax({
        url: CreateAbsoluteUrl("LoadListStoreExtendTo"),
        type: "post",
        traditional: true,
        data: { StoreId: StoreId },
        dataType: 'html',
        success: function (data) {
            $(".se-pre-con").hide();
            $(Component).html('');
            $(Component).html(data);
        }
    });
}