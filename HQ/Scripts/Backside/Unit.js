var fpRead = function (dept, page, subtype) {
    // 到Unit中的UnitRead取資料
    g_cus.CommonConnect(window.location.origin + "/api/Unit/UnitRead", { dept: dept, page: page, subtype: subtype }, function (res) {
        // 清空畫面上的資料
        $('.content').remove();
        if (res.Table.length > 0) {
            // 當有資料時 確保table是顯示 及input為隱藏狀態
            $('#table').show();
            $('#input').hide();
            $tPage = $('#Unit_table');
            // 建立新資料
            res.Table.forEach(function (val, index) {
                $tPage.append($('<tr>', { 'class': 'content' }).append(
                    $('<td>', { 'html': '<button class="btn" onclick=ChangeData(' + (index) + ')>修改</button><button class="btn" onclick=DeleteData(' + (index) + ')>刪除</button>' }),
                    $('<td>', { 'html': val.Page }),
                    $('<td>', { 'html': val.Dept }),
                    $('<td>', { 'html': val.subtype }),
                    $('<td>', { 'html': val.unit_title }),
                    $('<td>', { 'html': val.style }),
                    $('<td>', { 'html': val.include_file }),
                    $('<td>', { 'html': val.is_show })
                ))
            })
        }
    })

};
/* 風格的資料(目前沒使用)
 * UnitController.cs StyleSearch
 * */
var fpStyleSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/StyleSearch", {}, function (res) {
        if (res.Table.length > 0) {
            // 當有值時 放在某處 且長出下拉式選單
           
        }

    });
};
/* 部門的資料
 * UnitController.cs DeptSearch
 * */
var fpDeptSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/DeptSearch", {}, function (res) {
        if (res.Table.length > 0) {
            // 增加在DeptSelect的位置
            $tPage = $('#DeptSelect');
            res.Table.forEach(function (val, index) {
                $tPage.append(
                    $('<option>', { 'value': val.dept, 'html': val.dept })
                )
            })
        }

    });
};
/* 頁數的資料
 * UnitController.cs PageSearch
 * */
var fpPageSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/PageSearch", {}, function (res) {
        if (res.Table.length > 0) {
            // 增加在PageSelect的位置
            $tPage = $('#PageSelect');
            res.Table.forEach(function (val, index) {
                $tPage.append(
                    $('<option>', { 'value': val.page, 'html': val.page })
                )
            })
        }

    });
};
/* 單元的資料
 * UnitController.cs SubtypeSearch
 * */
var fpSubtypeSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/SubtypeSearch", {}, function (res) {
        if (res.Table.length > 0) {
            // 增加在SubtypeSelect的位置
            $tPage = $('#SubtypeSelect');
            res.Table.forEach(function (val, index) {
                $tPage.append(
                    $('<option>', { 'value': val.subtype, 'html': val.subtype })
                )
            })
        }
    });
};
/*
 * 更新資料
 * UnitController.cs UnitUpdate
 * */
var fpUpdate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/UnitUpdate", {
        page: $("#input_page").val(), dept: $("#input_dept").val(), subtype: $("#input_subtype").val(), unit_title: $("#input_unit_title").val(),
        style: $("#input_style").val(), include_file: $("#input_include_file").val(), is_show: $("#input_show").val()
    }, function (res) {
        fpShowAlert(res, "更新");
    }, function () {
    });
};
/*
 * 新增資料
 * UnitController.cs UnitCreate
 * */
var fpCreate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/UnitCreate", {
        page: $("#input_page").val(), dept: $("#input_dept").val(), subtype: $("#input_subtype").val(), unit_title: $("#input_unit_title").val(),
        style: $("#input_style").val(), include_file: $("#input_include_file").val(), is_show: $("#input_show").val()
    }, function (res) {
        fpShowAlert(res, "新增");
    }, function () {
    });
};
/*
 * 刪除資料
 * UnitController.cs UnitDelete
 * */
