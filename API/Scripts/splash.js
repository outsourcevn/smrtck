function opensplash(id, welcome_text, image) {
    $("#cp_ID").val(id);
    $("#cp_welcome_text").val(welcome_text);
    $("#image").val(image);  
    $('#img_div_image').find('img').attr('src', image);
    if (id != 0) {
        $.ajax({
            url: "/Admin/getFullDesSplash?id=" + id, type: 'get',
            success: function (rs) {
                if (rs != "") {
                    $("#cp_welcome_text").val(rs);
                } else {

                }
            }
        });
    }
    $("#SplashDialog").show();
}
function savesplash() {
    //var cp_welcome_text = CKEDITOR.instances.cp_welcome_text.getData();
    if ($("#cp_welcome_text").val() == "" || $("#cp_welcome_text").val() == 0) {
        alert("Nhập lời giới thiệu chào mừng!");
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
        url: url_addUpdatesplash, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            ID: $("#cp_ID").val(), welcome_text: $("#cp_welcome_text").val(), image: $("#image").val()
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
function confirmDelsplash(cpId, splash) {
    $("#cp_ID").val(cpId);
    openNotification("Bạn có chắc chắn xóa " + splash + " ?", "deleteSplash");
}

function deleteSplash() {
    $.ajax({
        url: url_deletesplash, type: 'post',
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
function searchsplash() {
    window.location.href = "/Admin/splash?k=" + $("#keyword").val();
}