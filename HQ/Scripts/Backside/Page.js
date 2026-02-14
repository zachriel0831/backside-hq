
var fpRead = function (dept, page) {
    g_cus.CommonConnect(window.location.origin + "/api/Page/PageRead", { dept: dept, page: page }, function (res) {
        if (res.Table.length > 0) {
            $('#table').show();
            $('#input').hide();
            $('.content').remove();
            $tPage = $('#Page_table');
            res.Table.forEach(function (val, index) {
                $tPage.append($('<tr>', { 'class': 'content' }).append(
                    $('<td>', { 'html': '<button class="btn" onclick=ChangeData(' + (index) + ')>修改</button><button class="btn" onclick=DeleteData(' + (index) + ')>刪除</button>' }),
                    $('<td>', { 'html': val.page }),
                    $('<td>', { 'html': val.dept }),
                    $('<td>', { 'html': val.title }),
                    $('<td>', { 'html': val.url }),
                    $('<td>', { 'html': val.is_show })
                ))
            })
        }
    })

};


var fpDeptSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Page/DeptSearch", {}, function (res) {
        if (res.Table.length > 0) {

            $tPage = $('#DeptSelect');
            res.Table.forEach(function (val, index) {
                $tPage.append(
                    $('<option>', { 'value': val.dept, 'html': val.dept })
                )
            })
        }

    });
};

var fpPageSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Page/PageSearch", {}, function (res) {
        if (res.Table.length > 0) {

            $tPage = $('#PageSelect');
            res.Table.forEach(function (val, index) {
                $tPage.append(
                    $('<option>', { 'value': val.page, 'html': val.page })
                )
            })
        }

    });
};

var fpUpdate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Page/PageUpdate", {
        page: $("#input_page").val(), dept: $("#input_dept").val(), title: $("#input_title").val(), url: $("#input_url").val(), is_show: $("#input_show").val()
    }, function (res) {
        fpShowAlert(res, "Update");
    }, function () {
    });
};

var fpCreate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Page/PageCreate", {
        page: $("#input_page").val(), dept: $("#input_dept").val(), title: $("#input_title").val(), url: $("#input_url").val(), is_show: $("#input_show").val()
    }, function (res) {
        fpShowAlert(res, "Create");
    }, function () {
    });
};
var fpDelete = function (page, dept) {
    g_cus.CommonConnect(window.location.origin + "/api/Page/PageDelete", {
        page: page, dept: dept
    }, function (res) {
        fpShowAlert(res, "Delete");
    }, function () {
    });
};

var ChangeData = function (num) {
    $('#table').hide();
    $('#input').show().empty();
    var tr_content = $('#Page_table tr').eq(num)
    $('#input').append(
        $('<p>', { 'html': '網頁編號:' }).append($('<input>', { 'id': 'input_page', 'value': tr_content.find('td').eq(1).html() })),
        $('<p>', { 'html': '網頁部門:' }).append($('<input>', { 'id': 'input_dept', 'value': tr_content.find('td').eq(2).html() })),
        $('<p>', { 'html': '網頁標題:' }).append($('<input>', { 'id': 'input_title', 'value': tr_content.find('td').eq(3).html() })),
        $('<p>', { 'html': '網頁超連結:' }).append($('<input>', { 'id': 'input_url', 'value': tr_content.find('td').eq(4).html() })),
        $('<p>', { 'html': '網頁顯示:' }).append($('<input>', { 'id': 'input_show', 'value': tr_content.find('td').eq(5).html() })),
        $('<input>', { 'id': '', 'class': 'btn', 'type': 'button', 'value': '確定' }).click(function () { fpUpdate() }),
        $('<input>', { 'id': '', 'class': 'btn', 'type': 'button', 'value': '取消' }).click(function () { $('#input').hide(); $('#table').show(); })

    )
};
var cInsert = function () {
    $('#table').hide();
    $('#input').show().empty();
    $('#input').append(
        $('<p>', { 'html': '網頁編號:' }).append($('<input>', { 'id': 'input_page' })),
        $('<p>', { 'html': '網頁部門:' }).append($('<input>', { 'id': 'input_dept' })),
        $('<p>', { 'html': '網頁標題:' }).append($('<input>', { 'id': 'input_title' })),
        $('<p>', { 'html': '網頁超連結:' }).append($('<input>', { 'id': 'input_url' })),
        $('<p>', { 'html': '網頁顯示:' }).append($('<input>', { 'id': 'input_show' })),
        $('<input>', { 'id': '', 'class': 'btn', 'type': 'button', 'value': '確定' }).click(function () { fpCreate() }),
        $('<input>', { 'id': '', 'class': 'btn', 'type': 'button', 'value': '取消' }).click(function () { $('#input').hide(); $('#table').show(); })
    )
};

var DeleteData = function (num) {
    var tr_content = $('#Page_table tr').eq(num);
    fpDelete(tr_content.find('td').eq(1).html(), tr_content.find('td').eq(2).html())
};

var fpShowAlert = function (iSuccessCount, Action) {
    alert((iSuccessCount > 0) ? Action + "執行成功" : Action + "執行失敗");
    fpRead();
};


$(document).ready(function () {
    fpRead();
    fpDeptSearch();
    fpPageSearch();
    $('#DeptSelect').change(function () {
        fpRead($(this).val(), $('#PageSelect').val());
    })
    $('#PageSelect').change(function () {
        fpRead($('#DeptSelect').val(), $(this).val());
    })
})