var fpDelete = function (page, dept, subtype) {
    g_cus.CommonConnect(window.location.origin + "/api/Unit/UnitDelete", {
        page: page, dept: dept, subtype: subtype
    }, function (res) {
        fpShowAlert(res, "刪除");
    }, function () {
    });
};

// 當按下修改按鈕時
var ChangeData = function (num) {
    $('#table').hide();
    $('#input').show().empty();
    var tr_content = $('#Unit_table tr').eq(num)
    $('#input').append(
        $('<p>', { 'html': '網頁編號:' }).append($('<input>', { 'id': 'input_page', 'value': tr_content.find('td').eq(1).html() })),
        $('<p>', { 'html': '單元部門:' }).append($('<input>', { 'id': 'input_dept', 'value': tr_content.find('td').eq(2).html() })),
        $('<p>', { 'html': '單元類型:' }).append($('<input>', { 'id': 'input_subtype', 'value': tr_content.find('td').eq(3).html() })),
        $('<p>', { 'html': '單元標題:' }).append($('<input>', { 'id': 'input_unit_title', 'value': tr_content.find('td').eq(4).html() })),
        $('<p>', { 'html': '單元風格:' }).append($('<input>', { 'id': 'input_style', 'value': tr_content.find('td').eq(5).html() })),
        $('<p>', { 'html': '單元連結:' }).append($('<input>', { 'id': 'input_include_file', 'value': tr_content.find('td').eq(6).html() })),
        $('<p>', { 'html': '單元顯示:' }).append($('<input>', { 'id': 'input_show', 'value': tr_content.find('td').eq(7).html() })),
        $('<input>', { 'id': '', 'class': "btn", 'type': 'button', 'value': '確定' }).click(function () { fpUpdate() }),
        $('<input>', { 'id': '', 'class': "btn", 'type': 'button', 'value': '取消' }).click(function () { $('#input').hide(); $('#table').show(); })
    );
};
// 當按下新增按鈕時
var cInsert = function () {
    $('#table').hide();
    $('#input').show().empty();
    $('#input').append(
        $('<p>', { 'html': '網頁編號:' }).append($('<input>', { 'id': 'input_page' })),
        $('<p>', { 'html': '單元部門:' }).append($('<input>', { 'id': 'input_dept' })),
        $('<p>', { 'html': '單元類型:' }).append($('<input>', { 'id': 'input_subtype' })),
        $('<p>', { 'html': '單元標題:' }).append($('<input>', { 'id': 'input_unit_title' })),
        $('<p>', { 'html': '單元風格:' }).append($('<input>', { 'id': 'input_style' })),
        $('<p>', { 'html': '單元連結:' }).append($('<input>', { 'id': 'input_include_file' })),
        $('<p>', { 'html': '單元顯示:' }).append($('<input>', { 'id': 'input_show' })),
        $('<input>', { 'id': '', 'class': "btn", 'type': 'button', 'value': '確定' }).click(function () { fpCreate() }),
        $('<input>', { 'id': '', 'class': "btn", 'type': 'button', 'value': '取消' }).click(function () { $('#input').hide(); $('#table').show(); })
    );
};
// 當按下刪除時
var DeleteData = function (num) {
    var tr_content = $('#Unit_table tr').eq(num);
    fpDelete(tr_content.find('td').eq(1).html(), tr_content.find('td').eq(2).html(), tr_content.find('td').eq(3).html())
};

var fpShowAlert = function (iSuccessCount, Action) {
    alert((iSuccessCount > 0) ? Action + "執行成功" : Action + "執行失敗");
    fpRead();
};

$(document).ready(function () {
    fpRead();
    fpDeptSearch();
    fpPageSearch();
    fpSubtypeSearch();
    // 當下拉式選單改變資料時
    $('#DeptSelect').change(function () {
        fpRead($(this).val(), $('#PageSelect').val(), $('#SubtypeSelect').val());
    })
    $('#PageSelect').change(function () {
        fpRead($('#DeptSelect').val(), $(this).val(), $('#SubtypeSelect').val());
    })
    $('#SubtypeSelect').change(function () {
        fpRead($('#DeptSelect').val(), $('#PageSelect').val(), $(this).val());
    })
})