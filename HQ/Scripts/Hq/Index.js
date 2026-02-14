// 紀錄hr公告的總頁數
var int_hr_page = 0;
// 記錄下十頁的值
var int_next_ten = 11;
// 紀錄上十頁的值
var int_zero = 1;
var last_banner = '/banner/Tab_Img10.gif';

/// mouseover move function
MouseChangeColor = function (id) {
    $(id).css('background-color', color);
}
/// mouseout function
MouseChangeColorOver = function (id) {
    if ($(id).attr('class').indexOf('active') == -1) {
        $(id).css('background-color', 'white');
    }
}



///
///資訊部公告(分頁)
var fpItConent = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Main/ItData", { dept: 'com' }, function (res) {
        if (res.Table.length > 0) {
            var $d_main_l1 = $("#it_content");
            var table_page = 0;
            var $table_page = null;
            var $page_tab = $('<ul>', {
                'class': "pagination"
            }).appendTo($('<nav>', {
                'aria-label': "Page navigation example"
            }).appendTo($('#tab03')))
            // 當有回傳值時
            res.Table.forEach(function (val, index) {
                // 記錄頁數(一頁十五筆資料)
                let a_page = Math.ceil((index + 1) / 15);
                // 當目前頁數小於a_page時 建立新的頁籤及區域
                if (table_page < a_page) {
                    $table_page = $('<td>', {
                        'scope': 'col',
                        'class': 'it_table_page',
                        'id': 'it_table_' + a_page,
                        'style': 'text-align:left'
                    }).appendTo($d_main_l1.append($('<tb>')));
                    table_page = a_page;
                    $page_tab.append($('<li>', {
                        'class': "page-item"
                    }).append($('<a>', {
                        'class': "page-link",
                        'accesskey': "#it_table_" + a_page,
                        'html': a_page
                    })))
                }
                // 建立區域中的資料
                $table_page.append(
                    $('<img>', {
                        'src': '/content_img/pic_2.gif',
                        'width': '16',
                        'height': '16',
                    }), $('<a>', {
                        'style': 'text-overflow:ellipsis;font-size:15px;',
                        'href': val.path,
                        'html': val.t + '(' + val.edm_name + ')',
                        'target': '_blank'
                    }).append('<p></p>'))

            });
            // 做切換分頁(頁數)時的顯示跟隱藏
            var $it_li = $('#tab03 nav ul.pagination li');
            $($it_li.eq(0).addClass('active').find('a').attr('accesskey')).siblings('.it_table_page').hide();
            // 點擊頁籤(tab)時的動作
            $it_li.click(function () {
                $($(this).find('a').attr('accesskey')).show().siblings('.it_table_page').hide();
                $(this).addClass('active').siblings('.active').removeClass('active');
            });
        }
    }, function () {
    })
};
/// 
///人資部公告(分頁) 
fpHrContent = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Main/HrData", { dept: 'hr' }, function (res) {
        if (res.Table.length > 0) {
            var $d_main_l1 = $("#hr_content");
            var table_page = 0;
            var $table_page = null;
            var $page_tab = $('<ul>', {
                'id': 'hr_page',
                'class': "pagination"
            }).appendTo($('<nav>', {
                'aria-label': "Page navigation example",
            }).appendTo($('#tab02')));
            res.Table.forEach(function (val, index) {
                // 存當前頁數
                const a_page = Math.ceil((index + 1) / 15);
                // 當頁籤數量小於當前頁數時 建立新的頁籤及區域
                if (table_page < a_page) {
                    $table_page = $('<td>', {
                        'scope': 'col',
                        'class': 'hr_table_page',
                        'id': 'hr_table_' + a_page,
                        'style': 'text-align:left'
                    }).appendTo($d_main_l1.append($('<tb>')));
                    table_page = a_page;
                    int_hr_page = a_page;
                    $page_tab.append($('<li>', {
                        'class': "page-item"
                    }).append($('<a>', {
                        'class': "page-link",
                        'accesskey': "#hr_table_" + a_page,
                        'html': a_page,
                        'onclick': 'page_click(' + a_page + ')'
                    })))
                }
                // 建立區域中的資料
                $table_page.append(
                    $('<img>', {
                        'src': '/content_img/pic_2.gif',
                        'width': '16',
                        'height': '16',
                    }), $('<a>', {
                        'style': 'text-overflow:ellipsis;font-size:15px;',
                        'href': val.path,
                        'html': val.title + '(' + val.edm_name + ')',
                        'target': '_blank'
                    }).append('<p></p>'))

            });
            // 在頁籤的最後建立下十筆的按鈕
            $page_tab.append($('<li>', {
                'class': "page-item"
            }).append($('<a>', {
                'class': "page-link",
                'accesskey': "#hr_table_ten",
                'html': '>>',
                'id': 'hr_next_ten',
                'onclick': "page_click('up')"
            })))
            var $hr_li = $('#tab02 nav ul.pagination li');
            // 如果頁籤數量大於10 將其他頁籤做隱藏
            if ($hr_li.length > 10) {
                for (i = 11; i <= $hr_li.length; i++) {
                    $("[accesskey='#hr_table_" + i + "']").hide();
                }
            }
            // 對目前所有區域(除了第一頁外)做隱藏
            $($hr_li.eq(0).addClass('active').find('a').attr('accesskey')).siblings('.hr_table_page').hide();
            // 當頁籤倍點擊時 做顯示即隱藏的動作
            $hr_li.click(function () {
                $($(this).find('a').attr('accesskey')).show().siblings('.hr_table_page').hide();
                $(this).addClass('active').siblings('.active').removeClass('active');
            });
        }
    }, function () {
    })
};


