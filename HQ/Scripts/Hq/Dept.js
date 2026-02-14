// 存下載區資料位置
var dDownload = [];
// 分店公告內容
var dLetter = {
    "函": "article",
    "令": "BonusPenalty",
    "簡便行文": "SimpleNotes",
    "其他": "Other"
};
// 顏色區塊
var Color_Change = {
    "blue": "#D4EAFF",
    "skin": "#FEE0C4",
    "black": "#5D5D5D",
    "green": "#ADEA8B",
    "violet": "#DBCAFC",
    "pink": "#FFA4A2"
};
// 顏色區塊(背景)
var Color_Board = {
    "blue": "#99CDFF",
    "skin": "#FFCB99",
    "black": "#000000",
    "green": "#67CB33",
    "violet": "#9967FE",
    "pink": "#FFA4A2"
};

/// 目前在tabs li的位置上
/// 長出tabs page(up)
var fpTabPageContent = function (str) {
    // 去DeptController.cs 的TabsPage中取得資料(帶id)
    g_cus.CommonConnect(window.location.origin + "/api/Dept/TabsPage?id=" + str, {}, function (res) {
        // 先清空裡面的值
        $('#tabs li').remove();
        if (res.Table.length > 0) {
            // 當裡面有值時
            var $d_main = $("#tabs");
            var intTabNum = 0;
            res.Table.forEach(function (val, index) {
                $d_main.append($('<li>', {
                }).append($('<button>', {
                    'id': val.url ? val.subject === '電話速查表' ? '../Hq#tab05' : val.url : '#page_' + val.page,
                    'class': 'topbtn',
                    'html': val.subject
                }).hover(function () { if (str !== dept) { $(this).css('background-color', color); } }, function () { $(this).css('background-color', 'white'); })
                    .click(function () { val.subject == '電話速查表' || val.url ? window.location.href = (val.subject == '電話速查表' ? window.location.origin + '/Hq#tab05' : val.url) : null })))

                intTabNum++;
            });
            /*
             * 變更左側按鈕顏色
             * 標頭
             * 總公司
             * 按鈕
             * 底
             * 分公司
             * 左側按鈕換顏色
             */
            $('#b_top').css('background', 'url("/images/' + res.Table[0].style + '/bg_top.gif")');
            $('.td_lbtn_title').attr('src', "/images/" + res.Table[0].style + "/d_left/u_title.gif");
            $('.td_lbtn_vl_line').attr('background', "/images/" + res.Table[0].style + "/d_left/u_btn.gif");
            $('.td_lbtn_u_h').attr('background', "/images/" + res.Table[0].style + "/d_left1/u_h.gif");
            $('.tb_lbtn_store').attr('src', "/images/" + res.Table[0].style + "/d_left/u_title_store.gif");
            $('#' + str).attr('background', "/images/" + res.Table[0].style + "/d_left/u_btn_on.gif")
            color = Color_Change[res.Table[0].style];
            $('#tabs').css({ 'background-color': color, 'width': (126 * intTabNum) + 'px' });
            color = Color_Board[res.Table[0].style];
            $(".lbtn").css('border-color', color);
            $("button #" + str).css('background-color', color);
            $('#left_btn table tbody tr td button').css('color', color);
            $('#tabs li button').css('border-color', color);
            $('#left_btn table tbody tr td button').mouseout();
        }
    }, function () {
    })
};
///
/// page contnet
var fpPageContent = function (str) {
    // 從DeptController.cs的PageContent取資料(帶dept)
    g_cus.CommonConnect(window.location.origin + "/api/Dept/PageContent", { dept: str }, function (res) {
        $('#page_content').empty();
        dDownload = [];
        var page = null;
        var strDownloadPage = null;
        var newData = null;
        // 如果有回傳資料
        if (res.Table.length > 0) {
            var $div_tag = $('#page_content')
            res.Table.forEach(function (val, index) {
                var $div = null
                // tab還沒被建立
                if (page !== val.page) {
                    $div = $('<div>', {
                        'class': 'tab-inner',
                        'id': 'page_' + val.page
                    }).appendTo($div_tag)
                    page = val.page;

                    $('<div>', {
                        'id': 'content',
                        'class': 'left2'
                    }).appendTo($div)
                    $('<div>', {
                        'id': 'd_right',
                        'class': 'right'
                    }).appendTo($div)
                }
                if (val.page == '1000' && index < 1) {
                    /// 判斷是否有檔案(banner)                  
                    var client = new XMLHttpRequest();
                    client.onload = function () {
                        // in case of network errors this might not give reliable results
                        returnStatus(this.status, window.location.origin + '/ad/' + val.dept + '.swf');
                    }
                    //client.open("HEAD", window.location.origin + '/ad/' + val.dept + '.swf', true);
                    //client.send();
                }
                // 為d_main
                if (val.subtype.indexOf('d_main') != -1) {
                    // 建立外框
                    fpMainTableAdd('page_' + val.page + ' #content', val)
                    // 如果是function4.asp時
                    if (val.strif == 'function4.asp') {
                        fpLetter(val.page + val.subtype, val);
                    }
                    // 如果是系統通訊或公告時
                    if (val.unit_title == '系統通訊' || val.subject == '公文') {
                        fpSystemConnect(val.page + val.subtype, val)
                    }
                    // 如果是最新公告時
                    if (val.unit_title.indexOf('最新公告') !== -1 && dBranch[val.dept]) {
                        fpNew(val.page + val.subtype, val)
                    }
                }
                // 為d_right(長右邊區域)
                if (val.subtype.indexOf('d_right') !== -1) {
                    fpRightAdd('page_' + val.page + ' #d_right', val)
                    if (val.strif == 'downloadright.asp') {
                        newData = val.subtype;
                    }
                }
                // 為d_dw
                if (val.subtype.indexOf('d_dw') !== -1) {
                    if (val.page !== strDownloadPage) {
                        fpRightAdd('page_' + val.page + ' #d_right', val)
                        strDownloadPage = val.page;
                    }
                    if (val.url) {

                        dDownload[val.subject] = val.url;
                    }
                }

            });
            // 部門最新消息內容
            if (newData) {
                fpDeptNewsData(res.Table[0].dept, newData)
            }
            // 如果有下載區的資料時
            if (Object.keys(dDownload).length > 0) {
                fpDownloadAdd();
            }
            // tab的切換
            tag_click();
        }
    }, function () {
    })
};

