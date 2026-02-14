var id = 0;
// 初始化編輯器
var editor = null;
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
 * 讀資料
 * NewsController.cs NewsRead
 * */
var fpRead = function () {
    g_cus.CommonConnect(window.location.origin + "/api/News/NewsRead", { type: 'msg2' }, function (res) {
        if (res.Table.length > 0) {
            $('#table').show();
            $('#input').hide();
            $('.content').remove();
            $tPage = $('#News_table');
            res.Table.forEach(function (val, index) {
                $tPage.append($('<tr>', { 'class': 'content' }).append(
                    $('<td>', { 'html': '<button class="btn" onclick=ChangeData(' + (index) + ')>修改</button><button class="btn" onclick=DeleteData(' + (index) + ')>刪除</button>' }),
                    $('<td>', { 'html': val.background }),
                    $('<td>', { 'html': val.dept }),
                    $('<td>', { 'html': val.priority }),
                    $('<td>', { 'html': val.type }),
                    $('<td>', { 'html': val.back_type }),
                    $('<td>', { 'html': val.urlpath, 'class': 'ellipsis' }).hide(),
                    $('<td>', { 'html': '<div>' + val.descpt + '</div>' }).hide(),
                    $('<td>', { 'html': val.s }),
                    $('<td>', { 'html': val.e }),
                    $('<td>', { 'html': val.des_no }).hide()
                ))
            });
        }
    });

};
/*
 * 更新資料
 * NewsController.cs NewsUpdate
 * */
var fpUpdate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/News/NewsUpdate", {
        dept: $("#input_dept").val(), type: $("#input_type").val(), background: $("#input_background").val(), urlpath: $("#input_urlpath").val(),
        priority: $("#input_priority").val(), descpt: editor.getContent(), start_date: $("#input_start_date").val(), end_date: $("#input_end_date").val(), des_no: id
    }, function (res) {
        fpShowAlert(res, "Update");
    }, function () {
    });
};
/*
 * 新增資料
 * NewsController.cs NewsCreate
 * */
var fpCreate = function () {
    g_cus.CommonConnect(window.location.origin + "/api/News/NewsCreate", {
        dept: $("#input_dept").val(), type: $("#input_type").val(), background: $("#input_background").val(), urlpath: $("#input_urlpath").val(),
        priority: $("#input_priority").val(), descpt: editor.getContent(), start_date: $("#input_start_date").val(), end_date: $("#input_end_date").val(), des_no: id
    }, function (res) {
        fpShowAlert(res, "Create");
    }, function () {
    });
};
/*
 * 刪除資料
 * NewsController.cs NewsDelete
 * */
