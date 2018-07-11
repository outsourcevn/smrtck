var err_generalMess = 'Đã có lỗi xẩy ra, vui lòng thử lại sau!';
function resetValue(obj, defaultVal) {
    if (defaultVal == undefined) defaultVal = '';
    if (obj == undefined || obj == null) return defaultVal;
    return obj;
}

function closeDDialog(dID) {
    $(dID).hide();
}

function openNotification(txt, action) {
    
    $("#lbNotification").text(txt);
    $("#notifyAction").val(action);
    $("#notificationDialog").show();
}

function notifyOK() {
    var action = $("#notifyAction").val();
    
    if (action == "deleteCustomer") {
        deleteCustomer();
    } else if (action == "deleteCompany") {
        deleteCompany();
    } else if (action == "deleteVoucher") {
        deleteVoucher();
    } else if (action == "deleteWinning") {
        deleteWinning();
    } else if (action == "deleteSplash") {
        deleteSplash();
    } else if (action == "delUser") {
        deleteUser();
    } else if (action == "deleteCompanyConfig") {
        deleteCompanyConfig();
    } else if (action == "deletePartner") {
        deletePartner();
    } else if (action == "delCarType") {
        deleteCarType();
    } else if (action == "delHireType") {
        deleteHireType();
    } else if (action == "delWhoType") {
        deleteWhoType();
    } else if (action == "delBooking") {
        deleteBooking();
    } else if (action == "delAirportWay") {
        deleteAirportWay();
    } else if (action == "delNationalDay") {
        deleteNationalDay();
    }
    
    closeDDialog("#notificationDialog");
}

// convert to JsDate
function convertJsDate(dateValue) {
    if (dateValue == null || dateValue == undefined) {
        return null;
    };
    return new Date(parseInt(dateValue.replace("/Date(", "").replace(")/", ""), 10));
}

// format date to dd/MM/yyyy (hh:mm:ss)
function formatDate(strDate, isHasTime) {
    var fullDate = new Date();
    if (strDate != undefined) {
        fullDate = new Date(strDate);
    }
    var tmpMonth = (fullDate.getMonth() + 1).toString();
    var tmpDate = fullDate.getDate().toString();
    var rs = (tmpDate.length == 2 ? tmpDate : "0" + tmpDate) + "/" + (tmpMonth.length == 2 ? tmpMonth : "0" + tmpMonth)
        + "/" + fullDate.getFullYear().toString();
    if (isHasTime != undefined && isHasTime) {
        var minutes = fullDate.getMinutes().toString();
        var hours = fullDate.getHours().toString();
        var seconds = fullDate.getSeconds().toString();
        rs += " " + (hours.length == 2 ? hours : "0" + hours) + ":" + (minutes.length == 2 ? minutes : "0" + minutes) + ":"
            + (seconds.length == 2 ? seconds : "0" + seconds);
    }
    return rs;
}

// Convert json date to js date
function convertDate(dateValue, isHasTime) {
    if (dateValue == null || dateValue == undefined)
        return null;
    return formatDate(new Date(parseInt(dateValue.replace("/Date(", "").replace(")/", ""), 10)), isHasTime);
}