var checkAll = false;

/* Toogle check all checkboxes from table or div with given element selector, ex: "#divID", ".tableClass" */
function ToogleCheckAll(e, containElementSelector) {
    checkAll = $(e).prop("checked");
    $(containElementSelector).find("input[type='checkbox']").prop("checked", checkAll);
    if ($(e).prop('id') != 'select-all')
        ToggleBtnDelete();
}

/* Toogle check all checkboxes from table or div with given element selector, ex: "#divID", ".tableClass" */
function IndexCheckAll(containElementSelector, e) {
    if ($(e).hasClass("check-all")) {
        $(containElementSelector).find("input[type='checkbox']").prop("checked", true);
        $(e).removeClass("check-all").addClass("uncheck-all");
    }
    else if ($(e).hasClass("uncheck-all")) {
        $(containElementSelector).find("input[type='checkbox']").prop("checked", false);
        $(e).removeClass("uncheck-all").addClass("check-all");
    }
    ToggleBtnDelete();
}

function ToggleBtnDelete() {
    //if ($(".gridview tbody input[type='checkbox']:checked").length > 0) {
    //    $("#btn-delete").removeClass('disabled');
    //    $("#btn-actives").removeClass('disabled');
    //} else {
    //    $("#btn-delete").addClass('disabled');
    //    $("#btn-actives").addClass('disabled');
    //}

    var totalRow = $(".gridview tbody input[type='checkbox']").length;
    var totalChecked = $(".gridview tbody input[type='checkbox']:checked").length;

    if (totalRow == totalChecked) {
        $("#chb-checkall").prop('checked', true);
    } else {
        $("#chb-checkall").prop('checked', false);

    }
}

function checkItem() {
    var countCheck = $('.employee-items').find("input[type='checkbox']").length;
    var index = 0;
    for (var i = 0; i < countCheck; i++) {
        var item = $('.employee-items').find("input[id='ListEmployees_" + i + "__Checked']")
        if (item.prop('checked')) {
            index++;
        }
    }
    if (index == countCheck) {
        $("#checkAllEmp").prop('checked', true);
    }
    else {
        $("#checkAllEmp").prop('checked', false);
    }
}

function include(scriptUrl) {
    document.write('<script src="' + scriptUrl + '"></script>');
}
