(function(window) {
    'use strict';

    window.libs = window.libs || {};

    // Modal 模組：處理 Bootstrap Modal 的開啟、關閉和內容載入
    (function() {
        const modalManager = {
            el: null,
            className: 'modal-dialog modal-lg',
            title: '',
            body: '',
            modalElement: null
        };

        function isJson(str) {
            try {
                JSON.parse(str);
                return true;
            } catch (e) {
                return false;
            }
        }

        function ensureModalExists() {
            let modalElement = document.getElementById('componentModal');
            
            if (!modalElement) {
                modalElement = document.createElement('div');
                modalElement.id = 'componentModal';
                modalElement.className = 'modal fade';
                modalElement.setAttribute('tabindex', '-1');
                modalElement.setAttribute('aria-labelledby', 'componentModalLabel');
                modalElement.setAttribute('aria-hidden', 'true');
                modalElement.innerHTML = `
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title" id="componentModalLabel"></h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body p-0" id="modal-body"></div>
                        </div>
                    </div>
                `;
                document.body.appendChild(modalElement);
            }

            if (!modalManager.el) {
                modalManager.modalElement = modalElement;
                modalManager.el = new bootstrap.Modal(modalElement);
            }

            return modalElement;
        }

        async function openModalCore(title, url) {
            try {
                let res;
                if (window.jQuery && window.jQuery.ajax) {
                    const response = await new Promise((resolve, reject) => {
                        window.jQuery.ajax({
                            url: url,
                            type: 'GET',
                            success: resolve,
                            error: reject
                        });
                    });
                    res = response;
                } else {
                    const response = await fetch(url);
                    res = await response.text();
                }

                if (isJson(res)) {
                    console.error('Response is JSON, not HTML content');
                    return;
                }

                const modalElement = ensureModalExists();
                const dialogElement = modalElement.querySelector('.modal-dialog');
                const titleElement = modalElement.querySelector('#componentModalLabel');
                const bodyElement = document.getElementById('modal-body');

                if (titleElement) {
                    titleElement.textContent = title;
                }

                if (dialogElement) {
                    dialogElement.className = modalManager.className;
                }

                if (bodyElement) {
                    bodyElement.innerHTML = '';
                    if (window.jQuery) {
                        window.jQuery(bodyElement).html(res);
                    } else {
                        bodyElement.innerHTML = res;
                    }
                }

                if (modalManager.el) {
                    modalManager.el.show();
                    
                    if (modalElement && window.jQuery) {
                        window.jQuery(modalElement).off('hidden.bs.modal');
                        window.jQuery(modalElement).on('hidden.bs.modal', function() {
                            window.libs.form && window.libs.form.rebindAjax();
                        });
                        
                        // 觸發 shown.bs.modal 事件，讓動態載入的內容可以初始化
                        window.jQuery(modalElement).one('shown.bs.modal', function() {
                            // 觸發自定義事件，讓載入的內容知道 modal 已顯示
                            window.dispatchEvent(new CustomEvent('modalContentLoaded'));
                        });
                        
                        // 如果 modal 已經顯示，立即觸發事件
                        if (window.jQuery(modalElement).hasClass('show')) {
                            setTimeout(function() {
                                window.jQuery(modalElement).trigger('shown.bs.modal');
                            }, 100);
                        }
                    }
                }
            } catch (error) {
                console.error('Error opening modal:', error);
                alert('載入內容時發生錯誤');
            }
        }

        async function openModalS(title, url) {
            modalManager.className = 'modal-dialog modal-sm';
            await openModalCore(title, url);
        }

        async function openModal(title, url) {
            modalManager.className = 'modal-dialog modal-lg';
            await openModalCore(title, url);
        }

        async function openModalL(title, url) {
            modalManager.className = 'modal-dialog modal-xl';
            await openModalCore(title, url);
        }

        async function openModalF(title, url) {
            modalManager.className = 'modal-dialog modal-fullscreen';
            await openModalCore(title, url);
        }

        function closeModal() {
            if (modalManager.el) {
                modalManager.el.hide();
            }
        }

        window.libs.modal = {
            openModalS: openModalS,
            openModal: openModal,
            openModalL: openModalL,
            openModalF: openModalF,
            closeModal: closeModal
        };
    })();

    // Form 模組：處理表單 AJAX 重新綁定
    (function() {
        function rebindAjax() {
            if (window.jQuery) {
                var $form = window.jQuery('#queryForm');
                if ($form.length > 0) {
                    $form.off('submit');
                    
                    var hasDataAjax = $form.attr('data-ajax') === 'true' || $form.data('ajax');
                    
                    if (hasDataAjax) {
                        $form.on('submit', function(e) {
                            e.preventDefault();
                            
                            var formData = $form.serialize();
                            var url = $form.attr('action') || '/Menu/IndexQuery';
                            var updateTargetId = $form.attr('data-ajax-update') || 
                                               $form.data('ajax-update') || 
                                               '#queryResult';
                            
                            window.jQuery.ajax({
                                url: url,
                                type: 'POST',
                                data: formData,
                                success: function(response) {
                                    window.jQuery(updateTargetId).html(response);
                                },
                                error: function() {
                                    alert('查詢時發生錯誤');
                                }
                            });
                            
                            return false;
                        });
                    }
                }
            }
        }

        window.libs.form = {
            rebindAjax: rebindAjax
        };
    })();

    // Date 模組：日期格式化工具
    (function() {
        function formatDate(date, format) {
            if (!(date instanceof Date)) {
                date = new Date(date);
            }
            
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            const seconds = String(date.getSeconds()).padStart(2, '0');
            
            if (format === 'YYYY-MM-DD HH:mm:ss') {
                return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
            } else if (format === 'YYYY-MM-DDTHH:mm:ss') {
                return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
            }
            return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
        }

        window.libs.date = {
            format: formatDate
        };
    })();

    // Loading 模組：載入指示器管理
    (function() {
        let loadingCount = 0;
        let loadingElement = null;

        function showLoading() {
            if (!loadingElement) {
                loadingElement = document.createElement('div');
                loadingElement.id = 'libs-loading';
                loadingElement.style.cssText = `
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background-color: rgba(0, 0, 0, 0.5);
                    z-index: 9999;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                `;
                loadingElement.innerHTML = `
                    <div class="spinner-border text-light" role="status" style="width: 3rem; height: 3rem;">
                        <span class="visually-hidden">載入中...</span>
                    </div>
                `;
                document.body.appendChild(loadingElement);
            }
            loadingCount++;
            loadingElement.style.display = 'flex';
        }

        function hideLoading() {
            loadingCount--;
            if (loadingCount <= 0) {
                loadingCount = 0;
                if (loadingElement) {
                    loadingElement.style.display = 'none';
                }
            }
        }

        function loading() {
            hideLoading();
        }

        window.libs.loading = {
            show: showLoading,
            hide: hideLoading,
            loading: loading
        };
    })();

    // API 模組：AJAX 請求處理（GET/POST）
    (function() {
        function formatDate(date, format) {
            if (!(date instanceof Date)) {
                date = new Date(date);
            }
            
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            const seconds = String(date.getSeconds()).padStart(2, '0');
            
            if (format === 'YYYY-MM-DD HH:mm:ss') {
                return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
            } else if (format === 'YYYY-MM-DDTHH:mm:ss') {
                return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
            }
            return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
        }

        async function get(url, options) {
            const startTime = new Date();

            if (!options) options = {};

            let loadding = options.loadding !== undefined ? options.loadding : true;

            let headers = options.headers || {};
            headers = Object.assign({
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'x-requested-with': 'XMLHttpRequest',
            }, headers);

            try {
                if (loadding) {
                    window.libs.loading.show();
                }

                const response = await fetch(url, {
                    method: 'GET',
                    headers: headers,
                    credentials: 'same-origin'
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const data = await response.json();
                return data;

            } catch (e) {
                console.log(e);
                window.libs.loading.loading();
                return { code: 0, message: '系統異常，請聯絡管理員' };
            } finally {
                if (loadding) {
                    window.libs.loading.hide();
                }

                const endTime = new Date();
                const duration = endTime - startTime;
                const s = formatDate(startTime, 'YYYY-MM-DD HH:mm:ss');
                const e = formatDate(endTime, 'YYYY-MM-DD HH:mm:ss');
                console.log(`${url}: ${duration}ms`);
            }
        }

        async function post(url, options) {
            const startTime = new Date();

            if (!options) options = {};
 
            let loadding = options.loadding !== undefined ? options.loadding : true;

            let body = Object.assign({}, options.data || {});

            let headers = options.headers || {};
            headers = Object.assign({
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'x-requested-with': 'XMLHttpRequest',
            }, headers);

            try {
                if (loadding) {
                    window.libs.loading.show();
                }

                const response = await fetch(url, {
                    method: 'POST',
                    headers: headers,
                    body: JSON.stringify(body),
                    credentials: 'same-origin'
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const data = await response.json();
                return data;

            } catch (e) {
                console.log(e);
                window.libs.loading.loading();
                return { code: 0, message: '系統異常，請聯絡管理員' };
            } finally {
                if (loadding) {
                    window.libs.loading.hide();
                }

                const endTime = new Date();
                const duration = endTime - startTime;
                const s = formatDate(startTime, 'YYYY-MM-DD HH:mm:ss');
                const e = formatDate(endTime, 'YYYY-MM-DD HH:mm:ss');
                console.log(`${url}: ${duration}ms`);
            }
        }

        window.libs.api = {
            get: get,
            post: post
        };
    })();

    window.libs.reload = function () {
        window.location.reload();
    };

    window.libs.result = function (res, callback) {
        if (res.code === 0) {
            alert(res.message)
        } else if (res.code === -1) {
            // Not Permission
        }
        else {
            if (callback) callback()
        }
    }

})(window);

// Pager 模組：處理分頁功能（定義在 IIFE 外部，確保全域可用）
window.pagerGoToPage = function(pageNo) {
    var form = document.getElementById('queryForm');
    if (form) {
        var pageNoInput = document.getElementById('PageNo');
        if (!pageNoInput) {
            pageNoInput = document.createElement('input');
            pageNoInput.type = 'hidden';
            pageNoInput.id = 'PageNo';
            pageNoInput.name = 'PageNo';
            form.appendChild(pageNoInput);
        }
        pageNoInput.value = pageNo;
    
        if (typeof query === 'function') {
            query();
        } else if (typeof jQuery !== 'undefined') {
            jQuery(form).submit();
        } else {
            form.submit();
        }
    }
};
