
var subject_id = 0;
var editor = null;
// 初始化編輯器
var Ueditor = function () {
    editor = new baidu.editor.ui.Editor({
        UEDITOR_HOME_URL: '/ueditor/',//配置编辑器路径
        iframeCssUrl: '/ueditor/themes/iframe.css',//样式路径
        initialContent: '',//初始化编辑器内容
        autoHeightEnabled: true,//高度自动增长
        minFrameHeight: 500,//最小高度
        autoFloatEnabled: true,
        initialFrameWidth: 690,
        initialFrameHeight: 483
    });
}
/*
 * 查詢、取得資料
 * ContentController.cs ContentRead
 * */
var fpRead = function (dept, page, subtype) {
    g_cus.CommonConnect(window.location.origin + "/api/Content/ContentRead", { dept: dept, page: page, subtype: subtype }, function (res) {
        $('.content').remove();
        if (res.Table.length > 0) {
            $('#table').show();
            $('#input').hide();
            $tPage = $('#Content_table');
            res.Table.forEach(function (val, index) {
                $tPage.append($('<tr>', { 'class': 'content' }).append(
                    $('<td>', { 'html': '<button class="btn" onclick=ChangeData(' + (index) + ')>修改</button><button class="btn" onclick=DeleteData(' + (index) + ')>刪除</button>' }),
                    $('<td>', { 'html': val.Page }),
                    $('<td>', { 'html': val.Dept }),
                    $('<td>', { 'html': val.subtype }),
                    $('<td>', { 'html': val.subject }),
                    $('<td>', { 'html': val.url }),
                    $('<td>', { 'html': val.is_show }),
                    $('<td>', { 'html': val.subject_id }).hide(),
                    $('<td>', { 'html': val.content }).hide()
                ))
            });
        }
    })

};

/*
 * 部門下拉式選單資料
 * ContentController.cs ContentSearch
 * */
var fpDeptSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Content/DeptSearch", {}, function (res) {
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

/*
 * 頁數下拉式選單資料
 * ContentController.cs PageSearch
 * */
var fpPageSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Content/PageSearch", {}, function (res) {
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
/*
 * 單元下拉式選單資料
 * ContentController.cs SubjectSearch
 * */
var fpSubtypeSearch = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Content/SubtypeSearch", {}, function (res) {
        if (res.Table.length > 0) {

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
 * ContentController.cs ContentUpdate
 * */
var fpUpdate = function () {

    g_cus.CommonConnect(window.location.origin + "/api/Content/ContentUpdate", {
        page: $("#input_page").val(), dept: $("#input_dept").val(), subtype: $("#input_subtype").val(), subject: $("#input_subject").val(),
        url: $("#input_url").val(), content: editor.getContent(), is_show: $("#input_show").val(), subject_id: subject_id
    }, function (res) {
        fpShowAlert(res, "Update");
    }, function () {
    });
};
/*
 * 新增資料
 * ContentController.cs ContentCreate
 * */
var fpCreate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Content/ContentCreate", {
        page: $("#input_page").val(), dept: $("#input_dept").val(), subtype: $("#input_subtype").val(), subject: $("#input_subject").val(),
        url: $("#input_url").val(), content: editor.getContent(), is_show: $("#input_show").val()
    }, function (res) {
        fpShowAlert(res, "Create");
    }, function () {
    });
};
/*
 * 刪除資料
 * ContentController.cs ContentDelete
 * */
var fpDelete = function (page, dept, subtype, subject_id) {
    g_cus.CommonConnect(window.location.origin + "/api/Content/ContentDelete", {
        page: page, dept: dept, subtype: subtype, subject_id: subject_id
    }, function (res) {
        fpShowAlert(res, "Delete");
    }, function () {
    });
};

// 按下修改按鈕
var ChangeData = function (num) {
    $('#table').hide();
    $('#input').show().empty();
    var tr_content = $('#Content_table tr.content').eq(num).find('td');
    subject_id = tr_content.eq(7).html();
    Ueditor();  // 初始化編輯器
    $('#input').append(
        $('<p>', { 'html': '網頁編號:' }).append($('<input>', { 'id': 'input_page', 'value': tr_content.eq(1).html() })),
        $('<p>', { 'html': '部門:' }).append($('<input>', { 'id': 'input_dept', 'value': tr_content.eq(2).html() })),
        $('<p>', { 'html': '單元類型:' }).append($('<input>', { 'id': 'input_subtype', 'value': tr_content.eq(3).html() })),
        $('<p>', { 'html': '內容標題:' }).append($('<input>', { 'id': 'input_subject', 'value': tr_content.eq(4).html() })),
        $('<p>', { 'html': '網頁超連結:' }).append($('<input>', { 'id': 'input_url', 'value': tr_content.eq(5).html() })),
        $('<p>', { 'html': '網頁內容:' }).append($('<div>', { 'name': "editor", 'id': 'editor' })),
        $('<p>', { 'html': '是否顯示:' }).append($('<input>', { 'id': 'input_show', 'value': tr_content.eq(6).html() })),
        $('<input>', { 'type': 'button', 'value': '確定' }).click(function () { fpUpdate(); }),
        $('<input>', { 'type': 'button', 'value': '取消' }).click(function () { $('#input').hide(); $('#table').show(); })
    )
    editor.render('editor');
    editor.ready(function () {
        editor.setContent(tr_content.eq(8).html());
    })
};
// 按下新增按鈕時
var cInsert = function () {
    $('#table').hide();
    $('#input').show().empty();
    Ueditor(); //初始化編輯器
    $('#input').append(
        $('<p>', { 'html': '網頁編號:' }).append($('<input>', { 'id': 'input_page' })),
        $('<p>', { 'html': '部門:' }).append($('<input>', { 'id': 'input_dept' })),
        $('<p>', { 'html': '單元類型:' }).append($('<input>', { 'id': 'input_subtype' })),
        $('<p>', { 'html': '內容標題:' }).append($('<input>', { 'id': 'input_subject' })),
        $('<p>', { 'html': '網頁超連結:' }).append($('<input>', { 'id': 'input_url' })),
        $('<p>', { 'html': '網頁內容:' }).append($('<textarea>', { 'name': "editor", 'id': 'editor' })),
        $('<p>', { 'html': '是否顯示:' }).append($('<input>', { 'id': 'input_show' })),
        $('<input>', { 'type': 'button', 'value': '確定' }).click(function () { fpCreate() }),
        $('<input>', { 'type': 'button', 'value': '取消' }).click(function () { $('#input').hide(); $('#table').show(); })
    );
    editor.render('editor');
};
// 當按下刪除按鈕時
var DeleteData = function (num) {
    var tr_content = $('#Content_table .content').eq(num).find('td');
    fpDelete(tr_content.eq(1).html(), tr_content.eq(2).html(), tr_content.eq(3).html(), tr_content.eq(7).html())
};
// 當更新 刪除 新增成功時
var fpShowAlert = function (iSuccessCount, Action) {
    alert((iSuccessCount > 0) ? Action + "執行成功" : Action + "執行失敗");
    fpRead();
};
// 進入畫面時
$(document).ready(function () {
    fpDeptSearch();
    fpPageSearch();
    fpSubtypeSearch();
    fpRead();
    // 以下皆為下拉式選單變動時
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