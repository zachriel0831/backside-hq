var oDepartment = []; //  部門中文對應英文分類
var dBranch = [];   // 分店中文對應完整名稱簡稱
var oBranch = []; // 分店中文對應英文分類
var d = $.Deferred();
var color = '#FFA4A2'; // 預設顏色
var dept = null;    //預設部門為null

//
var initialObject = function () {
    // 長出左側按鈕(部門跟分店分開長)
    createUl(oDepartment);
    createUl(oBranch);
};
/// 寫在left_btn中
/// 建立左側按鈕
var createUl = function (oLayout) {
    var $divDepartment = $('#left_btn');
    // 在left_btn底下開始建立table
    var $table_lbtn = $('<table>', {
        'width': "210",
        'border': "0",
        'cellpadding': "0",
        'cellspacing': "0"

    }).appendTo($divDepartment);
    // 判斷是否為部門還是分店(圖片、class不同)
    $table_lbtn.append($('<tr>').append($('<img>', {
        'height': "45",
        'class': oLayout === oDepartment ? 'td_lbtn_title' : 'tb_lbtn_store',
        'src': oLayout === oDepartment ? '/images/pink/d_left/u_title.gif' : '/images/pink/d_left/u_title_store.gif',
        'border': "0",
        'style': 'text-align:center;width:212px;cursor:pointer',
        'onclick': "onclick_page('HQ');"
    })))
    // 建立其中各值的按鈕
    Object.keys(oLayout).forEach(function (val) {
        $table_lbtn.append($('<tr>').append($('<td>', {
            'height': "38",
            'id': val,
            'class': 'td_lbtn_vl_line',
            'background': "/images/skin/d_left/u_btn.gif",
            'html': ['<div align="left" style="padding-left: 7px;" class="fnt01"><button class="lbtn" onmouseout="ChangeColorOver(' + "'btn_" + val + "'" + ')" onmousemove="ChangeColor(' + "'btn_" + val + "'" + ')" id=btn_' + val + ' onclick="onclick_page(' + "'" + val + "'" + ')" >' + '<b>&nbsp;|</b>&nbsp;&nbsp;&nbsp;  ' + oLayout[val] + '</button ></div>'],
            'font-weight': "bold"
        })))
    })
    // 在最底下增加標底
    $table_lbtn.append('<tr> <td height="12" class="td_lbtn_u_h" background="/images/pink/d_left1/u_h.gif"></td></tr >')
};

// 滑鼠移動在按鈕上的切換顏色
ChangeColor = function (id) {
    $('#' + id).css('background-color', color);
    $('#' + id).css('color', 'white');
}
// 滑鼠移動出按鈕的切換顏色
ChangeColorOver = function (id) {
    if (id.indexOf(dept) == -1) {
        $('#' + id).css('background-color', 'white');
        $('#' + id).css('color', color);
    }

}

/// 點閱率
var CountRead = function (dept, page) {
    // 對MainController.cs中的Count 傳值(dept page)
    g_cus.CommonConnect(window.location.origin + "/api/Main/Count", { dept: dept, page: page }, function (res) {
    }, function () {
    })
};


// 點下搜尋按鈕的動作
var onSearchTel = function () {
    var location = window.location.href;
    // 如果當前頁面不在Hq時
    if (location.indexOf('Dept') === -1) {
        // 當是在HQ時 到電話那張表 並且搜尋 清掉cookie中的值
        $($('#tel').find('button').attr('id')).show().siblings('.tab-inner').hide();
        fptelSearch();
        clearAllCookie();
    } else {
        // 存search的cookie
        document.cookie = 'search=' + $('#tel_search').val() + ';Path=/';
        document.cookie = 'search=' + $('#tel_search').val();
        window.location = window.location.origin + '/Hq';
    }
}
// 取得各部門 分店的資料
var fpRead = function () {
    // 初始化陣列
    oDepartment = [];
    oBranch = [];
    dBranch = [];
    // 從RoomController.cs的Read取資料
    g_cus.CommonConnect(window.location.origin + "/api/Room/Read", {}, function (res) {
        $("tr[name='store_name']").remove();
        if (res.Table.length > 0) {
            // 當裡面有值時
            res.Table.forEach(function (val) {
                // 判斷裡面是否為str(分店名稱)
                if (val.type_name == '部門網頁') {
                    oDepartment[val.code_name] = val.data1;
                } if (val.type_name == '分店網頁') {
                    oBranch[val.code_name] = val.data1;
                    dBranch[val.code_name] = val.data2;
                }
            });
        }
    }, function () {
    })
};
// 清除cookie
clearAllCookie = function () {
    var keys = document.cookie.match(/[^ =;]+(?=\=)/g);
    if (keys) {
        for (var i = keys.length; i--;)
            document.cookie = keys[i] + '="" ;Path=/Hq"expires=' + new Date(0);
    }
}


/*
 * 名稱:onclick_page
 * 目的:點擊後會執行相對應動作
 * 參數:strAction 動作
 * */
var onclick_page = function (strDept) {
    dept = strDept;
    if (strDept === 'HQ') {
        // 如果回到首頁 
        CountRead(strDept, '1')
        window.location.href = window.location.origin + '/Hq'
    } else {
        // 如果目前頁面不在部門中
        if (window.location.href.indexOf('Dept') === -1) {
            window.location.href = window.location.origin + '/Hq/Dept?' + strDept;
            CountRead(strDept, '1000')
        } else {
            // 如果在部門中則直接切換頁面
            window.history.replaceState(null, '', '/Hq/Dept?' + strDept);
            fpTabPageContent(strDept);
            fpPageContent(strDept);
            CountRead(strDept, '1000')
        }
    }
    ChangeColor('btn_' + dept);

};
// 完成畫面時 取得資料並長出左側按鈕
$(document).ready(function () {
    fpRead();
    initialObject();
});