///
/// banner
fpBannerRead = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Main/BannerData", {}, function (res) {
        // 當每日頭條有東西時
        if (res.Table.length > 0) {
            var $d_main_l1 = $("#banner");
            // 當只有一筆資料時 只建立一筆
            if (res.Table.length < 2) {
                res.Table.forEach(function (val) {
                    // 當是flash時(判斷是否為.swf黨) 
                    //if (val.background.indexOf('.swf') !== -1) {
                    $d_main_l1.append($('<embed>', {
                        'src': '/ad/' + val.background,
                        'width': '590px'
                    }))
                    //} else {
                    ////     如果不是flash時 
                    //    $d_main_l1.append($('<a>', {
                    //        'html': val.descpt,
                    //        'href': val.urlpath,
                    //        'class': 'banner_class',
                    //        'id': 'banner_',
                    //        'style': 'background-image:url("/ad/' + val.background + '");width:590px;height:122px'
                    //    }))
                    //}
                });
            } else {
                // 如果大於兩筆時 建立頁籤
                var $ul = $('<ul>', {
                    'class': "tab-title",
                    'style': 'height: 26px;padding-top: 10px;'
                }).appendTo($d_main_l1)
                res.Table.forEach(function (val, index) {
                    $ul.append($('<li>', {
                        'style': 'width:91px;float:left',
                        'class': 'banner_cls',
                        'html': '<a accessKey="#banner_' + index + '"><img src="/banner/Tab_Img' + (index + 1) + '0.gif"></a>'
                    }))
                    //// 當是flash時(判斷是否為swf檔)
                    //if (val.background.indexOf('.swf') !== -1) {
                    $d_main_l1.append($('<embed>', {
                        'class': 'banner_class',
                        'src': '/ad/' + val.background,
                        'id': 'banner_' + index,
                        'href': val.urlpath,
                        'style': 'width:590px;height:122px',
                        'alt': val.descpt
                    }))
                    //} else {
                    ////     如果不是flash時 
                    //    $d_main_l1.append($('<a>', {
                    //        'html': val.descpt,
                    //        'href': val.urlpath,
                    //        'class': 'banner_class',
                    //        'id': 'banner_' + index,
                    //        'style': 'background-image:url("/ad/' + val.background + '");width:590px;height:122px'
                    //    }))
                    //}
                });
                // 對頁籤做隱藏及顯示
                $b_li = $('#banner ul.tab-title li');
                // 初始化頁籤顯示，僅顯示第一個頁籤的內容，隱藏其他內容
                $b_li.each(function (index) {
                    var img = $(this).find('img');
                    img.data('initialImage', img.attr('src')); // 存入每個頁籤的初始圖片

                    // 設定每個頁籤對應內容的顯示狀態
                    var accessKey = $(this).find('a').data('access-key');
                    if (index === 0) {
                        $(accessKey).show(); // 第一個頁籤顯示
                    } else {
                        $(accessKey).hide(); // 其他頁籤隱藏
                    }
                })

                $b_li.click(function () {
                    $($(this).find('a').attr('accessKey')).show().siblings('.banner_class').hide();
                    var a = $(this).find('a').find('img').attr('src');
                    $(this).siblings().find('a').find('img').attr('src', last_banner);

                    var $this = $(this);
                    var accessKey = $this.find('a').data('access-key');

                    // 顯示選定的頁籤內容並隱藏其他內容
                    $(accessKey).show().siblings('.banner_class').hide();

                    // 更新圖片
                    var img = $this.find('img');
                    var currentSrc = img.attr('src');

                    // 還原其他頁籤的圖片
                    $b_li.not($this).each(function () {
                        $(this).find('img').attr('src', $(this).find('img').data('initialImage'));
                    });

                    last_banner = a;
                    $(this).find('a').find('img').attr('src', a.replace('0', '1'));
                })

            }
        } else {
            // 如果沒有的話 就直接不顯示此區塊
            $("#banner").hide()
        }
    }, function () {
    })
};

