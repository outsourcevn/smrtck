function openCompany(id, name, email, phone, code, phone_contact, email_contact, modifiable, address, web,mst,des) {

    $("#cp_ID").val(id);
    $("#cp_name").val(name);   
    $("#cp_email").val(email);
    $("#cp_phone").val(phone);
    $("#cp_code").val(code);
    $("#cp_phone_contact").val(phone_contact);
    $("#cp_email_contact").val(email_contact);
    $("#cp_address").val(address);
    $("#cp_web").val(web);
    $("#cp_des").val(des);
    $("#cp_mst").val(mst);
    if (modifiable == 1) {
        document.getElementById("cp_modifiable").checked = true;
    } else {
        document.getElementById("cp_modifiable").checked = false;
    }
    $("#CompanyDialog").show();
    if (id == 0) {
        $.ajax({
            url: "/Admin/getMaxCompanyCode", type: 'post',            
            success: function (rs) {
                if (rs != "0") {
                    $("#cp_code").val(rs);
                } else {

                }
            }
        });
    }
}

function saveCompany() {
    var is_modifiable = 0;
    if (document.getElementById("cp_modifiable").checked == true) { is_modifiable = 1; }
    if ($("#cp_name").val() == "") {
        alert("Nhập tên!");
        return;
    }
    if ($("#cp_code").val() == "") {
        alert("Nhập mã doanh nghiệp!");
        return;
    }
    if ($("#cp_email").val() == "") {
        alert("Nhập email!");
        return;
    }
    if ($("#cp_phone").val() == "") {
        alert("Nhập phone!");
        return;
    }
    if ($("#cp_ID").val() == 0) {
        if ($("#cp_pass").val() == "") {
            alert("Nhập mật khẩu!");
            return;
        }
        if ($("#cp_pass").val() != $("#cp_pass2").val()) {
            alert("Mật khẩu phải giống nhau!");
            return;
        }
    }
    $.ajax({
        url: url_addUpdateCompany, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            ID: $("#cp_ID").val(), name: $("#cp_name").val(), code: $("#cp_code").val(), email: $("#cp_email").val(), phone: $("#cp_phone").val(), pass: $("#cp_pass").val(),
            modifiable: is_modifiable, phone_contact: $("#cp_phone_contact").val(), email_contact: $("#cp_email_contact").val(), address: $("#cp_address").val(),
            web: $("#cp_web").val(), des: $("#cp_des").val(), mst: $("#cp_mst").val()
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

function confirmDelCompany(cpId, Company_name) {
    $("#cp_ID").val(cpId);
    openNotification("Bạn có chắc chắn xóa " + Company_name + " ?", "deleteCompany");
}

function deleteCompany() {
    $.ajax({
        url: url_deleteCompany, type: 'post',
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
function checkDuplicateCode() {
    if ($("#cp_ID").val() != "0") return;
    $.ajax({
        url: "/Admin/checkDuplicateCode", type: 'post',
        data: { code: $("#cp_code").val() },
        success: function (rs) {
            //alert(rs);
            if (rs == "True") {
                alert("Đã có mã doanh nghiệp " + $("#cp_code").val() + " này rồi, bạn chọn mã khác là dạng số tự nhiên>0");
                document.getElementById("cp_code").focus();
            } else {
                
            }
        }
    });
}
function searchCompany() {

    window.location.href = "/Admin/Company?k=" + $("#keyword").val();

}
function confirmAdmin(id) {
    $.ajax({
        url: "/Admin/confirmAdminCompany", type: 'post',
        data: { id: id },
        success: function (rs) {
            if (rs == '1') {
                alert("Đã xác nhận quyền admin");
                location.reload();
            } else {
                alert(rs);
            }
        }
    });
}