/// banner 長出來 or 不長
function returnStatus(status, address) {
    // 如果裡面圖片位置正確時
    if (status === 200) {
        //$('#page_1000 #content').prepend($('<embed>', { 'src': address, 'width': "578", 'height': "120" }));
    }
    else {
        return null;
    }
}

///
///長出main表格
var fpMainTableAdd = function (id, content) {
    var $id = $('#' + id);
    $id.append($('<table>', {
        'style': content.unit_title !== '系統通訊' || content.subject !== '公文' ? 'width:571px' : 'width:753px',
        'border': "0",
        'cellpadding': "0",
        'cellspacing': "0",
        'class': 'd_main'
    }).append($('<tr>').append($('<td>', {
        'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.left_up_line + '");height:28px;width:8px',
    }),
        $('<td>', {
            'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.title_pic + '");width:553px;height: 28',
            'class': "fnt03"
        }).append($('<b>', {
            'html': content.unit_title !== '系統通訊' || content.subject !== '公文' ? content.unit_title : null,
        })), $('<td>', {
            'width': "10",
            'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.right_up_line + '")',
            'height': "28"
        })
    ),
        $('<tr>').append($('<td>', {
            'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.vl_line + '")',
            'html': '&nbsp'
        }), $('<td>', {
            'id': content.page + content.subtype,
            'html': content.strif !== 'function4.asp' && content.unit_title !== '系統通訊' || content.subject !== '公文' ? content.content : null
        }), $('<td>', {
            'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.vr_line + '")',
            'html': '&nbsp;'
        })
        ), $('<tr>').append($('<td>', {
            'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.left_down_line + '")',
            'width': "8",
            'height': "7"
        }), $('<td>', {
            'height': "7",
            'width': "571",
            'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.h_line + '")'
        }),
            $('<td>', {
                'style': 'background-image: url("/images/' + content.style + '/d_main/' + content.right_down_line + '")',
                'width': "10",
                'height': "7"
            })
        )
    ))
};

