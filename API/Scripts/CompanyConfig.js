function openCompanyConfig(id, cp_company, cp_code_company, cp_text_in_qr_code, cp_text_in_active, cp_text_in_location, cp_text_in_point) {
    if (id!=0){
        $.ajax({
            url: "/Admin/GetCompanyQrCodeInfo", type: 'get',
            data: { id: id },
            success: function (rs) {
                //alert(rs);
                if (rs != "") {
                    rs = rs.replace("[", "").replace("]", "");
                    var obj = JSON.parse(rs);
                    //alert(obj.company);
                    $("#cp_ID").val(id);
                    $("#cp_company").val(obj.company);
                    $("#cp_code_company").val(obj.code_company);
                    $("#cp_text_in_qr_code").val(obj.text_in_qr_code);
                    $("#cp_text_in_active").val(obj.text_in_active);
                    $("#cp_text_in_location").val(obj.text_in_location);
                    $("#cp_text_in_point").val(obj.text_in_point);
                    $("#CompanyConfigDialog").show();
                } else {
                    //alert(rs);
                }
            }
        });
    } else {
        $("#cp_ID").val(0);
        $("#cp_company").val("Công ty...");
        $("#cp_code_company").val("");
        $("#cp_text_in_qr_code").val("Đây là....Mã sản phẩm là {GUID}");
        $("#cp_text_in_active").val("Kích hoạt thành công..., thời điểm bảo hành được tính từ {NGAYTHANG}...");
        $("#cp_text_in_location").val("...tại địa điểm {DIADIEM}...");
        $("#cp_text_in_point").val("...số điểm {DIEM}...");
        $("#CompanyConfigDialog").show();
    }
    
}

function saveCompanyConfig() {
   
    if ($("#cp_code_company").val() == "-1") {
        alert("Nhập doanh nghiệp!");
        return;
    }
    if ($("#cp_text_in_qr_code").val() == "") {
        alert("Nhập Thông báo ở QR Code(phủ)!");
        return;
    }
    if ($("#cp_text_in_active").val() == "") {
        alert("Nhập Thông báo kích hoạt!");
        return;
    }
    if ($("#cp_text_in_location").val() == "") {
        alert("Nhập Thông báo Địa Điểm!");
        return;
    }
    if ($("#cp_text_in_point").val() == "") {
        alert("Nhập Thông báo tích điểm!");
        return;
    }
    $.ajax({
        url: url_addUpdateCompanyConfig, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            ID: $("#cp_ID").val(), company: $("#cp_company").val(), code_company: $("#cp_code_company").val(), text_in_qr_code: $("#cp_text_in_qr_code").val(), text_in_active: $("#cp_text_in_active").val(), text_in_location: $("#cp_text_in_location").val(), text_in_point: $("#cp_text_in_point").val()
        }),
        success: function (rs) {
            if (rs == '') {
                location.reload();
            } else {
                alert(rs);
            }
        }
    })
}

function confirmDelCompanyConfig(cpId, CompanyConfig_name) {
    if (cpId == 1) {
        alert("Đây là cấu hình hệ thống, bạn chỉ có thể sửa chứ không được xóa");
        return;
    }
    $("#cp_ID").val(cpId);
    openNotification("Bạn có chắc chắn xóa " + CompanyConfig_name + " ?", "deleteCompanyConfig");
}

function deleteCompanyConfig() {
    $.ajax({
        url: url_deleteCompanyConfig, type: 'post',
        data: { cpId: $("#cp_ID").val() },
        success: function (rs) {
            if (rs == '') {
                location.reload();
            } else {
                alert(rs);
            }
        }
    });
}
function checkDuplicateQrCode() {
    $.ajax({
        url: "/Admin/checkDuplicateQrCode", type: 'post',
        data: { code: $("#cp_code_company").val() },
        success: function (rs) {
            //alert(rs);
            if (rs == "True") {
                alert("Đã có cấu hình cho mã doanh nghiệp " + $("#cp_code").val() + " này rồi, bạn chọn mã khác là dạng số tự nhiên>0. Hoặc tìm sửa cấu hình cho mã doanh nghiệp này");
                document.getElementById("cp_code_company").focus();
            } else {
                
            }
        }
    });
}
function searchCompanyConfig() {

    window.location.href = "/Admin/CompanyQrcodeConfig?k=" + $("#keyword").val();

}