///目前寫在d_main_l1的位置上
///人資部公告
fpMainLeftRead = function () {
    // 從 MainController.cs中的Read 讀取資料(帶的值為hr)
    g_cus.CommonConnect(window.location.origin + "/api/Main/Read", { dept: 'hr' }, function (res) {
        // 當裡面有值時
        if (res.Table.length > 0) {
            var $d_main_l1 = $("#d_main_l1");
            res.Table.forEach(function (val) {
                // 建立錨點<a> 在d_main_l1底下
                // <a herf="http://od-paper.skm.com.tw/' + val.path + "/wmx_edmimage/" + val.id + "/edm_content.htm" >'◎' + val.title.substring(0, 16) + '...'<a>
                var $a = $('<a>', {
                    'href': val.path,
                    'html': val.title,
                    'target': "_blank"
                }).appendTo($d_main_l1);
                $('<br>').appendTo($a);
            });
        }
    }, function () {
    })
};

///目前寫在d_main_r1的位置上
///資訊部公告
fpMainRightRead = function () {
    // 從 MainController.cs中的Read 讀取資料(帶的值為com)
    g_cus.CommonConnect(window.location.origin + "/api/Main/Read", { dept: 'com' }, function (res) {
        // 當裡面有值時
        if (res.Table.length > 0) {
            var $d_main_r1 = $("#d_main_r1");
            res.Table.forEach(function (val) {
                // 建立錨點<a> 在d_main_r1底下
                // <a herf="http://od-paper.skm.com.tw/' + val.path + "/wmx_edmimage/" + val.id + "/edm_content.htm" >'◎' + val.title.substring(13, 29) + '...'<a>
                var $a = $('<a>', {
                    'href': val.path,
                    'html': val.title,
                    'target': "_blank"
                }).appendTo($d_main_r1);
                $('<br>').appendTo($a);
            });
        }
    }, function () {
    })
};

///目前寫在web_connect的位置上
///網站連結
fpWebConnectRead = function () {
    // 從 MainController.cs中的WebConnectData 讀取資料(無帶值)
    g_cus.CommonConnect(window.location.origin + "/api/Main/WebConnectData", {}, function (res) {
        if (res.Table.length > 0) {
            // 當有值時
            var $web_connect = $("#web_connect");
            res.Table.forEach(function (val) {
                // 建立錨點<a> 在web_connect之下
                // <a herf=urlpath欄位 target = "_blank" class = "style3 fn01">descpt欄位<a>
                var $a = $('<a>', {
                    'href': val.urlpath,
                    'html': val.descpt,
                    'target': "_blank",
                    'class': "style3 fnt01"
                }).appendTo($web_connect);
                $('<br>').appendTo($a);
            });
        }
    }, function () {
    })
};