///
///長出右邊
var fpRightAdd = function (id, content) {
    //奕丞 2022/2/25 特別針對 顧客服務>教育訓練 頁籤進行調整，讓右側的單元選單移動至合適位置 
    var csSpecialStyle = "";
    if (dept == undefined || dept == null) {
        var qs = window.location.search || "";
        if (qs != "") {
            dept = qs.replace("?", "");
        }
    }
    if (dept != undefined && dept != null && dept == "cs") {
        if (content.page == "1040" && content.subtype == "d_right1" && content.unit_title == "單元選單") {
            csSpecialStyle = ";padding-left: 10px";
        }
    }
    var $id = $('#' + id);
    $id.append(
        $('<table>', {
            'border': "0",
            'cellpadding': "0",
            'cellspacing': "0",
            'class': "fnt01"
        }).append(
            $('<tr>').append(
                $('<td>', {
                    'style': 'background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.left_up_line + '"); width:13px;height: 30px' + csSpecialStyle,

                }),
                $('<th>', {
                    'html': content.unit_title,
                    'style': 'width:12px;height:30px;text-align:left;background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.title_pic + '")'
                }),
                $('<td>', {
                    'style': 'width:12px;height:30px;background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.right_up_line + '")' + csSpecialStyle

                })), $('<tr>').append(
                    $('<td>', {
                        'style': 'background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.vl_line + '")',
                        'html': '&nbsp;'
                    }), $('<td>', {
                        'class': content.subtype,
                        'html': '&nbsp;' + content.subtype == 'd_dw' ? '請選擇' : content.content
                    }), $('<td>', {
                        'style': 'background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.vr_line + '")',
                        'html': '&nbsp;'
                    })
                ), $('<tr>', { 'padding': '0', 'margin': '0' }).append(
                    $('<td>', {
                        'style': 'height: 8px; background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.left_down_line + '");width:13px'
                    }),
                    $('<td>', {
                        'style': 'height: 8px; background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.h_line + '");width:162px'
                    }),
                    $('<td>', {
                        'style': 'height: 8px; background-image: url("/images/' + content.style + '/' + content.subtype + '/' + content.right_down_line + '");width:12px'
                    })
                )

        )
    )

    if (csSpecialStyle != "") {
        $id.removeClass("right");
        $id.addClass("right_cs_special");
    }
};

///
/// 長下載區資料
var fpDownloadAdd = function () {
    let $dw = $(".d_dw");
    $dw.append($('<select>', {
        'onChange': "dw(this);",
        'class': 'd_select',
        'style': 'width:160px',
        'id': 'd_dw'
    }).append($('<option>', {
        'value': "0",
        'html': '請選擇...',
    })));
    Object.keys(dDownload).forEach(function (key, index) {
        $('.d_select').append($('<option>', {
            'value': dDownload[key],
            'html': key
        }))
    })

};


// 分頁切換(上下十頁)
page_click = function (id, ipage) {
    // 當按下頁碼時
    if (typeof (ipage) == 'number') {
        if (ipage > 5) {
            for (i = 0; i < 100; i++) {
                $("[href='" + id + i + "']").hide();
            }
            for (i = ipage - 5; i < ipage + 5; i++) {
                $("[href='" + id + i + "']").show();
            }
        } else {
            for (i = 0; i < 100; i++) {
                $("[href='" + id + i + "']").hide();
            }
            for (i = 1; i <= 10; i++) {
                $("[href='" + id + i + "']").show();
            }
        }
    }
}


