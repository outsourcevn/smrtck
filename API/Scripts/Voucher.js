function openVoucher(id, name, image, big_image, image1, image2, image3, price, from_date, to_date, quantity) {
    $("#cp_ID").val(id);
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
    $("#cp_price").val(price);
    $("#cp_from_date").val(from_date);
    $("#cp_to_date").val(to_date);
    $("#cp_quantity").val(quantity);
    if (id != 0) {
        $.ajax({
            url: "/Admin/getFullDes?id="+id, type: 'get',
            success: function (rs) {
                if (rs != "") {
                    CKEDITOR.instances['cp_full_des'].setData(rs);
                } else {

                }
            }
        });
    }
    
    $("#VoucherDialog").show();
}
function saveVoucher() {
    var full_des = CKEDITOR.instances.cp_full_des.getData();
    if ($("#cp_name").val() == "") {
        alert("Nhập tên!");
        return;
    }
    if ($("#cp_price").val() == "") {
        alert("Nhập số điểm!");
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
    if (full_des == "") {
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
        url: url_addUpdateVoucher, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            ID: $("#cp_ID").val(), name: $("#cp_name").val(), price: $("#cp_price").val(), from_date: $("#cp_from_date").val(), to_date: $("#cp_to_date").val(), quantity: $("#cp_quantity").val(), image: $("#image").val(), big_image: $("#big_image").val(), image1: $("#image1").val(), image2: $("#image2").val(), image3: $("#image3").val(), full_des: full_des
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
function confirmDelVoucher(cpId, Voucher) {
    $("#cp_ID").val(cpId);
    openNotification("Bạn có chắc chắn xóa " + Voucher + " ?", "deleteVoucher");
}

function deleteVoucher() {
    $.ajax({
        url: url_deleteVoucher, type: 'post',
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