/// 目前寫在safe_stop的位置上
/// 安全停看聽
fpSafeRead = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Main/SafeRead", {}, function (res) {
        if (res.Table.length > 0) {
            var $safe = $("#safe_stop");
            var data = null;
            // 當有值時
            res.Table.forEach(function (val) {
                // 建立<tr> <td>
                var $a = $('<tr>', {}).appendTo($safe);
                if (val.dept !== data) {
                    $('<td>', {
                        'colspan': "2",
                        'class': "font16",
                        'html': '&nbsp;&nbsp;<b>【' + val.dept + '】</b>'
                    }).appendTo($a)
                }
                var $tr = $('<tr>', {}).appendTo($safe);
                $('<td>', {
                    'width': "25",
                    'class': "font13",
                    'html': '<p></p>&nbsp;&nbsp;&nbsp;&nbsp;◆'
                }).appendTo($tr);

                var title = '99';
                switch (val.dept) {
                    case '案例分享':
                        title = '1';
                        break;
                    case '最新公告':
                        title = '2';
                        break;
                    case '安全新知':
                        title = '3';
                        break;
                    default:
                        break;
                }


                $('<td>', {
                    'width': "175",
                    'class': "font13",
                    'html': '<p></p>&nbsp;<a href=http://ep.skm.com.tw/Other/HQ/Securitypage.aspx?title=' + title + '&desno=' + val.des_no + ' target=_blank >' + val.title + '</a>'
                }).appendTo($tr)
                // 部門(名稱)相同建立其他資訊跟閱讀更多連結
                if (val.dept === data) {
                    var $tr = $('<tr>', {}),
                        $td = $('<td>', {
                            'colspan': '2',
                            'class': 'font13'
                        }),
                        $div = $('<div>', {
                            'align': 'right'
                        });
                    $safe.append($tr.append($td.append($div)));

                    var titleNo = '99';
                    switch (val.dept) {
                        case '案例分享':
                            titleNo = '1';
                            break;
                        case '最新公告':
                            titleNo = '2';
                            break;
                        case '安全新知':
                            titleNo = '3';
                            break;
                        default:
                            break;
                    }

                    $div.append(
                        $('<a>', {
                            'href': 'http://ep.skm.com.tw/Other/HQ/Security.aspx?title=' + titleNo, //2022/4/25 修正安控室預設連結 http://ep.skm.com.tw/Other/HQ/Security.aspx?title=
                            'target': '_blank',
                            'class': 'font14',
                            'html': '<p></p>閱讀更多'
                        })
                    );
                }
                data = val.dept;
            });
        }
    }, function () {
    })
};

/// 目前寫在hr_home的位置上
/// 人事園地
fpHrHomeRead = function () {
    g_cus.CommonConnect(window.location.origin + "/api/Main/HrRead", {}, function (res) {
        console.log(res);
        if (res.Table.length > 0) {
            var $safe = $("#hr_home");
            var $hr_home1 = $("#hr_home1");
            var $hr_home2 = $("#hr_home2");
            var $hr_home3 = $("#hr_home3");
            var $hr_home4 = $("#hr_home4");
            // 當有值時
            res.Table.forEach(function (val) {
                console.log('val:::' + val);
                console.log('val.title:::' + val.title);
                console.log('val.dept:::' + val.dept);
                console.log('val.des_no:::' + val.des_no);
                //正式機網址
                var skmhtml = 'http://ep.skm.com.tw';
                //測試機網址
                //var skmhtml = 'http://10.90.101.33:8017';
                // 部門(名稱)相同建立其他資訊跟閱讀更多連結                             
                    var titleNo = '99';
                    switch (val.dept) {
                        case '政策推動':                           
                            titleNo = '1';
                            // 建立<tr> <td>   
                            var $tr = $('<tr Height="120">', {}).appendTo($hr_home1);                            
                            $('<td>', {
                                'width': "15",
                                'class': "font13",
                                'html': '<p></p>&nbsp;&nbsp;&nbsp;&nbsp;◆'
                            }).appendTo($tr);
                            $('<td>', {
                                'width': "100",
                                'class': "font13",
                                'html': '<p></p>&nbsp;<a href='+ skmhtml +'/Other/HQ/Hrpage.aspx?title=1&desno=' + val.des_no +' target=_blank >' + val.title + '</a>'
                            }).appendTo($tr)
                            break;
                        case '熱門話題':
                            titleNo = '2';
                            // 建立<tr> <td>                             
                            var $tr = $('<tr Height="120">', {}).appendTo($hr_home2);
                            $('<td>', {
                                'width': "15",
                                'class': "font13",
                                'html': '<p></p>&nbsp;&nbsp;&nbsp;&nbsp;◆'
                            }).appendTo($tr);
                            $('<td>', {
                                'width': "100",
                                'class': "font13",
                                'html': '<p></p>&nbsp;<a href=' + skmhtml +'/Other/HQ/Hrpage.aspx?title=2&desno=' + val.des_no + ' target=_blank >' + val.title + '</a>'
                            }).appendTo($tr)
                            break;
                        case '職場生活':
                            titleNo = '3';
                            // 建立<tr> <td>  
                            var $tr = $('<tr Height="120">', {}).appendTo($hr_home3);
                            $('<td>', {
                                'width': "15",
                                'class': "font13",
                                'html': '<p></p>&nbsp;&nbsp;&nbsp;&nbsp;◆'
                            }).appendTo($tr);
                            $('<td>', {
                                'width': "100",
                                'class': "font13",
                                'html': '<p></p>&nbsp;<a href=' + skmhtml +'/Other/HQ/Hrpage.aspx?title=3&desno=' + val.des_no + ' target=_blank >' + val.title + '</a>'
                            }).appendTo($tr)
                            break;
                        case '快樂員購':
                            titleNo = '4';
                            // 建立<tr> <td>  
                            var $tr = $('<tr Height="120">', {}).appendTo($hr_home4);
                            $('<td>', {
                                'width': "15",
                                'class': "font13",
                                'html': '<p></p>&nbsp;&nbsp;&nbsp;&nbsp;◆'
                            }).appendTo($tr);
                            $('<td>', {
                                'width': "100",
                                'class': "font13",
                                'html': '<p></p>&nbsp;<a href=' + skmhtml +'/Other/HQ/Hrpage.aspx?title=4&desno=' + val.des_no + ' target=_blank >' + val.title + '</a>'
                            }).appendTo($tr)
                            break;
                        default:
                            break;
                    }
            });
        }
    }, function () {
    })
};

