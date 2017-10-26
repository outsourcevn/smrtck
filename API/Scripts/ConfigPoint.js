function openConfig(id, check, share, ref, time) {

    $("#cp_ID").val(id);
    $("#cp_check_point").val(check);
    $("#cp_share_point").val(share);
    $("#cp_ref_point").val(ref);
    $("#cp_time_point").val(time);
    $("#ConfigDialog").show();
}
function saveConfig() {

    if ($("#cp_check_point").val() == "") {
        alert("Nhập điểm thưởng quét app!");
        return;
    }
    if ($("#cp_share_point").val() == "") {
        alert("Nhập điểm thưởng share app!");
        return;
    }
    if ($("#cp_ref_point").val() == "") {
        alert("Nhập điểm thưởng khi được giới thiệu!");
        return;
    }
    if ($("#cp_time_point").val() == "") {
        alert("Nhập điểm thưởng khi cài app lâu!");
        return;
    }
    $.ajax({
        url: url_addUpdateConfig, type: 'post',
        contentType: 'application/json',
        data: JSON.stringify({
            id: $("#cp_ID").val(), check_point: $("#cp_check_point").val(), share_point: $("#cp_share_point").val(), ref_point: $("#cp_ref_point").val(), time_point: $("#cp_time_point").val()
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