function openCompanyConfig(id, cp_company, cp_code_company,image, cp_text_in_qr_code,cp_product_code, cp_text_in_active, cp_text_in_location, cp_text_in_point, cp_waranty_text, cp_waranty_year, cp_waranty_link_web,cp_product_date) {
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
                    $("#cp_product_code").val(obj.product_code);
                    $("#cp_product_date").val(obj.product_date);
                    $("#cp_text_in_active").val(obj.text_in_active);
                    $("#cp_text_in_location").val(obj.text_in_location);
                    $("#cp_text_in_point").val(obj.text_in_point);
                    //$("#cp_waranty_text").val(obj.waranty_text);
                    $('#image').val(obj.image);
                    $('#img_div_image').find('img').attr('src', obj.image);
                    $("#cp_waranty_year").val(obj.waranty_year);
                    $("#cp_waranty_link_web").val(obj.waranty_link_web);
                    
                    CKEDITOR.instances['cp_buy_more'].setData(obj.buy_more);
                    CKEDITOR.instances['cp_product_info'].setData(obj.product_info);
                    CKEDITOR.instances['cp_waranty_text'].setData(obj.waranty_text);
                    if (obj.is_waranty == 1) {
                        document.getElementById("cp_is_waranty").checked = true;
                    } else {
                        document.getElementById("cp_is_waranty").checked = false;
                    }
                    $("#CompanyConfigDialog").show();
                } else {
                    //alert(rs);
                }
            }
        });
    } else {
        $("#cp_ID").val(0);
        $("#cp_company").val("");
        $("#image").val("");
        $("#prd_image").attr("src", "");
        $("#cp_code_company").val("");
        $("#cp_text_in_qr_code").val("");
        $("#cp_product_code").val("");
        $("#cp_product_date").val("");
        $("#cp_text_in_active").val("Kích hoạt thành công..., thời điểm bảo hành được tính từ {NGAYTHANG}...");
        $("#cp_text_in_location").val("...tại địa điểm {DIADIEM}...");
        $("#cp_text_in_point").val("...số điểm {DIEM}...");
        $("#cp_waranty_text").val("");
        $("#cp_waranty_year").val("1");
        $("#cp_waranty_link_web").val("");
        CKEDITOR.instances['cp_buy_more'].setData("");       
        CKEDITOR.instances['cp_product_info'].setData("");
        CKEDITOR.instances['cp_waranty_text'].setData("");
        document.getElementById("cp_is_waranty").checked = false;
        $("#CompanyConfigDialog").show();
    }
    
}

function saveCompanyConfig() {
    var sbuy_more = CKEDITOR.instances.cp_buy_more.getData();
    var swaranty_text = CKEDITOR.instances.cp_waranty_text.getData();
    var sproduct_info = CKEDITOR.instances.cp_product_info.getData();
    var is_waranty = 0;
    //alert(document.getElementById("cp_is_waranty").checked);
    if (document.getElementById("cp_is_waranty").checked==true) { is_waranty = 1;}
    //if ($("#cp_ID").val() == "0") { checkDuplicateQrCode(); }
    if ($("#cp_code_company").val() == "-1") {
        alert("Nhập doanh nghiệp!");
        return;
    }
    if ($("#cp_text_in_qr_code").val() == "") {
        alert("Nhập tên sản phẩm!");
        return;
    }
    if ($("#cp_product_code").val() == "") {
        alert("Nhập mã sản phẩm!");
        return;
    }
    if ($("#cp_product_date").val()!="" && !isDate($("#cp_product_date").val())) {
        alert("Nhập sai định dạng ngày sản xuất sản phẩm!");
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
    if (is_waranty==1 && swaranty_text == "") {
        alert("Nhập thông tin bảo hành!");
        return;
    }
    //if ($("#cp_waranty_link_web").val() == "") {
    //    alert("Nhập link website khi cần mua hàng!");
    //    return;
    //}
    //if ($("#cp_text_in_qr_code").val().indexOf("{GUID}")<0) {
    //    alert("Nhập Thông báo ở QR Code(phủ) phải tồn tại ký tự {GUID}!");
    //    return;
    //}
    if ($("#cp_text_in_active").val().indexOf("{NGAYTHANG}") < 0) {
        alert("Nhập Thông báo kích hoạt phải tồn tại ký tự {NGAYTHANG}!");
        return;
    }
    if ($("#cp_text_in_location").val().indexOf("{DIADIEM}") < 0) {
        alert("Nhập Thông báo địa điểm phải tồn tại ký tự {DIADIEM}!");
        return;
    }
    if ($("#cp_text_in_point").val().indexOf("{DIEM}") < 0) {
        alert("Nhập Thông báo tích điểm phải tồn tại ký tự {DIEM}!");
        return;
    }
    $.ajax({
        url: url_addUpdateCompanyConfig, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            ID: $("#cp_ID").val(), company: $("#cp_company").val(), code_company: $("#cp_code_company").val(), image: $("#image").val(), text_in_qr_code: $("#cp_text_in_qr_code").val(), product_code: $("#cp_product_code").val(), text_in_active: $("#cp_text_in_active").val(), text_in_location: $("#cp_text_in_location").val(), text_in_point: $("#cp_text_in_point").val()
            , waranty_year: $("#cp_waranty_year").val(), waranty_text: swaranty_text, waranty_link_web: $("#cp_waranty_link_web").val()
            , is_waranty: is_waranty, buy_more: sbuy_more, product_info: sproduct_info, product_date: $("#cp_product_date").val()
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
function isDate(val) {
    var res = val.split("/");
    if (res.length != 3) {
        return false;
    }
    for (var i = 0; i < res.length; i++) {
        if (isNaN(res[i])) return false;
    }
    return true;
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