// 分頁切換(上下十頁)
page_click = function (ipage) {
    // 當按下頁碼時
    if (typeof (ipage) == 'number') {
        // 當大於五頁時 將不再頁數前後區間的頁籤隱藏 顯示部分頁籤
        if (ipage > 5) {
            for (i = 0; i < int_hr_page; i++) {
                $("[accesskey='#hr_table_" + i + "']").hide();
            }
            for (i = ipage - 5; i < ipage + 5; i++) {
                $("[accesskey='#hr_table_" + i + "']").show();
            }
            int_zero = 1;
        } else {
            // 當小於五頁時 只顯示1~5筆的頁籤
            for (i = 0; i < int_hr_page; i++) {
                $("[accesskey='#hr_table_" + i + "']").hide();
            }
            for (i = 1; i < 10; i++) {
                $("[accesskey='#hr_table_" + i + "']").show();
            }
            int_zero = 1;
        }
        // 當頁數大於11時 建立前十頁的頁籤
        if (ipage >= 11) {
            if (!$('#hr_next_zero').html())
                $('#hr_page').prepend($('<li>', {
                    'class': "page-item"
                }).append($('<a>', {
                    'class': "page-link",
                    'html': '<<',
                    'id': 'hr_next_zero',
                    'onclick': "page_click('un')"
                })))
            int_zero = ipage - 10;
        }
        int_next_ten = ipage + 10;
        // 當按下下十頁或上十頁時
    } else {
        // 先判斷是上十頁還是下十頁 並且取相對應的頁數
        var page = ipage === 'up' ? int_next_ten : int_zero;
        for (i = 0; i <= int_hr_page; i++) {
            $("[accesskey='#hr_table_" + i + "']").hide();
        }
        for (i = page; i < page + 10; i++) {
            $("[accesskey='#hr_table_" + i + "']").show();
        }
        if (!$('#hr_next_zero').html()) {
            $('#hr_page').prepend($('<li>', {
                'class': "page-item"
            }).append($('<a>', {
                'class': "page-link",
                'html': '<<',
                'id': 'hr_next_zero',
                'onclick': "page_click('un')"
            })))
        }
        if (ipage == 'up') {
            int_next_ten += 10;
            int_zero = int_zero + 10;
        } else {
            int_zero = int_zero > 10 ? int_zero - 10 : 1;
            int_next_ten = int_zero + 10;
        }

    }
}
// 取得cookie的值
getCookie = function (cname) {
    var name = cname + "=";
    // 取得目前cookie內全部的值
    var ca = document.cookie.split(';');
    // 如果有符合cname的資料時 回傳值 否則回傳空字串
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
    }
    return "";
}

