function openwinning(id,company,code_company, name, image, big_image, image1, image2, image3, money, from_date, to_date, quantity) {
    $("#cp_ID").val(id);
    $("#cp_company").val(company);
    $("#cp_code_company").val(code_company);
    $("#cp_name").val(name);
    $("#image").val(image);
    $("#big_image").val(big_image);
    $("#image1").val(image1);
    $("#image2").val(image2);
    $("#image3").val(image3);
    $('#img_div_image').find('img').attr('src', image);
    $('#img_div_big_image').find('img').attr('src', big_image);
    $('#img_div_image1').find('img').attr('src', image1);
    $('#img_div_image2').find('img').attr('src', image2);
    $('#img_div_image3').find('img').attr('src', image3);
  
    $("#cp_money").val(money);
    $("#cp_from_date").val(from_date);
    $("#cp_to_date").val(to_date);
    $("#cp_quantity").val(quantity);
    if (id != 0) {
        $.ajax({
            url: "/Admin/getFullDesWinning?id=" + id, type: 'get',
            success: function (rs) {
                if (rs != "") {
                    CKEDITOR.instances['cp_des'].setData(rs);
                } else {

                }
            }
        });
    }

    $("#WinningDialog").show();
}
function savewinning() {
    var des = CKEDITOR.instances.cp_des.getData();
    if ($("#cp_company").val() == "" || $("#cp_code_company").val() == 0) {
        alert("Nhập công ty!");
        return;
    }
    if ($("#cp_name").val() == "") {
        alert("Nhập tên!");
        return;
    }
    if ($("#cp_money").val() == "") {
        alert("Nhập số tiền!");
        return;
    }
    if ($("#cp_from_date").val() == "") {
        alert("Nhập từ ngày!");
        return;
    }
    if ($("#cp_to_date").val() == "") {
        alert("Nhập đến ngày!");
        return;
    }
    if ($("#cp_quantity").val() == "") {
        alert("Nhập số lượng!");
        return;
    }
    if (des == "") {
        alert("Nhập nội dung chi tiết!");
        return;
    }
    if ($("#image").val() == "") {
        alert("Nhập ảnh đại diện!");
        return;
    }
    if ($("#big_image").val() == "") {
        alert("Nhập ảnh cover!");
        return;
    }

    document.getElementById("btnSAVE").disabled = true;
    $.ajax({
        url: url_addUpdateWinning, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            ID: $("#cp_ID").val(), name: $("#cp_name").val(), company: $("#cp_company").val(), code_company: $("#cp_code_company").val(), money: $("#cp_money").val(), from_date: $("#cp_from_date").val(), to_date: $("#cp_to_date").val(), quantity: $("#cp_quantity").val(), image: $("#image").val(), big_image: $("#big_image").val(), image1: $("#image1").val(), image2: $("#image2").val(), image3: $("#image3").val(), des: des
        }),
        success: function (rs) {
            if (rs == '') {
                location.reload();
            } else {
                alert(rs);
                document.getElementById("btnSAVE").disabled = false;
            }
        }
    })
}
function confirmDelwinning(cpId, winning) {
    $("#cp_ID").val(cpId);
    openNotification("Bạn có chắc chắn xóa " + winning + " ?", "deletewinning");
}

function deletewinning() {
    $.ajax({
        url: url_deletewinning, type: 'post',
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
function searchwinning() {
    window.location.href = "/Admin/Winning?k=" + $("#keyword").val();
}