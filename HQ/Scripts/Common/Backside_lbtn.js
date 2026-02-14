var oDepartment = {
    'Page': '網頁維護',
    'Unit': '單元維護',
    'Content': '單元內容維護',
    'News': '公告',
    'Safe': '安全停看聽',
    'Hr': '人資園地'
}; //  後台分類

var d = $.Deferred();
var color = '#FFA4A2';
var dept = window.location.href;

// 建立按鈕
var initialObject = function () {
    createUl(oDepartment);
};
///
/// 建立左側按鈕
var createUl = function (oLayout) {
    console.log(oLayout)
    var $divDepartment = $('#left_btn');
    var $table_lbtn = $('<table>', {
        'width': "210",
        'border': "0",
        'cellpadding': "0",
        'cellspacing': "0"

    }).appendTo($divDepartment);
    $table_lbtn.append($('<tr>').append($('<img>', {
        'height': "45",
        'class': oLayout === oDepartment ? 'td_lbtn_title' : 'tb_lbtn_store',
        'src': oLayout === oDepartment ? '/images/pink/d_left/u_title.gif' : '/images/pink/d_left/u_title_store.gif',
        'border': "0",
        'style': 'text-align:center;width:212px',
        'onclick': "onclick_page('HQ');"
    })))

    Object.keys(oLayout).forEach(function (val) {

        $table_lbtn.append($('<tr>').append($('<td>', {
            'height': "38",
            'id': val,
            'class': 'td_lbtn_vl_line',
            'background': "/images/skin/d_left/u_btn.gif",
        }).append($('<div>', {
            'style': "padding-left: 7px; text-align:left",
            'class': "fnt01"
        })).append($('<button>', {
            'class': "lbtn",
            'id': 'btn_' + val,
            'html': '<b>|</b > ' + oLayout[val]
        }).hover(function () {
            ChangeColor("btn_" + val)
        }, function () {
            ChangeColorOver("btn_" + val)
        }
        ).click(function () {
            onclick_page(val)
        }))))
    })
    $table_lbtn.append('<tr> <td height="12" class="td_lbtn_u_h" background="/images/pink/d_left1/u_h.gif"></td></tr >')
};
var ChangeColor = function (id) {
    $('#' + id).css('background-color', color);
    $('#' + id).css('color', 'white');
};
var ChangeColorOver = function (id) {
    if (dept.indexOf(id.substring(4)) == -1) {
        $('#' + id).css('background-color', 'white');
        $('#' + id).css('color', color);
    }
};

/*
 * 名稱:onclick_page
 * 目的:點擊後會執行相對應動作
 * 參數:strAction 動作
 * */
var onclick_page = function (strDept) {
    if (strDept == 'HQ') {
        window.location.href = window.location.origin + '/' + strDept;
    } else {
        window.location.href = window.location.origin + '/Backside/' + strDept;
    }

};

$(document).ready(function () {
    initialObject();
});
$(window).load(function () {
    $('#left_btn table tbody tr td button').mouseover().mouseout();
})