// 頁面上切換時 取消掉.active 並且對其都做一次mouseout
tab_open = function (tab) {
    $('.active').removeClass('active');
    if (tab == '#tab04') {
        window.open("http://10.0.101.111/smartit5/"); //20230420將搜尋引擎改成線上報修
    } else if (tab == '#tab06') {
        window.open("https://meetingroom.skm.com.tw/"); //會議室位置請記得調整,原本 http://hq.skm.com.tw/meeting/Meet_Ad.asp?f=7 新 https://meetingroom.skm.com.tw/
    } else if (tab == '#tab07') {
        window.open("http://ep.skm.com.tw/");
    } else {
        if (tab == '#tab05') {
            $('#address').show();
            fptelRead();
        }
        window.location.href = window.location.origin + '/Hq' + tab
    }
    $('ul.tab-title li button').mouseout();
}

$(document).ready(function () {
    // 頁面載入完成後 將顏色替換成此頁顏色
    $('#left_btn table tbody tr td button').css('color', color);
    // tabs的隱藏跟顯示
    var $li = $('ul.tab-title li');
    $($li.eq(0).addClass('active').find('button').attr('id')).siblings('.tab-inner').hide();
    $li.click(function () {
        // 點擊tabs時 做畫面的隱藏跟顯示 並且傳送點閱率
        var $button = $(this).find('button');
        $($(this).find('button').attr('id')).show().siblings('.tab-inner').hide();
        $button.addClass('active');
        CountRead('HQ', ($(this).find('button').attr('id')) ? $(this).find('button').attr('id').substring(5) : '1');
    })

    // banner
    fpBannerRead();
    //人資園地
    fpHrHomeRead();
    // 人資部公告
    fpMainLeftRead();
    // 資訊部公告
    fpMainRightRead();
    // 安全停看聽
    fpSafeRead();
    // 網站連結
    fpWebConnectRead();
    // 電話速查表
    fptelRead();
    // 人資部公告(分頁)
    fpHrContent();
    // 資訊部公告(分頁)
    fpItConent();

     $('.marquee').marquee({
        //duration in milliseconds of the marquee
        duration: 10000,
        //gap in pixels between the tickers
        gap: 50,
        //time in milliseconds before the marquee will start animating
        delayBeforeStart: 0,
        pauseOnHover : true,
        //'left' or 'right'
        direction: 'up',
        //true or false - should the marquee be duplicated to show an effect of continues flow
        duplicated: true,
        startVisible: true
     });

    //setInterval(function () {
    //    $('.marquee').marquee('toggle');
    //}, 1600);
});
$(window).load(function () {
    var cookie = getCookie('search');
    // 如果有#時 顯示其頁面
    if (window.location.href.indexOf('#') !== -1) {
        var tab = window.location.href.substring(window.location.href.indexOf('#'));
        $(tab).show().siblings('.tab-inner').hide();
    }
    // 如果cookie中有search的值時 做搜尋電話
    if (cookie) {
        $('#tel_search').val(cookie);
        onSearchTel();
        document.cookie = 'search=;Path=/;expires=' + Date(0);
        document.cookie = 'search=;Path=/Hq;expires=' + Date(0);
    }
    // 以下皆為輪播
    var slideShow = $("#banner"), //獲取最外層框架的名稱
        ul = $("#banner ul"),
        showNumber = ul.find(".banner_cls "),//獲取按鈕
        oneWidth = slideShow.find("ul li").eq(0).width(); //獲取每個圖片的寬度
    var timer = null; //定時器返回值，主要用於關閉定時器
    var iNow = 0; //iNow為正在展示的圖片索引值，當使用者開啟網頁時首先顯示第一張圖，即索引值為0

    timer = setInterval(function () { //開啟定時器
        iNow++;    //讓圖片的索引值次序加1，這樣就可以實現順序輪播圖片
        if (iNow > showNumber.length - 1) { //當到達最後一張圖的時候，讓iNow賦值為第一張圖的索引值，輪播效果跳轉到第一張圖重新開始
            iNow = 0;
        }
        showNumber.eq(iNow).click(); //模擬觸發數字按鈕的click
    }, 2000); //2000為輪播的時間

});