var fpDelete = function (dept) {
    g_cus.CommonConnect(window.location.origin + "/api/News/NewsDelete", {
        des_no: dept
    }, function (res) {
        fpShowAlert(res, "Delete");
    }, function () {
    });
};
// 按下更新按鈕時
var ChangeData = function (num) {
    console.log('ChangeData:::');
    $('#table').hide();
    $('#input').show().empty();
    var tr_content = $('#News_table tr.content').eq(num).find('td');
    console.log('tr_content.length:::' + tr_content.length);
    id = tr_content.eq(tr_content.length - 1).html();
    Ueditor();  // 初始化編輯器
    $('#input').append(
        $('<p>', { 'html': '類型:' }).append($('<select>', { 'id': 'input_type', 'value': tr_content.eq(4).html() })),
        $('<p>', { 'html': '主題區名稱:' }).append($('<select>', { 'id': 'input_dept', 'value': tr_content.eq(2).html() })),        
        $('<p>', { 'html': '文章名稱:' }).append($('<input>', { 'id': 'input_background', 'value': tr_content.eq(1).html() })),
        $('<p>', { 'html': '超連結:' }).append($('<input>', { 'id': 'input_urlpath', 'value': tr_content.eq(6).html() })).hide(),
        $('<p>', { 'html': '優先權:' }).append($('<input>', { 'id': 'input_priority', 'value': tr_content.eq(3).html() })),
        $('<p>', { 'html': '內容:' }).append($('<div>', { 'id': 'editor', 'name': 'editor' })),
        $('<p>', { 'html': '開始時間:' }).append($('<input>', { 'id': 'input_start_date', 'value': tr_content.eq(tr_content.length-3).html() })),
        $('<p>', { 'html': '結束時間:' }).append($('<input>', { 'id': 'input_end_date', 'value': tr_content.eq(tr_content.length-2).html() })),
        $('<input>', { 'id': '', 'type': 'button', 'value': '確定', 'class': "btn" }).click(function () { fpUpdate() }),
        $('<input>', { 'id': '', 'type': 'button', 'value': '取消', 'class': "btn" }).click(function () { $('#input').hide(); $('#table').show(); })
    )

    var newOptions = {
        "政策推動": "政策推動",
        "熱門話題": "熱門話題",
        "職場生活": "職場生活",
        "快樂員購": "快樂員購"
    };

    var $el = $("#input_dept");
    $el.empty(); // remove old options
    $.each(newOptions, function (key, value) {
        $el.append($("<option></option>")
            .attr("value", value).text(key));
    });

    //指定下拉選單值
    $('#input_dept option').filter('[value="' + tr_content.eq(2).html() + '"]').attr("selected", true);
    $('#input_dept').attr('disabled', true);

    var newOptions2 = {
        "人資園地": "msg2"
    };

    var $el = $("#input_type");
    $el.empty(); // remove old options
    $.each(newOptions2, function (key, value) {
        $el.append($("<option></option>")
            .attr("value", value).text(key));
    });   

    editor.render('editor');
    editor.ready(function () {
        editor.setContent(tr_content.eq(7).find('div').html());
    })
};
// 按下新增按鈕時
var cInsert = function () {
    console.log('cInsert:::');
    $('#table').hide();
    $('#input').show().empty();
    Ueditor();  // 初始化編輯器
    $('#input').append(
        $('<p>', { 'html': '類型:' }).append($('<select>', { 'id': 'input_type', 'readonly': 'readonly', 'value': 'msg2' })),
        $('<p>', { 'html': '主題區名稱:' }).append($('<select>', { 'id': 'input_dept' })),        
        $('<p>', { 'html': '文章名稱:' }).append($('<input>', { 'id': 'input_background' })),
        $('<p>', { 'html': '超連結:' }).append($('<input>', { 'id': 'input_urlpath' })).hide(),
        $('<p>', { 'html': '優先權:' }).append($('<input>', { 'id': 'input_priority' })),
        $('<p>', { 'html': '內容:' }).append($('<textarea>', { 'id': 'editor' })),
        $('<p>', { 'html': '開始時間:' }).append($('<input>', { 'id': 'input_start_date', 'value': '2019/01/01' })),
        $('<p>', { 'html': '結束時間:' }).append($('<input>', { 'id': 'input_end_date', 'value': '2099/01/01' })),
        $('<input>', { 'id': '', 'type': 'button', 'value': '確定', 'class': "btn" }).click(function () { fpCreate() }),
        $('<input>', { 'id': '', 'type': 'button', 'value': '取消', 'class': "btn" }).click(function () { $('#input').hide(); $('#table').show(); })
    )
   
    var newOptions = {
        "政策推動": "政策推動",
        "熱門話題": "熱門話題",
        "職場生活": "職場生活",
        "快樂員購": "快樂員購"
    };

    var $el = $("#input_dept");
    $el.empty(); // remove old options
    $.each(newOptions, function (key, value) {
        $el.append($("<option></option>")
            .attr("value", value).text(key));
    });

    var newOptions2 = {
        "人資園地": "msg2"
    };

    var $el = $("#input_type");
    $el.empty(); // remove old options
    $.each(newOptions2, function (key, value) {
        $el.append($("<option></option>")
            .attr("value", value).text(key));
    });
   

    editor.render('editor');
};
// 按下刪除按鈕時
var DeleteData = function (num) {
    //20221208之前原始寫法(因為巢狀表格無法適用而改寫)
    //var tr_content = $('#News_table tr').eq(num);
    //fpDelete(tr_content.find('td').eq(10).html())
    console.log(num);
    var tr_content = $('#News_table tr.content').eq(num);
    console.log(tr_content);
    var td_content = tr_content.find('td');
    console.log(td_content);
    console.log('tr_content.length:::' + tr_content.length);
    console.log('td_content.length:::' + td_content.length);
    console.log('0::' + tr_content.find('td').eq(0).html());
    console.log('1::' + tr_content.find('td').eq(1).html());
    console.log('2::' + tr_content.find('td').eq(td_content.length - 1).html());
    console.log('3::' + tr_content.find('td').eq(td_content.length).html());
    fpDelete(tr_content.find('td').eq(td_content.length - 1).html())
};
// 當執行成功時 跳出確認視窗
var fpShowAlert = function (iSuccessCount, Action) {
    alert((iSuccessCount > 0) ? Action + "執行成功" : Action + "執行失敗");
    fpRead();
};


$(document).ready(function () {
    fpRead();
})