///
/// 部門最新消息內容
var fpDeptNewsData = function (dept, id) {
    // 到DeptController.cs中的DeptNewData中取資料(帶dept)
    g_cus.CommonConnect(window.location.origin + "/api/Dept/DeptNewData", { dept: dept }, function (res) {
        if (res.Table.length > 0) {
            var $id = $('.' + id);
            $id.empty();
            // 清空原本內容 開始建立輪播
            var $marquee = $('<marquee>', {
                'onmouseover': "this.stop()",
                'onmouseout': "this.start()",
                'direction': "up",
                'scrollamount': "1",
                'behavior': "scroll",
                'style': 'width: 160px;height: 137px;'
            }).appendTo($('<div>', {
                'style': 'width:160px;height:137px',
                'align': 'center'
            }).appendTo($id))
            res.Table.forEach(function (val) {
                $marquee.append($('<a>', {
                    'html': '◎' + val.descpt,
                    'href': val.urlpath,
                    'target': "_blank"
                }).append("<br />"))
            })
        }
    }, function () {
    })
};


///
/// 撈函令等內容(有頁籤)
var fpLetterContent = function (dept, letter, id) {
    // 到DeptController.cs中的LetterData中取資料(帶dept letter)
    g_cus.CommonConnect(window.location.origin + "/api/Dept/LetterData", { dept: dept, letter: letter }, function (res) {
        if (res.Table != undefined) {
        if (res.Table.length > 1) {
            // 當裡面有值時
            var $d_main_l1 = $("#" + id);
            var table_page = 0;
            var $table_page = null;
            var $id = $('#' + id);
            // 建立顯示區域
            var $tableContent = $('<tbody>').appendTo($('<table>', {
                'class': "fnt03",
                'style': 'width: 575px;align:center; border: 2px #bbe9ff;',
                'border': '1px ',
                'cellspacing': '0',
                'cellpadding': '3'
            }).appendTo($('<div>', {
                'align': 'center',
                'style': 'font-size:medium'
            }).appendTo($id)));

            // 建立頁籤區域
            var $page_tab = $('<ul>', {
                'class': "pagination"
            }).appendTo($('<nav>', {
                'style': 'text-align:center',
                'aria-label': "Page navigation example"
            }).appendTo($('#' + id)));

            res.Table.forEach(function (val, index) {
                // 存頁數大小
                let a_page = Math.ceil((index + 1) / 8);
                if (table_page < a_page) {
                    // 如果頁數小於總頁數時 建立新頁籤及區域
                    $table_page = $('<td>', {
                        'scope': 'col',
                        'class': 'content_' + id,
                        'id': 'Branch_' + id + a_page,
                        'style': 'text-align:left'
                    }).appendTo($tableContent);
                    table_page = a_page;
                    $page_tab.append($('<li>', {
                        'class': "page-item"
                    }).append($('<a>', {
                        'class': "page-link",
                        'href': "#Branch_" + id + a_page,
                        'html': a_page,
                        'onclick': 'page_click( "#Branch_' + id + '",' + a_page + ')'
                    })))
                }
                // 建立其中資料
                $table_page.append(
                    $('<tr>').append($('<td>', {
                        'id': 'T11_1',
                        'style': 'border:1px #777b94 solid'
                    }).append($('<span>', {
                        'class': 'style2',
                        'html': val.edm_name
                    })), $('<td>', {
                        'id': 'T11_3',
                        'style': 'border:1px #777b94 solid'
                    }).append($('<a>', {
                        /*'href': ' http://od-paper.skm.com.tw/' + val.path.substring(0, val.path.length - 11) + '/wmx_edmimage/' + val.id + '/edm_content.htm',*/
                        'href': val.path,
                        'html': val.title,
                        'target': "_blank"
                    }))));
            })
            // 當頁數大於十頁時
            if (table_page > 10) {
                for (var i = 11; i <= table_page; i++) {
                    $("[href= '#Branch_" + id + i + "'").hide();
                }
            }
            // 對頁籤做隱藏及顯示
            var $Branch_li = $('#' + id + ' nav ul.pagination li');
            $($Branch_li.eq(0).addClass('active').find('a').attr('href')).siblings('.content_' + id).hide();
            // 按下頁籤的動作
            $Branch_li.click(function () {
                $($(this).find('a').attr('href')).show().siblings('.content_' + id).hide();
                $(this).addClass('active').siblings('.active').removeClass('active');
            });
        }
    }
    }, function () {
    })
};



///
/// 系統通訊 公文
fpSystem = function (dept, id) {
    // 判斷是哪個部門的 資訊部為系統通訊 人資部為公文
    var d_main = '1010d_main';
    var dept_data = 'com'
    if (dept == 'hr') {
        d_main = '1000d_main1';
        dept_data = dept;
    }
    // 去MainController.cs 取資料
    g_cus.CommonConnect(window.location.origin + "/api/Main/ItData", { dept: dept_data }, function (res) {
        if (res.Table.length > 1) {
            var $d_main_l1 = $("#it_content");
            var table_page = 0;
            var $table_page = null;
            // 建立頁籤區塊
            var $page_tab = $('<ul>', {
                'class': "pagination"
            }).appendTo($('<nav>', {
                'style': 'text-align:center',
                'aria-label': "Page navigation example"
            }).appendTo($('#' + d_main)))
            res.Table.forEach(function (val, index) {
                // 存頁籤大小
                let a_page = Math.ceil((index + 1) / 15);
                if (table_page < a_page) {
                    // 如果目前頁籤小於總頁籤時 建立新頁籤及新區塊
                    $table_page = $('<td>', {
                        'scope': 'col',
                        'class': 'it_content_page',
                        'id': 'it_table_' + a_page,
                        'style': 'text-align:left'
                    }).appendTo($d_main_l1.append($('<tb>')));
                    table_page = a_page;
                    $page_tab.append($('<li>', {
                        'class': "page-item",
                        'style': 'width:980px'
                    }).append($('<a>', {
                        'class': "page-link",
                        'href': "#it_table_" + a_page,
                        'html': a_page,
                        'onclick': 'page_click( "#it_table_",' + a_page + ')'
                    })))
                }
                // 內文
                $table_page.append(
                    $('<img>', {
                        'src': '/content_img/pic_2.gif',
                        'width': '12',
                        'height': '12',
                    }), $('<a>', {
                        'style': 'text-overflow : ellipsis; ',
                        'href': val.path,
                        'html': (dept == 'hr' ? val.title : val.t) + '(' + val.edm_name + ')',
                        'target': "_blank"
                    }).append('<p></p>'))
            })
            // 如果頁籤大於10時 將其他的頁籤做隱藏
            if (table_page > 10) {
                for (var i = 11; i <= table_page; i++) {
                    $("[href= '#it_table_" + i + "'").hide();
                }
            }
            // 頁籤的切換
            var $it_li = $('#' + d_main + ' nav ul.pagination li');
            $($it_li.eq(0).addClass('active').find('a').attr('href')).siblings('.it_content_page').hide();
            // 頁籤的動作
            $it_li.click(function () {
                $($(this).find('a').attr('href')).show().siblings('.it_content_page').hide();
                $(this).addClass('active').siblings('.active').removeClass('active');
            });
        }
    }, function () {
    })
};

///
/// 最新消息
fpNewData = function (dept, id) {
    // 從DeptController.cs的NewData中取資料(帶dept)
    g_cus.CommonConnect(window.location.origin + "/api/Dept/NewData", { dept: dept }, function (res) {
        if (res.Table.length > 1) {
            var $id = $('#' + id);
            var $tableContent = $('<tbody>').appendTo($('<table>', {
                'class': "fnt03",
                'style': 'width: 575px;align:center; border: 2px #bbe9ff;',
                'border': '1px ',
                'cellspacing': '0',
                'cellpadding': '3'
            }).appendTo($('<div>', {
                'align': 'center',
                'style': 'font-size:medium'
            }).appendTo($id)));

            res.Table.forEach(function (val) {
                $tableContent.append($('<tr>').append($('<td>', {
                    'id': 'T11_1'
                }).append($('<span>', {
                    'class': 'style2',
                    'html': val.edm_name
                })
                ), $('<td>', {
                    'id': 'T11_3'
                }).append($('<a>', {
                    /*'href': ' http://od-paper.skm.com.tw/' + val.strfilepath.substring(0, val.strfilepath.length - 11) + '/wmx_edmimage/' + val.id + '/edm_content.htm',*/
                    'href': val.strfilepath,
                    'html': val.title,
                    'target': "_blank"
                }))));

            })
            $('#' + id).append($('<div>', {
                'align': "right"
            }).append($('<a>', {
                'onclick': "onclick_tabs('page_1010')",
                'html': 'more'
            })))
        }
    }, function () {
    })
};

// 系統通訊
var fpSystemConnect = function (id, val) {
    $('#' + id).append($('<table>', {
        'border': "0",
        'cellpadding': "0",
        'cellspacing': "0"
    }).append($('<tr>').append($('<td>', {
        'style': 'width:11px;height:36px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_lu.gif")',
    }), $('<td>', {
        'style': 'width:87px;height:36px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_title.gif")',
        'html': '<b>' + val.subject + '</b>'
    }), $('<td>', {
        'style': 'width:474px;height:36px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_title.gif")',

    }), $('<td>', {
        'style': 'width:8px;height:36px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_ru.gif")',
    })), $('<tr>').append($('<td>', {
        'style': 'width:11px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_vl.gif")',
    }), $('<td>', {
        'colspan': '2'
    }).append($('<table>', {
        'border': "0",
        'cellpadding': "1",
        'class': "fnt09"
    }).append($('<tr>').append($('<td>', {
        'scope': "col",
        'id': "it_content",
    })))), $('<td>', {
        'style': 'width:8px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_vr.gif")',

    })), $('<tr>').append($('<td>', {
        'style': 'width:11px;height:8px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_ld.gif")',
    }), $('<td>', {
        'colspan': '2',
        'style': 'width:561px;height:8px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_h.gif")',
    }), $('<td>', {
        'style': 'width:8px;height:8px;background-image:url("/images/' + val.style + '/' + val.subtype + '/u_rd.gif")',
    }))
    ));
    fpSystem(val.dept, 'it_content');

};

///
/// 令 函等
var fpLetter = function (id, val) {
    fpLetterContent(dBranch[val.dept], dLetter[val.subject], id);
};
///
/// 最新公告
var fpNew = function (id, val) {
    fpNewData(dBranch[val.dept], id)
};


///
///下載檔案或顯示某網頁
var dw = function (node) {
    //let $dw = $("#d_dw")
    let $dw = $(node);
    if ($dw.val() == 0) { return; }
    window.open($dw.val(), '');
};

var tag_click = function () {
    var $li = $('ul.tab-title li');
    $($li.eq(0).find('button').attr('id')).siblings('.tab-inner').hide();
    $li.click(function () {
        $('#tabs li button').hover(
            function () {
                $(this).css('background-color', color);
            },
            function () {
                $(this).css('background-color', 'white');
            });

        $($(this).find('button').attr('id')).show().siblings('.tab-inner').hide();
        $('.active').removeClass('active');
        $(this).find('button').hover(
            function () {
                $(this).css('background-color', color);
            }, function () {
                $(this).css('background-color', color);
            });
        $('#tabs li button').mouseout();
        // 點閱率的增加
        CountRead(dept !== null ? dept : 'HQ', ($(this).find('button').attr('href')) ? $(this).find('button').attr('href').substring(6) : '1000');
    });

    if (dept == undefined || dept == null) {
        var qs = window.location.search || "";
        if (qs != "") {
            dept = qs.replace("?", "");
        }
    }
    if (dept == "gen") {
        $('#page_1010').hide();
        $('#page_1000').show();
        $('#page_1000').children('#d_right').hide();
    }
};
///
/// 跳tabs
var onclick_tabs = function (id) {
    $('#' + id).show().siblings('.tab-inner').hide();
};
function scrollToAnchor(aid) {
    $('[id="#page_1030"]').click();
    $('html, body').animate({
        scrollTop: $("#t" + aid + "").offset().top
    }, 200);
};
$(document).ready(function () {
    // 進入畫面時要先做的動作
    tag_click();
    clearAllCookie();
    // 如果有帶部門的值時
    if (window.location.href.indexOf('?') !== -1) {
        var tab = window.location.href.substring(window.location.href.indexOf('?') + 1);
        fpTabPageContent(tab);
        fpPageContent(tab);
    }

});